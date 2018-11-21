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
            Console.WriteLine("Starting up Mobius .NET SDK Console App...");
            Client client = new Client();

            _userKeypair = RandomKeypair();
            _devKeypair = RandomKeypair();

            string xdr = GetChallenge(_devKeypair.SecretSeed);
            string signedXdr = SignChallenge(xdr);
            string tokenHash = GenerateAuthToken(_devKeypair.SecretSeed, signedXdr, _userKeypair.AccountId);
        }

        static KeyPair RandomKeypair()
        {
            return KeyPair.Random();
        }

        static string GetChallenge(string devSecret)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Generating challenge...");
            
            string challenge = new Challenge().Call(devSecret);

            Console.WriteLine($"Challenge generated: {challenge}");
            Console.WriteLine("---------------------------------");

            return challenge;
        }

        static string SignChallenge(string xdr)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Signing requested XDR...");
        
            string signedXdr = new Sign().Call(_userKeypair.SeedBytes, xdr, _devKeypair.PublicKey);

            Console.WriteLine($"XDR signed: {signedXdr}");
            Console.WriteLine("---------------------------------");

            return signedXdr;
        }

        static string GenerateAuthToken(string devSecret, string signedXdr, string userPublic)
        {
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Generating Auth Token...");

            Token token = new Token(devSecret, signedXdr, userPublic);

            Console.WriteLine($"Token Generated");

            bool validToken = token.Validate();

            Console.WriteLine($"Token is valid: {validToken}");

            string tokenHash = token.Hash();

            Console.WriteLine($"Token Hash: {tokenHash}");
            Console.WriteLine("---------------------------------");

            return tokenHash;
        }
    }
}
