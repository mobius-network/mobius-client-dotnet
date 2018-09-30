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
        private Blockchain.Account _appAccount { get; set; }
        private Blockchain.Account _userAccount { get; set; }
        private Server _clientInstance { get; set; }
        public App(Blockchain.Account appAccount, Blockchain.Account userAccount)
        {
            _appAccount = appAccount;
            _userAccount = userAccount;
            _clientInstance = new Client().HorizonClient;
        }

        /**
        * @returns {boolean} true if developer is authorized to use an application
        */
        public Boolean authorized() {
            return this._userAccount.authorized(appKeypair());
        }

        /**
        * @returns {Account} app acount
        */
        public Blockchain.Account appAccount() {
            return this._appAccount;
        }

        /**
        * @returns {number} app balance
        */
        async public Task<decimal> appBalance() {
            return await _appAccount.balance();
        }

        /**
        * @returns {StellarSdk.Keypair} StellarSdk.Keypair object for app
        */
        public KeyPair appKeypair() {
            return this._appAccount.KeyPair();
        }

        /**
        * @returns {Account} user account
        */
        public Blockchain.Account userAccount() {
            return this._userAccount;
        }

        /**
        * @returns {number} user balance
        */
        async public Task<decimal> userBalance() {
            await _validateUserBalance();

            return await _userAccount.balance();
        }

        /**
        * @returns {StellarSdk.Keypair} StellarSdk.Keypair object for user
        */
        public KeyPair userKeypair() {
            return this._userAccount.KeyPair();
        }

        /**
        * Charges specified amount from user account and then optionally transfers
        * it from app account to a third party in the same transaction.
        * @param {number} amount - Payment amount
        * @param {?string} [destination] - third party receiver address
        * @returns {Promise}
        */
        async public Task<SubmitTransactionResponse> charge(decimal amount, KeyPair destination = null) {
            if (await userBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await _submitTx(tx => {
                tx.AddOperation(this._userPaymentOp(amount, appKeypair(), null).Result);

                if (destination != null) {
                    tx.AddOperation(this._appPaymentOp(amount, destination, null).Result);
                }
            });
        }

        /**
        * Sends money from the application account to the user or third party.
        * @param {number} amount - Payment amount
        * @param {string} [destination] - third party receiver address
        * @returns {Promise}
        */
        async public Task<SubmitTransactionResponse> payout(decimal amount, KeyPair destination = null) {
            if (destination == null) destination = userKeypair();

            if (await appBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this._submitTx(tx => {
                tx.AddOperation(_appPaymentOp(amount, destination, null).Result);
            });
        }

        /**
        * Sends money from the user account to the third party directly.
        * @param {number} amount - Payment amount
        * @param {string} destination - third party receiver address
        * @returns {Promise}
        */
        async public Task<SubmitTransactionResponse> transfer(decimal amount, KeyPair destination) {
            if (await userBalance() < amount) {
                throw new Exception("Insufficient Funds");
            }

            return await this._submitTx(tx => {
                tx.AddOperation(_userPaymentOp(amount, destination, null).Result);
            });
        }

        /**
        * @private
        * @param {transactionBuildFn} buildFn - callback to build the transaction
        * @returns {Promise} that resolves or rejects with response of horizon
        */
        async Task<SubmitTransactionResponse>_submitTx(Action<Transaction.Builder> buildFn) {
            Transaction.Builder builder = new Transaction.Builder(new stellar_dotnet_sdk.Account(userAccount().KeyPair(), null));
            buildFn(builder);
            Transaction tx = builder.Build();
            tx.Sign(appKeypair());

            SubmitTransactionResponse response = await this._clientInstance.SubmitTransaction(tx);

            await this._reload();

            return response;
        }

        /**
        * @private
        * @returns {Promise} to reload app and user accounts
        */
        async private Task<List<AccountResponse>> _reload() {
            List<AccountResponse> accounts = new List<AccountResponse>();
            AccountResponse appAccount = await this.appAccount().reload();
            AccountResponse userAccount = await this.userAccount().reload();

            accounts.Add(appAccount);
            accounts.Add(userAccount);

            return accounts;
        }

        /**
        * @private
        * @param {number} amount - payment amount
        * @param {string} destination - receiver address
        * @returns {Operation} payment operation
        */
        async private Task<Operation> _userPaymentOp(decimal amount, KeyPair destination, AssetResponse asset) {
            if (asset == null) asset = await new Client().StellarAsset();

            return new PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(userKeypair())
                .Build();
        }

        /**
        * @private
        * @param {number} amount - payment amount
        * @param {string} destination - receiver address
        * @returns {Operation} payment operation
        */
        async private Task<Operation> _appPaymentOp(decimal amount, KeyPair destination, AssetResponse asset = null) {
            if (asset == null) asset = await new Client().StellarAsset();

            return new PaymentOperation
                .Builder(destination, asset.Asset, amount.ToString())
                .SetSourceAccount(appKeypair())
                .Build();
        }

        /**
        * @private
        * @returns {boolean} true if developer is authorized to use an application and trustline exists
        */
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
