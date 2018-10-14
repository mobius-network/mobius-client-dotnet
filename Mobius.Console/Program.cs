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

        static string GenerateAuthToken(string devSecret, string signedXdr, string userPublic)
        {
            System.Console.WriteLine("---------------------------------");
            System.Console.WriteLine("Generating Auth Token...");

            Token token = new Token(devSecret, signedXdr, userPublic);

            System.Console.WriteLine($"Token Generated");

            bool validToken = token.Validate();

            System.Console.WriteLine($"Token is valid: {validToken}");

            string tokenHash = token.Hash();

            System.Console.WriteLine($"Token Hash: {tokenHash}");
            System.Console.WriteLine("---------------------------------");

            return tokenHash;
        }
    }
}
