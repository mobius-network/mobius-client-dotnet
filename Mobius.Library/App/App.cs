using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mobius.Library.Blockchain;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;

namespace Mobius.Library.App
{
    public class App
    {
        private Blockchain.Account InnerAppAccount;
        private Blockchain.Account InnerUserAccount;
        private Stellar.Server ClientInstance;

        ///<summary>Build app instance</summary>
        ///<param name="appAccount">App account</param>
        ///<param name="userAccount">User account</param>
        ///<returns>Returns app instance</returns>
        public App(Blockchain.Account appAccount, Blockchain.Account userAccount)
        {
            this.InnerAppAccount = appAccount;
            this.InnerUserAccount = userAccount;
            this.ClientInstance = new Client().HorizonClient;
        }

        ///<summary>Check if developer is authorized to use an application</summary>
        ///<returns>Returns true if developer is authorized.</returns>
        public Boolean Authorized() {
            return this.InnerUserAccount.Authorized(AppKeypair());
        }

        ///<summary>Get the developer app account (developers account)</summary>
        ///<returns>Returns app acount</returns>
        public Blockchain.Account AppAccount() {
            return this.InnerAppAccount;
        }

        ///<summary>Get the balance of the app (developers account)</summary>
        ///<returns>Returns Promise - app balance</returns>
        public decimal AppBalance() {
            return this.InnerAppAccount.Balance().Result;
        }

        ///<summary>Get the developer app keypair.</summary>
        ///<returns>sReturns keypair object for app</returns>
        public Stellar.KeyPair AppKeypair() {
            return this.InnerAppAccount.KeyPair();
        }

        ///<returns>Returns the users account</returns>
        public Blockchain.Account UserAccount() {
            return this.InnerUserAccount;
        }

        ///<returns>Promise returns the users balance</returns>
        public decimal UserBalance() {
            this.ValidateUserBalance();

            return this.InnerUserAccount.Balance().Result;
        }

        ///<returns>Returns keypair object for user</returns>
        public Stellar.KeyPair UserKeypair() {
            return this.InnerUserAccount.KeyPair();
        }

        ///<summary>Charges specified amount from user account and then optionally transfers
        /// it from app account to a third party in the same transaction.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">(optional) Third party receiver address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<StellarResponses.SubmitTransactionResponse> Charge(decimal amount, string destination = null) {
            if (UserBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }


            return await SubmitTx(tx => {
                tx.AddOperation(this.UserPaymentOp(amount, AppKeypair()).Result);

                if (destination != null) {
                    Stellar.KeyPair target = Stellar.KeyPair.FromAccountId(destination);

                    tx.AddOperation(this.AppPaymentOp(amount, target).Result);
                }
            });
        }

        ///<summary>Sends money from the application account to the user or third party.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Third party receiver address - default, user address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<StellarResponses.SubmitTransactionResponse> Payout(decimal amount, Stellar.KeyPair destination = null) {
            if (destination == null) destination = UserKeypair();

            if (AppBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this.SubmitTx(tx => {
                tx.AddOperation(this.AppPaymentOp(amount, destination).Result);
            });
        }

        ///<summary>Sends money from the user account to the third party directly.</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Third party receiver address</param>
        ///<returns>Promise returns response object of transaction</returns>
        async public Task<StellarResponses.SubmitTransactionResponse> Transfer(decimal amount, Stellar.KeyPair destination) {
            if (UserBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this.SubmitTx(tx => {
                tx.AddOperation(this.UserPaymentOp(amount, destination).Result);
            });
        }

        ///<summary>Submit transaction</summary>
        ///<param name="buildFn">Callback to build the transaction</param>
        ///<returns>Promise that resolves or rejects with response of horizon</returns>
        async public Task<StellarResponses.SubmitTransactionResponse> SubmitTx(Action<Stellar.Transaction.Builder> buildFn) {
            Stellar.Transaction.Builder builder = new Stellar.Transaction.Builder(new Stellar.Account(UserAccount().KeyPair(), null));

            buildFn(builder);
            
            Stellar.Transaction tx = builder.Build();
            tx.Sign(AppKeypair());

            StellarResponses.SubmitTransactionResponse response = await this.ClientInstance.SubmitTransaction(tx);

            await this.Reload();

            return response;
        }

        ///<summary>Private: Reload the user and app accounts</summary>
        ///<returns>Promise, Returns reloaded app and user accounts</returns>
        async private Task<List<StellarResponses.AccountResponse>> Reload() {
            List<StellarResponses.AccountResponse> accounts = new List<StellarResponses.AccountResponse>();

            StellarResponses.AccountResponse appAccount = await this.InnerAppAccount.Reload();
            StellarResponses.AccountResponse userAccount = await this.InnerUserAccount.Reload();

            accounts.Add(appAccount);
            accounts.Add(userAccount);

            return accounts;
        }

        ///<summary>Private: User payment operation</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Receiver keypair</param>
        ///<returns>Promise returns payment operation</returns>
        async private Task<Stellar.Operation> UserPaymentOp(decimal amount, Stellar.KeyPair destination) {
            StellarResponses.AssetResponse asset = await new Client().StellarAsset();

            return new Stellar.PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(UserKeypair())
                .Build();
        }

        ///<summary>Private: App payment operation</summary>
        ///<param name="amount">Payment amount</param>
        ///<param name="destination">Receiver keypair</param>
        ///<returns>Promise returns payment operation</returns>
        async private Task<Stellar.Operation> AppPaymentOp(decimal amount, Stellar.KeyPair destination) {
            StellarResponses.AssetResponse asset = await new Client().StellarAsset();

            return new Stellar.PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(AppKeypair())
                .Build();
        }

        ///<summary>Private: Check developer authorization and trustline.</summary>
        ///<returns>Promise returns true if developer is authorized to use an application and trustline exists</returns>
        private Boolean ValidateUserBalance() {
            if (!Authorized()) {
                throw new Exception("Authorisation missing");
            }

            if (UserAccount().TrustlineExists().Result != true) {
                throw new Exception("Trustline not found");
            }

            return true;
        }
    }
}
