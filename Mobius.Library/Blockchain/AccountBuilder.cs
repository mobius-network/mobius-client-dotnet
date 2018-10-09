using System;
using System.Net.Http;
using System.Threading.Tasks;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;

namespace Mobius.Library.Blockchain
{
    public class AccountBuilder
    {
        ///<summary>Get account information from Stellar network and returns an instance of Account</summary>
        ///<returns>Promise returns Blockchain.Accont instance</returns>
        async public Task<Account> Build(KeyPair keypair)
        {
            string accountId = StrKey.EncodeStellarAccountId(keypair.PublicKey);

            // Fixed for now....
            Uri uri = new Uri($"https://horizon-testnet.stellar.org/accounts/{accountId}");

            Server server = new Client().HorizonClient;

            AccountResponse account = await server.Accounts.Account(uri);

            return new Account(account, keypair);
        }   
    }
}
