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

            // Fixed for now....
            Uri uri = new Uri($"https://horizon-testnet.stellar.org/accounts/{accountId}");

            Stellar.Server server = new Client().HorizonClient;

            StellarResponses.AccountResponse account = await server.Accounts.Account(uri);

            return new Account(account, keypair);
        }   
    }
}
