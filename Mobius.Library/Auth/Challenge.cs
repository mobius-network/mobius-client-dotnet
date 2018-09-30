using System;
using stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Challenge
    {
        /**
        * Generates challenge transaction signed by developers private key.
        * @param {string} developerSecret - Developers private key
        * @param {number} expireIn - Session expiration time in seconds from now. Default is Client.challengeExpiresIn.
        * @returns {string} base64-encoded transaction envelope
        */
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

        /**
        * @private
        * @param {string} developerSecret - Developers private key
        * @returns {StellarSdk.Keypair} StellarSdk.Keypair
        */
        private KeyPair _keypair(byte[] developerSecret) {
            return KeyPair.FromSecretSeed(developerSecret);
        }

        /**
        * @private
        * @returns {number} Random sequence number
        */
        private long _randomSequence() {
            Random rnd = new Random();
            return (long)(99999999 - Math.Floor((decimal)rnd.Next() * 65536));
        }

        /**
        * @private
        * @param {number} expireIn - session expiration time in seconds from now
        * @returns {object} Time bounds (`minTime` and `maxTime`)
        */
        private TimeBounds _buildTimeBounds(int expireIn) {
            long minTime = (long)Math.Floor((double)new DateTime().Millisecond / 1000);
            long maxTime = (long)Math.Floor((double)new DateTime().Millisecond / 1000 + expireIn);

            return new TimeBounds(minTime, maxTime);
        }

        /**
        * @private
        * @returns {StellarSdk.Memo} Auth transaction memo
        */
        private dynamic memo() {
            return Memo.Text("Mobius authentication");
        }
    }
}
