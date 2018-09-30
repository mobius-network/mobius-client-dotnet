using System;
using System.Threading.Tasks;
using Mobius.Library.Blockchain;
using stellar_dotnet_sdk;

namespace Mobius.Library.App
{
    public class AppBuilder
    {
        async public Task<App> Build(byte[] developerSecret, byte[] address)
        {
            KeyPair developerKeypair = KeyPair.FromSecretSeed(developerSecret);
            Blockchain.Account developerAccount = await new AccountBuilder().Build(developerKeypair);

            KeyPair userKeypair = KeyPair.FromPublicKey(address);
            Blockchain.Account userAccount = await new AccountBuilder().Build(userKeypair);

            return new App(developerAccount, userAccount);
        }
    }
}
