
using System;
using System.Security.Cryptography;
using System.Text;
using Mobius.Library.Utils;
using stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Token
    {
        private byte[] _developerSecret { get; set; }
        private Transaction _tx { get; set; }
        private byte[] _address { get; set; }
        private KeyPair _keypair { get; set; }
        private KeyPair _theirKeypair { get; set; }
        public Token(byte[] developerSecret, string xdr, byte[] address)
        {
            _developerSecret = developerSecret;
            _tx = Transaction.FromEnvelopeXdr(xdr);
            _address = address;
        }

        /**
        * Returns time bounds for given transaction
        * @returns {StellarSdk.xdr.TimeBounds} Time bounds for given transaction (`minTime` and `maxTime`)
        */
        public TimeBounds timeBounds() {
            TimeBounds timebounds = _tx.TimeBounds;

            if (timebounds == null) {
                throw new Exception("Wrong challenge transaction structure");
            }

            return timebounds;
        }

        /**
        * Returns address this token is issued for.
        * @returns {string} Address.
        */
        public byte[] address() {
            return _getKeypair().PublicKey;
        }

        /**
        * Validates transaction signed by developer and user.
        * @param {boolean} [strict=true] - if true, checks that lower time limit is within Mobius::Client.strict_interval seconds from now
        * @returns {boolean} true if transaction is valid, raises exception otherwise
        */
        public Boolean validate(Boolean strict = true) {
            if (!_signedCorrectly()) {
                throw new Exception("Wrong challenge transaction signature");
            }

            TimeBounds bounds = timeBounds();

            if (!_timeNowCovers(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            if (strict && _tooOld(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            return true;
        }

        /**
        * @param {string} format="binary" - format for output data
        * @returns {Buffer|string} depends on `format` param passed
        */
        public string hash(string format = "binary") {
            validate();

            byte[] hash = _tx.Hash();

            return Util.BytesToHex(hash);
        }

        /**
        * @private
        * @returns {StellarSdk.Keypair} StellarSdk.Transaction object for given Developer private key
        */
        private KeyPair _getKeypair() {
            _keypair = _keypair != null ? _keypair : KeyPair.FromSecretSeed(_developerSecret);

            return _keypair;
        }

        /**
        * @private
        * @returns {StellarSdk.Keypair} StellarSdk.Transaction object of user being authorized
        */
        private KeyPair _getTheirKeypair() {
            _theirKeypair =
            _theirKeypair != null ? _theirKeypair : KeyPair.FromPublicKey(_address);

            return _theirKeypair;
        }

        /**
        * @private
        * @returns {boolean} true if transaction is correctly signed by user and developer
        */
        private Boolean _signedCorrectly() {
            bool isSignedByDeveloper = new Keypair().verify(_tx, _getKeypair());
            bool isSignedByUser = new Keypair().verify(_tx, _getTheirKeypair());

            return isSignedByDeveloper && isSignedByUser;
        }

        /**
        * @private
        * @param {StellarSdk.xdr.TimeBounds} timeBounds - Time bounds for given transaction
        * @returns {boolean} true if current time is within transaction time bounds
        */
        private Boolean _timeNowCovers(TimeBounds timeBounds) {
            long now = (long)Math.Floor((double)new DateTime().Millisecond / 1000);

            return (
                now >= timeBounds.MinTime &&
                now <= timeBounds.MaxTime
            );
        }

        /**
        * @param {StellarSdk.xdr.TimeBounds} timeBounds - Time bounds for given transaction
        * @returns {boolean} true if transaction is created more than 10 secods from now
        */
        public Boolean _tooOld(TimeBounds timeBounds) {
            long now = (long)Math.Floor((double)new DateTime().Millisecond/ 1000);
            int strictInterval = new Client().strictInterval;

            return now > timeBounds.MinTime + strictInterval;
        }
    }
}
