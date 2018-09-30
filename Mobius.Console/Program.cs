using System;
using System.Text;
using Mobius.Library;
using Mobius.Library.App;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.chaos.nacl;

namespace Mobius.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] devSecretSeed = StrKey.DecodeStellarSecretSeed("SDIVAIF2LC3TK7JCUN42Z3AQPHKH3SHBLIN3N5KY2L65Y27STNI2QWPP");
            byte[] userPublicKey = StrKey.DecodeStellarAccountId("GC4K2VSOJDVFSH6BHILE7GFSFM46GUFG5NU6VGSH7MRPVGOJNG5OT376");

            App app = new AppBuilder().Build(devSecretSeed, userPublicKey).Result;
            System.Console.WriteLine(app.appBalance().Result);
		}
    }
}
