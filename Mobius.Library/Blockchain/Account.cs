using System;
using Stellar = stellar_dotnet_sdk;
using stellar_dotnet_sdk.requests;
using stellar_dotnet_sdk.responses;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using stellar_dotnet_sdk.responses.page;
using System.Text;
using stellar_dotnet_sdk;

namespace Mobius.Library.Blockchain
{
    public class Account
    {
        private AccountResponse _account { get; set; }
        private Stellar.KeyPair _keypair { get; set; }
        private Dictionary<string, string> _assetIssuers { get; set; }
        private Stellar.Server _clientInstance { get; set; }
        public Account(AccountResponse account, Stellar.KeyPair keypair)
        {
            _account = account;
            _keypair = keypair;
            _assetIssuers = new Dictionary<string, string>();
            _clientInstance = new Client().HorizonClient;
        }

        public Stellar.KeyPair KeyPair()
        {
            return _keypair;
        }

        public AccountResponse Info()
        {
            return _account;
        }

        ///<param name="toKeypair"> {StellarSdk.Keypair} toKeypair</param>
        ///<returns> {boolean} true if given keypair is added as cosigner to current account</returns>
        public Boolean authorized(Stellar.KeyPair toKeypair) 
        {
			stellar_dotnet_sdk.responses.Signer signer = _findSigner(toKeypair.PublicKey.ToString());

            return signer != null ? true : false;
        }

        ///<param name="asset"> {StellarSdk.Asset} [asset=Client.stellarAsset]</param>
        ///<returns> {number} balance for given asset</returns>
        async public Task<decimal> balance(AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            Balance balance = _findBalance(asset);

            return decimal.Parse(balance.BalanceString);
        }

        ///<param name="asset"> {StellarSdk.Asset}</param>
        ///<returns> {boolean} true if trustline exists for given asset and limit is positive</returns>
        async public Task<Boolean> trustlineExists(AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            Balance balance = _findBalance(asset);

            decimal limit = decimal.Parse(balance.Limit);

            return limit > 0;
        }

        ///<summary>>Invalidates current account information.</summary>
        ///<returns> {Promise}</returns>
        async public Task<AccountResponse> reload() 
        {
            _account = null;
    
            byte[] accountId = _keypair.PublicKey;
            
            Uri uri = new Uri($"/accounts/{accountId}");

            _account = await _clientInstance.Accounts.Account(uri);

            return _account;
        }

        ///<summary>Private</summary>
        ///<param name="asset"> {StellarSdk.Asset} asset - Asset to compare</param>
        ///<param name="balance"> {any} balance - balance entry to compare</param>
        ///<returns> {boolean} true if balance matches with given asset</returns>
        private Boolean _balanceMatches(AssetResponse asset, Balance balance) 
        {
            string assetType = balance.AssetType;
            string assetCode = balance.AssetCode;
            string assetIssuer = balance.AssetIssuer.AccountId;

            if (asset.Asset.GetType() == "native") 
                return assetType == "native";

            if (_assetIssuers.ContainsKey(assetCode) == false) 
                _assetIssuers[assetCode] = asset.AssetIssuer;

            return (
                assetCode == asset.AssetCode && 
                assetIssuer == _assetIssuers[assetCode]
            );
        }

        ///<summary>Private</summary>
        ///<param name="asset"> {StellarSdk.Asset} asset - Asset to find</param>
        ///<returns> {object} matched balance</returns>
        private Balance _findBalance(AssetResponse asset) 
        {
            return _account.Balances.Where(b => _balanceMatches(asset, b)).FirstOrDefault();
        }

        ///<summary>Private</summary>
        ///<param name="publicKey"> {string} publicKey - signer's key to find</param>
        ///<returns> {object} matched signer</returns>
        private stellar_dotnet_sdk.responses.Signer _findSigner(string publicKey) 
        {
            return _account.Signers.Where(s => s.AccountId == publicKey).FirstOrDefault();
        }
    }
}
