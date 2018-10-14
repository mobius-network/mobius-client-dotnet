using System;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;

namespace Mobius.Library.Blockchain
{
    public class AddCosigner
    {
        ///<summary>Add a cosigner to a given account.</summary>
        ///<param name="keypair">Account keypair</param>
        ///<param name="cosignerKeypair">Cosigner keypair</param>
        ///<param name="weight">Cosigner weight = 1 (default)</param>
        ///<returns>Returns submitted transaction response</returns>
        public StellarResponses.SubmitTransactionResponse Call (
            Stellar.KeyPair keypair, 
            Stellar.xdr.SignerKey cosignerKeypair, 
            int weight = 1
        ) 
        {
            Stellar.Server client = new Client().HorizonClient;
            Stellar.Account account =  new Stellar.Account(keypair, null); // null sequnece number for now
            Stellar.Transaction tx = this.Tx(account, cosignerKeypair, weight);

            tx.Sign(account.KeyPair);

            return client.SubmitTransaction(tx).Result;
        }

        ///<summary>Private: Generate setOptions transaction with given parameters.</summary>
        ///<param name="account">Stellar account</param>
        ///<param name="cosignerKeypair">Cosigner keypair</param>
        ///<param name="weight">Cosigner weight</param>
        ///<returns>Returns new Stellar Transaction.</returns>
        private Stellar.Transaction Tx(
            Stellar.Account account, 
            Stellar.xdr.SignerKey cosignerKeypair, 
            int weight
        ) 
        {
            Stellar.SetOptionsOperation operation = 
                new Stellar.SetOptionsOperation.Builder()
                    .SetHighThreshold(10)
                    .SetLowThreshold(1)
                    .SetMasterKeyWeight(10)
                    .SetMediumThreshold(1)
                    .SetSigner(cosignerKeypair, weight)
                    .Build();

            return new Stellar.Transaction.Builder(account)
                .AddOperation(operation)
                .Build();
        }
    }
}
