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

            Blockchain.Account account = await new AccountBuilder().Build(keypair);

            Stellar.Account _account = client.GetStellarAccount(account);

            Stellar.Transaction tx = _tx(_account, asset.Asset);

            tx.Sign(account.KeyPair().PrivateKey);

            return await client.HorizonClient.SubmitTransaction(tx);
        }

        ///<summary>Private: Generate changeTrust transaction with given parameters.</summary>
        ///<param name="account">Stellar account</param>
        ///<param name="asset">Stellar asset</param>
        ///<returns>Returns transaction.</returns>
        private Stellar.Transaction _tx(Stellar.Account account, Stellar.Asset asset) {
            string assetXdr = asset.ToXdr().ToString();
            string opXdrAmount = Stellar.Operation.ToXdrAmount(assetXdr).ToString();

            Stellar.ChangeTrustOperation operation = new Stellar.ChangeTrustOperation.Builder(asset, opXdrAmount).Build();

            return new Stellar.Transaction.Builder(account).AddOperation(operation).Build();
        }
    }
}
