using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;
using StellarRequests = stellar_dotnet_sdk.requests;
using StellarResponses = stellar_dotnet_sdk.responses;
using StellarResponsesPage = stellar_dotnet_sdk.responses.page;

namespace Mobius.Library.Blockchain
{
    public class Account
    {
        private StellarResponses.AccountResponse InnerAccount;
        private Stellar.KeyPair InnerKeyPair;
        private Dictionary<string, string> AssetIssuers;
        private Stellar.Server ClientInstance;

        ///<param name="account">Stellar AccountResponse instance</param>
        ///<param name="keypair">Account keypair</param>
        public Account(StellarResponses.AccountResponse account, Stellar.KeyPair keypair)
        {
            this.InnerAccount = account;
            this.InnerKeyPair = keypair;
            this.AssetIssuers = new Dictionary<string, string>();
            this.ClientInstance = new Client().HorizonClient;
        }

        ///<returns>Keypair for account</returns>
        public Stellar.KeyPair KeyPair()
        {
            return this.InnerKeyPair;
        }

        ///<returns>Account info</returns>
        public StellarResponses.AccountResponse Info()
        {
            return this.InnerAccount;
        }

        ///<param name="toKeypair">Keypair to check</param>
        ///<returns>Returns true if given keypair is added as cosigner to current account.</returns>
        public Boolean Authorized(Stellar.KeyPair toKeypair) 
        {
            var signer = this.FindSigner(toKeypair.AccountId);

            return signer != null ? true : false;
        }

        ///<param name="asset">Asset to check balance of</param>
        ///<returns>Promise returns balance of given asset - default Client.StellarAsset()</returns>
        async public Task<decimal> Balance(StellarResponses.AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            var balance = this.FindBalance(asset);

            return decimal.Parse(balance.BalanceString);
        }

        ///<param name="asset">Asset to check for trustline of</param>
        ///<returns>Promise returns true if trustline exists for given asset and limit is positive.</returns>
        async public Task<Boolean> TrustlineExists(StellarResponses.AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            var balance = this.FindBalance(asset);

            decimal limit = decimal.Parse(balance.Limit);

            return limit > 0;
        }

        ///<summary>Invalidates current account information.</summary>
        ///<returns>Promise returns reloaded account.</returns>
        async public Task<StellarResponses.AccountResponse> Reload() 
        {
            this.InnerAccount = null;

            this.InnerAccount = await this.ClientInstance.Accounts.Account(this.InnerKeyPair);

            return this.InnerAccount;
        }

        ///<summary>Private: check if balance matches a given asset.</summary>
        ///<param name="asset">Asset to compare</param>
        ///<param name="balance">Balance entry to compare</param>
        ///<returns>Returns true if balance matches with given asset.</returns>
        private Boolean BalanceMatches(StellarResponses.AssetResponse asset, StellarResponses.Balance balance) 
        {
            string assetType = balance.AssetType;
            string assetCode = balance.AssetCode;
            string assetIssuer = balance.AssetIssuer.AccountId;

            if (asset.Asset.GetType() == "native") 
                return assetType == "native";

            if (this.AssetIssuers.ContainsKey(assetCode) == false) 
                this.AssetIssuers[assetCode] = asset.AssetIssuer;

            return (
                assetCode == asset.AssetCode && 
                assetIssuer == this.AssetIssuers[assetCode]
            );
        }

        ///<summary>Private: find balance of asset.</summary>
        ///<param name="asset">Asset to find balance of</param>
        ///<returns>Returns matched balance.</returns>
        private StellarResponses.Balance FindBalance(StellarResponses.AssetResponse asset) 
        {
            return this.InnerAccount.Balances.Where(b => this.BalanceMatches(asset, b)).FirstOrDefault();
        }

        ///<summary>Private: find signer of account.</summary>
        ///<param name="publicKey">Signer's public key to find</param>
        ///<returns>Returns matched signer.</returns>
        private StellarResponses.Signer FindSigner(string publicKey) 
        {
            return this.InnerAccount.Signers.Where(s => s.AccountId == publicKey).FirstOrDefault();
        }
    }
}
