using System;
using System.Linq;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.Utils
{
    public class Keypair
    {
        ///<summary>Verify given keypair is a signer on a given transaction.</summary>
        ///<param name="tx">Transaction to verify</param>
        ///<param name="keypair">Keypair object</param>
        ///<returns>Returns true if given transaction is signed using specified keypair</returns>
        public Boolean Verify(Stellar.Transaction tx, Stellar.KeyPair keypair) {
            var signatures = tx.Signatures;
            var hash = tx.Hash();

            if (signatures == null || signatures.Count == 0) return false;

            return signatures.Where(s => keypair.Verify(hash, s.Signature.InnerValue)).ToArray().Length >= 1;
        }
    }
}
