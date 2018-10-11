
using System;
using System.Security.Cryptography;
using System.Text;
using Mobius.Library.Utils;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Token
    {
        private byte[] _developerSecret;
        private Stellar.Transaction _tx;
        private byte[] _address;
        private Stellar.KeyPair _keypair;
        private Stellar.KeyPair _theirKeypair;
        
        ///<summary>Checks challenge transaction signed by user on developer's side.</summary>
        ///<param name="developerSecret">Developer secret seed</param>
        ///<param name="xdr">Challenge transaction xdr</param>
        ///<param name="address">User public key</param>
        public Token(byte[] developerSecret, string xdr, byte[] address)
        {
            _developerSecret = developerSecret;
            _tx = Stellar.Transaction.FromEnvelopeXdr(xdr);
            _address = address;
        }

        ///<summary>Verify and return timebounds for the given transaction</summary>
        ///<returns>Returns timebounds for given transaction (`minTime` and `maxTime`)</returns>
        public Stellar.TimeBounds timeBounds() {
            Stellar.TimeBounds timebounds = _tx.TimeBounds;

            if (timebounds == null) {
                throw new Exception("Wrong challenge transaction structure");
            }

            return timebounds;
        }

        ///<summary>Returns address this token is issued for.</summary>
        ///<returns>Returns the keypairs public key</returns>
        public byte[] address() {
            return _getKeypair().PublicKey;
        }

        ///<summary>Validates transaction signed by developer and user.</summary>
        ///<param name="strict">[strict=true] - if true, checks that lower time limit is within Mobius.Client.strictInterval seconds from now</param>
        ///<returns>Returns true if transaction is valid, raises exception otherwise</returns>
        public Boolean validate(Boolean strict = true) {
            if (!_signedCorrectly()) {
                throw new Exception("Wrong challenge transaction signature");
            }

            Stellar.TimeBounds bounds = timeBounds();

            if (!_timeNowCovers(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            if (strict && _tooOld(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            return true;
        }

        ///<summary>Validate token and return transaction hash as string</summary>
        ///<returns>Returns transaction hash bytes to hex as string.</returns>
        public string hash() {
            validate();

            byte[] hash = _tx.Hash();

            return Stellar.Util.BytesToHex(hash);
        }

        ///<summary>Private: Returns keypair or keypair from a secret seed</summary>
        ///<returns>keypair object for given Developer private key</returns>
        private Stellar.KeyPair _getKeypair() {
            _keypair = _keypair != null ? _keypair : Stellar.KeyPair.FromSecretSeed(_developerSecret);

            return _keypair;
        }

        ///<summary>Private: Returns user keypair or keypair from a public key</summary>
        ///<returns>keypair object of user being authorized</returns>
        private Stellar.KeyPair _getTheirKeypair() {
            _theirKeypair =
            _theirKeypair != null ? _theirKeypair : Stellar.KeyPair.FromPublicKey(_address);

            return _theirKeypair;
        }

        ///<returns>Returns true if transaction is correctly signed by user and developer</returns>
        private Boolean _signedCorrectly() {
            bool isSignedByDeveloper = new Keypair().verify(_tx, _getKeypair());
            bool isSignedByUser = new Keypair().verify(_tx, _getTheirKeypair());

            return isSignedByDeveloper && isSignedByUser;
        }

        ///<summary>Private: Checks if current time is within transaction timebounds</summary>
        ///<param name="timeBounds">Timebounds for given transaction</param>
        ///<returns>Returns true if current time is within transaction time bounds</returns>
        private Boolean _timeNowCovers(Stellar.TimeBounds timeBounds) {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();

            return (
                now >= timeBounds.MinTime &&
                now <= timeBounds.MaxTime
            );
        }

        ///<param name="timeBounds">Timebounds for given transaction</param>
        ///<returns>Returns true if transaction is created more than 10 secods from now</returns>
        public Boolean _tooOld(Stellar.TimeBounds timeBounds) {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            int strictInterval = new Client().strictInterval;

            return now > (timeBounds.MinTime + strictInterval);
        }
    }
}
