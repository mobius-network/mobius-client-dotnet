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
        async public static Task<Account> Build(Stellar.KeyPair keypair)
        {
            Client client = new Client();

            var account = await client.HorizonClient.Accounts.Account(keypair);

            return new Account(account, keypair);
        }   
    }
}
