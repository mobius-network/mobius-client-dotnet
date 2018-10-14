using System;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Challenge
    {
        ///<summary>Generates challenge transaction signed by developers private key.</summary>
        ///<param name="developerSecret">Developers secret seed</param>
        ///<param name="expireIn">Session expiration time in seconds from now. Default is Client.ChallengeExpiresIn.</param>
        ///<returns>Returns base64-encoded transaction envelope</returns>
        public string Call(string developerSecret, int expireIn = 0) {
            if (expireIn == 0) expireIn = Client.ChallengeExpiresIn;

            Stellar.KeyPair keypair = this.Keypair(developerSecret);
            Stellar.Account account = new Stellar.Account(keypair, this.RandomSequence());

            Stellar.PaymentOperation operation = 
                new Stellar.PaymentOperation.Builder(keypair, new Stellar.AssetTypeNative(), "0.000001")
                    .SetSourceAccount(Stellar.KeyPair.Random())
                    .Build();

            Stellar.Transaction tx = new Stellar.Transaction.Builder(account)
                .AddMemo(this.Memo())
                .AddTimeBounds(this.BuildTimeBounds(expireIn))
                .AddOperation(operation)
                .Build();

            tx.Sign(keypair);

            return tx.ToEnvelopeXdrBase64();
        }

        ///<param name="developerSecret">Developers secret seed</param>
        ///<returns>Returns developer keypair</returns>
        private Stellar.KeyPair Keypair(string developerSecret) {
            return Stellar.KeyPair.FromSecretSeed(developerSecret);
        }

        ///<returns>Returns a random sequence number</returns>
        private long RandomSequence() {
            Random rnd = new Random();
            return (long)(99999999 - Math.Floor((decimal)rnd.Next() * 65536));
        }

        ///<param name="expireIn">session expiration time in seconds from now</param>
        ///<returns>Returns timebounds, (`minTime` and `maxTime`)</returns>
        private Stellar.TimeBounds BuildTimeBounds(int expireIn) {
            long minTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            long maxTime = DateTimeOffset.Now.ToUnixTimeSeconds() + expireIn;

            return new Stellar.TimeBounds(minTime, maxTime);
        }

        ///<returns>Returns auth transaction memo</returns>
        private dynamic Memo() {
            return Stellar.Memo.Text("Mobius authentication");
        }
    }
}
