using System;
using stellar_dotnet_sdk;
using Mobius.Library.Utils;

namespace Mobius.Library.Auth
{
    public class Sign
    {
        public string Call(byte[] userSecret, string xdr, byte[] address)
        {
            Transaction tx = Transaction.FromEnvelopeXdr(xdr);
            KeyPair keypair = this._keypair(userSecret);
            KeyPair developerKeypair = this._developerKeypair(address);

            this._validate(developerKeypair, tx);

            tx.Sign(keypair);

            return tx.ToEnvelopeXdrBase64();
        }

        /**
        * @private
        * @param {string} userSecret - Users private key
        * @returns {StellarSdk.Keypair} StellarSdk.Keypair object for given users private key
        */
        private KeyPair _keypair(byte[] userSecret) {
            return KeyPair.FromSecretSeed(userSecret);
        }

        /**
        * @private
        * @param {string}  address - Developers public key
        * @returns {StellarSdk.Keypair} StellarSdk.Keypair object for given developers public key
        */
        private KeyPair _developerKeypair(byte[] address) {
            return KeyPair.FromPublicKey(address);
        }

        /**
        * Validates transaction is signed by developer.
        * @private
        * @param {StellarSdk.Keypair} keypair - StellarSdk.Keypair object for given Developer public key
        * @param {StellarSdk.Transaction} tx - StellarSdk.Transaction to verify
        * @returns {boolean} true is transaction is valid, throws error otherwise
        */
        private Boolean _validate(KeyPair keypair, Transaction tx) {
            Boolean isValid = new Utils.Keypair().verify(tx, keypair);

            if (!isValid) {
                throw new Exception("Wrong challenge transaction signature");
            }

            return true;
        }
    }
}
