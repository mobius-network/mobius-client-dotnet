using System;
using stellar_dotnet_sdk;
using Mobius.Library.Utils;

namespace Mobius.Library.Auth
{
    public class Sign
    {
        ///<summary>Adds signature to given transaction.</summary>
        ///<param name="userSecret">Users secret seed</param>
        ///<param name="xdr">Challenge transaction xdr</param>
        ///<param name="address">Developer public key</param>
        ///<returns>Returns base64-encoded transaction envelope</returns>
        public string Call(byte[] userSecret, string xdr, byte[] address)
        {
            Transaction tx = Transaction.FromEnvelopeXdr(xdr);
            KeyPair keypair = this._keypair(userSecret);
            KeyPair developerKeypair = this._developerKeypair(address);

            this._validate(developerKeypair, tx);

            tx.Sign(keypair);

            return tx.ToEnvelopeXdrBase64();
        }

        ///<summary>Private: returns keypair from secret seed</summary>
        ///<param name="userSecret">Private seed</param>
        ///<returns>Returns keypair from given secret seed</returns>
        private KeyPair _keypair(byte[] userSecret) {
            return KeyPair.FromSecretSeed(userSecret);
        }

        ///<summary>Private: returns keypair from public key</summary>
        ///<param name="address">Developers public key</param>
        ///<returns>Returns keypair from given users public key</returns>
        private KeyPair _developerKeypair(byte[] address) {
            return KeyPair.FromPublicKey(address);
        }

        ///<summary>Private: Validates transaction is signed by developer.</summary>
        ///<param name="keypair">keypair object for given developer public key</param>
        ///<param name="tx">Transaction to verify</param>
        ///<returns>Returns true is transaction is valid, throws error otherwise</returns>
        private Boolean _validate(KeyPair keypair, Transaction tx) {
            Boolean isValid = new Utils.Keypair().verify(tx, keypair);

            if (!isValid) {
                throw new Exception("Wrong challenge transaction signature");
            }

            return true;
        }
    }
}
