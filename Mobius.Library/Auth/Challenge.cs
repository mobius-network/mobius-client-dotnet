using System;
using stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Challenge
    {
        ///<summary>Generates challenge transaction signed by developers private key.</summary>
        ///<param name="developerSecret">Developers secret seed</param>
        ///<param name="expireIn">Session expiration time in seconds from now. Default is Client.challengeExpiresIn.</param>
        ///<returns>Returns base64-encoded transaction envelope</returns>
        public string Call(byte[] developerSecret, int expireIn = 0) {
            if (expireIn == 0) expireIn = Client.challengeExpiresIn;

            KeyPair keypair = _keypair(developerSecret);
            Account account = new Account(keypair, _randomSequence());

            PaymentOperation operation = 
                new PaymentOperation.Builder(keypair, new AssetTypeNative(), "0.000001")
                    .SetSourceAccount(KeyPair.Random())
                    .Build();

            Transaction tx = new Transaction.Builder(account)
                .AddMemo(memo())
                .AddTimeBounds(_buildTimeBounds(expireIn))
                .AddOperation(operation)
                .Build();

            tx.Sign(keypair);

            return tx.ToEnvelopeXdrBase64();
        }

        ///<param name="developerSecret">Developers secret seed</param>
        ///<returns>Returns developer keypair</returns>
        private KeyPair _keypair(byte[] developerSecret) {
            return KeyPair.FromSecretSeed(developerSecret);
        }

        ///<returns>Returns a random sequence number</returns>
        private long _randomSequence() {
            Random rnd = new Random();
            return (long)(99999999 - Math.Floor((decimal)rnd.Next() * 65536));
        }

        ///<param name="expireIn">session expiration time in seconds from now</param>
        ///<returns>Returns timebounds, (`minTime` and `maxTime`)</returns>
        private TimeBounds _buildTimeBounds(int expireIn) {
            long minTime = (long)Math.Floor((double)new DateTime().Millisecond / 1000);
            long maxTime = (long)Math.Floor((double)new DateTime().Millisecond / 1000 + expireIn);

            return new TimeBounds(minTime, maxTime);
        }

        ///<returns>Returns auth transaction memo</returns>
        private dynamic memo() {
            return Memo.Text("Mobius authentication");
        }
    }
}
