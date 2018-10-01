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
        private AccountResponse _account;
        private Stellar.KeyPair _keypair;
        private Dictionary<string, string> _assetIssuers;
        private Stellar.Server _clientInstance;

        ///<param name="account">Stellar AccountResponse instance</param>
        ///<param name="keypair">Account keypair</param>
        public Account(AccountResponse account, Stellar.KeyPair keypair)
        {
            _account = account;
            _keypair = keypair;
            _assetIssuers = new Dictionary<string, string>();
            _clientInstance = new Client().HorizonClient;
        }

        ///<returns>Keypair for account</returns>
        public Stellar.KeyPair KeyPair()
        {
            return _keypair;
        }

        ///<returns>Account info</returns>
        public AccountResponse Info()
        {
            return _account;
        }

        ///<param name="toKeypair">Keypair to check</param>
        ///<returns>Returns true if given keypair is added as cosigner to current account.</returns>
        public Boolean authorized(Stellar.KeyPair toKeypair) 
        {
            stellar_dotnet_sdk.responses.Signer signer = _findSigner(toKeypair.PublicKey.ToString());

            return signer != null ? true : false;
        }

        ///<param name="asset">Asset to check balance of</param>
        ///<returns>Promise returns balance of given asset - default Client.StellarAsset()</returns>
        async public Task<decimal> balance(AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            Balance balance = _findBalance(asset);

            return decimal.Parse(balance.BalanceString);
        }

        ///<param name="asset">Asset to check for trustline of</param>
        ///<returns>Promise returns true if trustline exists for given asset and limit is positive.</returns>
        async public Task<Boolean> trustlineExists(AssetResponse asset = null) 
        {
            if (asset == null) asset = await new Client().StellarAsset();

            Balance balance = _findBalance(asset);

            decimal limit = decimal.Parse(balance.Limit);

            return limit > 0;
        }

        ///<summary>Invalidates current account information.</summary>
        ///<returns>Promise returns reloaded account.</returns>
        async public Task<AccountResponse> reload() 
        {
            _account = null;

            byte[] accountId = _keypair.PublicKey;

            Uri uri = new Uri($"/accounts/{accountId}");

            _account = await _clientInstance.Accounts.Account(uri);

            return _account;
        }

        ///<summary>Private: check if balance matches a given asset.</summary>
        ///<param name="asset">Asset to compare</param>
        ///<param name="balance">Balance entry to compare</param>
        ///<returns>Returns true if balance matches with given asset.</returns>
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

        ///<summary>Private: find balance of asset.</summary>
        ///<param name="asset">Asset to find balance of</param>
        ///<returns>Returns matched balance.</returns>
        private Balance _findBalance(AssetResponse asset) 
        {
            return _account.Balances.Where(b => _balanceMatches(asset, b)).FirstOrDefault();
        }

        ///<summary>Private: find signer of account.</summary>
        ///<param name="publicKey">Signer's public key to find</param>
        ///<returns>Returns matched signer.</returns>
        private stellar_dotnet_sdk.responses.Signer _findSigner(string publicKey) 
        {
            return _account.Signers.Where(s => s.AccountId == publicKey).FirstOrDefault();
        }
    }
}
