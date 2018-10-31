using System;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;

namespace Mobius.Library.Blockchain
{
    public class CreateTrustline
    {
        ///<summary>Creates unlimited trustline for given asset.</summary>
        ///<param name="keypair">Account keypair</para>
        ///<param name="asset">(optional) - default to Client.StellarAsset()</param>
        ///<returns>Promise returns submitted transaction response.</returns>
        async public Task<StellarResponses.SubmitTransactionResponse> Call(Stellar.KeyPair keypair, StellarResponses.AssetResponse asset = null) {
            Client client = new Client();

            if (asset == null) asset = await client.StellarAsset();

            var account = await AccountBuilder.Build(keypair);

            var _account = client.GetStellarAccount(account);

            var tx = this.Tx(_account, asset.Asset);

            tx.Sign(account.KeyPair().PrivateKey);

            return await client.HorizonClient.SubmitTransaction(tx);
        }

        ///<summary>Private: Generate changeTrust transaction with given parameters.</summary>
        ///<param name="account">Stellar account</param>
        ///<param name="asset">Stellar asset</param>
        ///<returns>Returns transaction.</returns>
        private Stellar.Transaction Tx(Stellar.Account account, Stellar.Asset asset) {
            string assetXdr = asset.ToXdr().ToString();
            string opXdrAmount = Stellar.Operation.ToXdrAmount(assetXdr).ToString();

            var operation = new Stellar.ChangeTrustOperation.Builder(asset, opXdrAmount).Build();

            return new Stellar.Transaction.Builder(account).AddOperation(operation).Build();
        }
    }
}
