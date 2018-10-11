using System;
using Mobius.Library.Utils;
using Stellar = stellar_dotnet_sdk;

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
            Stellar.Transaction tx = Stellar.Transaction.FromEnvelopeXdr(xdr);
            Stellar.KeyPair keypair = this._keypair(userSecret);
            Stellar.KeyPair developerKeypair = this._developerKeypair(address);

            this._validate(developerKeypair, tx);

            tx.Sign(keypair);

            return tx.ToEnvelopeXdrBase64();
        }

        ///<summary>Private: returns keypair from secret seed</summary>
        ///<param name="userSecret">Private seed</param>
        ///<returns>Returns keypair from given secret seed</returns>
        private Stellar.KeyPair _keypair(byte[] userSecret) {
            return Stellar.KeyPair.FromSecretSeed(userSecret);
        }

        ///<summary>Private: returns keypair from public key</summary>
        ///<param name="address">Developers public key</param>
        ///<returns>Returns keypair from given users public key</returns>
        private Stellar.KeyPair _developerKeypair(byte[] address) {
            return Stellar.KeyPair.FromPublicKey(address);
        }

        ///<summary>Private: Validates transaction is signed by developer.</summary>
        ///<param name="keypair">keypair object for given developer public key</param>
        ///<param name="tx">Transaction to verify</param>
        ///<returns>Returns true is transaction is valid, throws error otherwise</returns>
        private Boolean _validate(Stellar.KeyPair keypair, Stellar.Transaction tx) {
            Boolean isValid = new Utils.Keypair().verify(tx, keypair);

            if (!isValid) {
                throw new Exception("Wrong challenge transaction signature");
            }

            return true;
        }
    }
}
