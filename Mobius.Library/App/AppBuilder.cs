using System;
using System.Threading.Tasks;
using Mobius.Library.Blockchain;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.App
{
    public class AppBuilder
    {
        ///<summary>Build new app instance</summary>
        ///<param name="developerSecret">Developer secret seed</param>
        ///<param name="address">User public key</param>
        ///<returns>Promise returns new app instance</returns>
        async public Task<App> Build(string developerSecret, string address)
        {
            var developerKeypair = Stellar.KeyPair.FromSecretSeed(developerSecret);
            var developerAccount = await AccountBuilder.Build(developerKeypair);

            var userKeypair = Stellar.KeyPair.FromAccountId(address);
            var userAccount = await AccountBuilder.Build(userKeypair);

            return new App(developerAccount, userAccount);
        }
    }
}
