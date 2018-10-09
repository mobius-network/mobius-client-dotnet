using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobius.Library.Blockchain;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;

namespace Mobius.Library.App
{
    public class App
    {
        private Blockchain.Account _appAccount;
        private Blockchain.Account _userAccount;
        private Server _clientInstance;

        ///<summary>Build app instance</summary>
        ///<param name="appAccount">App account</param>
        ///<param name="userAccount">User account</param>
        ///<returns>Returns app instance</returns>
        public App(Blockchain.Account appAccount, Blockchain.Account userAccount)
        {
            _appAccount = appAccount;
            _userAccount = userAccount;
            _clientInstance = new Client().HorizonClient;
        }

        ///<summary>Check if developer is authorized to use an application</summary>
        ///<returns>Returns true if developer is authorized.</returns>
        public Boolean authorized() {
            return this._userAccount.authorized(appKeypair());
        }

        ///<summary>Get the developer app account (developers account)</summary>
        ///<returns>Returns app acount</returns>
        public Blockchain.Account appAccount() {
            return this._appAccount;
        }

        ///<summary>Get the balance of the app (developers account)</summary>
        ///<returns>Returns Promise - app balance</returns>
        async public Task<decimal> appBalance() {
            return await _appAccount.balance();
        }

        ///<summary>Get the developer app keypair.</summary>
        ///<returns>sReturns keypair object for app</returns>
        public KeyPair appKeypair() {
            return this._appAccount.KeyPair();
        }

        ///<returns>Returns the users account</returns>
        public Blockchain.Account userAccount() {
            return this._userAccount;
        }

        ///<returns>Promise returns the users balance</returns>
        async public Task<decimal> userBalance() {
            await _validateUserBalance();

            return await _userAccount.balance();
        }

        ///<returns>Returns keypair object for user</returns>
        public KeyPair userKeypair() {
            return this._userAccount.KeyPair();
        }

        ///<summary>Charges specified amount from user account and then optionally transfers
        /// it from app account to a third party in the same transaction.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">(optional) Third party receiver address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<SubmitTransactionResponse> charge(decimal amount, KeyPair destination = null) {
            if (await userBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await _submitTx(tx => {
                tx.AddOperation(this._userPaymentOp(amount, appKeypair()).Result);

                if (destination != null) {
                    tx.AddOperation(this._appPaymentOp(amount, destination).Result);
                }
            });
        }

        ///<summary>Sends money from the application account to the user or third party.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Third party receiver address - default, user address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<SubmitTransactionResponse> payout(decimal amount, KeyPair destination = null) {
            if (destination == null) destination = userKeypair();

            if (await appBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this._submitTx(tx => {
                tx.AddOperation(_appPaymentOp(amount, destination).Result);
            });
        }

        ///<summary>Sends money from the user account to the third party directly.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Third party receiver address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<SubmitTransactionResponse> transfer(decimal amount, KeyPair destination) {
            if (await userBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this._submitTx(tx => {
                tx.AddOperation(_userPaymentOp(amount, destination).Result);
            });
        }

        ///<summary>Submit transaction</summary>
        ///<param name="buildFn">Callback to build the transaction</param>
        ///<returns>Promise that resolves or rejects with response of horizon</returns>
        async Task<SubmitTransactionResponse>_submitTx(Action<Transaction.Builder> buildFn) {
            Transaction.Builder builder = new Transaction.Builder(new stellar_dotnet_sdk.Account(userAccount().KeyPair(), null));

            buildFn(builder);
            
            Transaction tx = builder.Build();
            tx.Sign(appKeypair());

            SubmitTransactionResponse response = await this._clientInstance.SubmitTransaction(tx);

            await this._reload();

            return response;
        }

        ///<summary>Private: Reload the user and app accounts</summary>
        ///<returns>Promise, Returns reloaded app and user accounts</returns>
        async private Task<List<AccountResponse>> _reload() {
            List<AccountResponse> accounts = new List<AccountResponse>();

            AccountResponse appAccount = await this.appAccount().reload();
            AccountResponse userAccount = await this.userAccount().reload();

            accounts.Add(appAccount);
            accounts.Add(userAccount);

            return accounts;
        }

        ///<summary>Private: User payment operation</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Receiver keypair</param>
        ///<returns>Promise returns payment operation</returns>
        async private Task<Operation> _userPaymentOp(decimal amount, KeyPair destination) {
            AssetResponse asset = await new Client().StellarAsset();

            return new PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(userKeypair())
                .Build();
        }

        ///<summary>Private: App payment operation</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Receiver keypair</param>
        ///<returns>Promise returns payment operation</returns>
        async private Task<Operation> _appPaymentOp(decimal amount, KeyPair destination) {
            AssetResponse asset = await new Client().StellarAsset();

            return new PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(appKeypair())
                .Build();
        }

        ///<summary>Private: Check developer authorization and trustline.</summary>
        ///<returns>Promise returns true if developer is authorized to use an application and trustline exists</returns>
        async private Task<Boolean> _validateUserBalance() {
            if (!authorized()) {
                throw new Exception("Authorisation missing");
            }

            if (await userAccount().trustlineExists() != true) {
                throw new Exception("Trustline not found");
            }

            return true;
        }
    }
}