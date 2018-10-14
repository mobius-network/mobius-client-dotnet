
using System;
using System.Security.Cryptography;
using System.Text;
using Mobius.Library.Utils;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.Auth
{
    public class Token
    {
        private string DeveloperSecret;
        private Stellar.Transaction Tx;
        private string InnerAddress;
        private Stellar.KeyPair Keypair;
        private Stellar.KeyPair TheirKeypair;
        
        ///<summary>Checks challenge transaction signed by user on developer's side.</summary>
        ///<param name="developerSecret">Developer secret seed</param>
        ///<param name="xdr">Challenge transaction xdr</param>
        ///<param name="address">User public key</param>
        public Token(string developerSecret, string xdr, string address)
        {
            this.DeveloperSecret = developerSecret;
            this.Tx = Stellar.Transaction.FromEnvelopeXdr(xdr);
            this.InnerAddress = address;
        }

        ///<summary>Verify and return timebounds for the given transaction</summary>
        ///<returns>Returns timebounds for given transaction (`minTime` and `maxTime`)</returns>
        public Stellar.TimeBounds TimeBounds() {
            Stellar.TimeBounds timebounds = this.Tx.TimeBounds;

            if (timebounds == null) {
                throw new Exception("Wrong challenge transaction structure");
            }

            return timebounds;
        }

        ///<summary>Returns address this token is issued for.</summary>
        ///<returns>Returns the keypairs public key</returns>
        public byte[] Address() {
            return this.GetKeypair().PublicKey;
        }

        ///<summary>Validates transaction signed by developer and user.</summary>
        ///<param name="strict">[strict=true] - if true, checks that lower time limit is within Mobius.Client.StrictInterval seconds from now</param>
        ///<returns>Returns true if transaction is valid, raises exception otherwise</returns>
        public Boolean Validate(Boolean strict = true) {
            if (!this.SignedCorrectly()) {
                throw new Exception("Wrong challenge transaction signature");
            }

            Stellar.TimeBounds bounds = TimeBounds();

            if (!this.TimeNowCovers(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            if (strict && this.TooOld(bounds)) {
                throw new Exception("Challenge transaction expired");
            }

            return true;
        }

        ///<summary>Validate token and return transaction hash as string</summary>
        ///<returns>Returns transaction hash bytes to hex as string.</returns>
        public string Hash() {
            Validate();

            byte[] hash = this.Tx.Hash();

            return Stellar.Util.BytesToHex(hash);
        }

        ///<summary>Private: Returns keypair or keypair from a secret seed</summary>
        ///<returns>keypair object for given Developer private key</returns>
        private Stellar.KeyPair GetKeypair() {
            this.Keypair = this.Keypair != null ? this.Keypair : Stellar.KeyPair.FromSecretSeed(this.DeveloperSecret);

            return this.Keypair;
        }

        ///<summary>Private: Returns user keypair or keypair from a public key</summary>
        ///<returns>keypair object of user being authorized</returns>
        private Stellar.KeyPair GetTheirKeypair() {
            this.TheirKeypair =
            this.TheirKeypair != null ? this.TheirKeypair : Stellar.KeyPair.FromAccountId(this.InnerAddress);

            return this.TheirKeypair;
        }

        ///<returns>Returns true if transaction is correctly signed by user and developer</returns>
        private Boolean SignedCorrectly() {
            bool isSignedByDeveloper = new Keypair().Verify(this.Tx, this.GetKeypair());
            bool isSignedByUser = new Keypair().Verify(this.Tx, this.GetTheirKeypair());

            return isSignedByDeveloper && isSignedByUser;
        }

        ///<summary>Private: Checks if current time is within transaction timebounds</summary>
        ///<param name="timeBounds">Timebounds for given transaction</param>
        ///<returns>Returns true if current time is within transaction time bounds</returns>
        private Boolean TimeNowCovers(Stellar.TimeBounds timeBounds) {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();

            return (
                now >= timeBounds.MinTime &&
                now <= timeBounds.MaxTime
            );
        }

        ///<param name="timeBounds">Timebounds for given transaction</param>
        ///<returns>Returns true if transaction is created more than 10 secods from now</returns>
        public Boolean TooOld(Stellar.TimeBounds timeBounds) {
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            int strictInterval = new Client().StrictInterval;

            return now > (timeBounds.MinTime + strictInterval);
        }
    }
}
