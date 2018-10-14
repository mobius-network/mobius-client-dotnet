using System;
using System.Net.Http;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;

namespace Mobius.Library.Blockchain
{
    public class AccountBuilder
    {
        ///<summary>Get account information from Stellar network and returns an instance of Account</summary>
        ///<returns>Promise returns Blockchain.Accont instance</returns>
        async public Task<Account> Build(Stellar.KeyPair keypair)
        {
            string accountId = Stellar.StrKey.EncodeStellarAccountId(keypair.PublicKey);

            // URI must manually be built up for now due to bug in stellar_dotnet_sdk.
            // Commit for bug pushed here: https://github.com/elucidsoft/dotnet-stellar-sdk/commit/e1577f423e8de8bea5ad007de08a4e464cf0684f
            // Upon next release server.Accounts.Account() will not need to supply the entire URI.
            Client client = new Client();
            string endpoint = Stellar.Network.IsPublicNetwork(client.Network) 
                ? Client.Urls["PUBLIC"]
                : Client.Urls["TESTNET"];

            Uri uri = new Uri($"{endpoint}/{accountId}");

            StellarResponses.AccountResponse account = await client.HorizonClient.Accounts.Account(uri);

            return new Account(account, keypair);
        }   
    }
}
