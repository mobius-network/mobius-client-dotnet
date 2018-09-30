using System;
using System.Threading.Tasks;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;

namespace Mobius.Library.Blockchain
{
    public class CreateTrustline
    {
        ///<summary>Creates unlimited trustline for given asset.</summary>
        ///<param name="keypair">KeyPair: keypair - Account keypair</para>
        ///<param name="asset">AssetResponse: asset - optional, default to Client.StellarAsset</param>
        ///<returns>Promise = SubmitTransactionResponse: submitted transaction response</returns>
        async public Task<SubmitTransactionResponse> Call(KeyPair keypair, AssetResponse asset = null) {
            Client client = new Client();

            if (asset == null) asset = await client.StellarAsset();

            Blockchain.Account account = await new AccountBuilder().Build(keypair);

            stellar_dotnet_sdk.Account _account = client.GetStellarAccount(account);

            Transaction tx = _tx(_account, asset.Asset);

            tx.Sign(account.KeyPair().PrivateKey);

            return await client.HorizonClient.SubmitTransaction(tx);
        }

        ///<summary>Private: Generate changeTrust transaction with given parameters.</summary>
        ///<param name="account">stellar_dotnet_sdk.Account: account</param>
        ///<param name="asset">stellar_dotnet_sdk.Asset: asset</param>
        ///<returns>stellar_dotnet_sdk.Transaction: stellar transaction</returns>
        private Transaction _tx(stellar_dotnet_sdk.Account account, Asset asset) {
            string assetXdr = asset.ToXdr().ToString();
            string opXdrAmount = Operation.ToXdrAmount(assetXdr).ToString();

            ChangeTrustOperation operation = new ChangeTrustOperation.Builder(asset, opXdrAmount).Build();

            return new Transaction.Builder(account).AddOperation(operation).Build();
        }
    }
}
