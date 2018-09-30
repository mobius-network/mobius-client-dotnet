using System;
using System.Text;
using Mobius.Library;
using Mobius.Library.App;
using Mobius.Library.Auth;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.chaos.nacl;

namespace Mobius.Console
{
    class Program
    {
        private static KeyPair _userKeypair { get; set; }
        private static KeyPair _devKeypair { get; set; }
        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting up Mobius .NET SDK Console App...");
            Client client = new Client();

            _userKeypair = RandomKeypair();
            _devKeypair = RandomKeypair();

            string xdr = GetChallenge(_devKeypair.SeedBytes);
            string signedXdr = SignChallenge(xdr);
            string tokenHash = GenerateAuthToken(_devKeypair.SeedBytes, signedXdr, _userKeypair.PublicKey);

            // Fund With Friendbot first, returning errors for empty unfound accounts via horizon.
            // System.Console.WriteLine(StrKey.EncodeStellarAccountId(_userKeypair.PublicKey));
            // App app = new AppBuilder().Build(_devKeypair.SeedBytes, _userKeypair.PublicKey).Result;
            // System.Console.WriteLine(app.userBalance().Result);
        }

        static KeyPair RandomKeypair()
        {
            return KeyPair.Random();
        }

        static string GetChallenge(byte[] devSecret)
        {
            System.Console.WriteLine("---------------------------------");
            System.Console.WriteLine("Generating challenge...");
            
            string challenge = new Challenge().Call(devSecret);

            System.Console.WriteLine($"Challenge generated: {challenge}");
            System.Console.WriteLine("---------------------------------");

            return challenge;
        }

        static string SignChallenge(string xdr)
        {
            System.Console.WriteLine("---------------------------------");
            System.Console.WriteLine("Signing requested XDR...");
        
            string signedXdr = new Sign().Call(_userKeypair.SeedBytes, xdr, _devKeypair.PublicKey);

            System.Console.WriteLine($"XDR signed: {signedXdr}");
            System.Console.WriteLine("---------------------------------");

            return signedXdr;
        }

        static string GenerateAuthToken(byte[] devSecret, string signedXdr, byte[] userPublic)
        {
            System.Console.WriteLine("---------------------------------");
            System.Console.WriteLine("Generating Auth Token...");

            Token token = new Token(devSecret, signedXdr, userPublic);

            System.Console.WriteLine($"Token Generated");

            bool validToken = token.validate();

            System.Console.WriteLine($"Token Validated: {validToken}");

            string tokenHash = token.hash("hex");

            System.Console.WriteLine($"Token Hash: {tokenHash}");
            System.Console.WriteLine("---------------------------------");

            return tokenHash;
        }
    }
}
