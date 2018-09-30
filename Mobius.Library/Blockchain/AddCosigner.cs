using System;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Library.Blockchain
{
    public class AddCosigner
    {
        ///<summary>Add a cosigner to a given account.</summary>
        ///<param name="keypair">StellarSdk.Keypair: keypair - Account keypair</param>
        ///<param name="cosignerKeypair">StellarSdk.Keypair: cosignerKeypair - Cosigner account keypair</param>
        ///<param name="weight">int: weight - Cosigner weight = 1 (default)</param>
        ///<returns>Stellar.responses.SubmitTransactionResponse: submitted transaction response</returns>
        public Stellar.responses.SubmitTransactionResponse Call (
            Stellar.KeyPair keypair, 
            Stellar.xdr.SignerKey cosignerKeypair, 
            int weight = 1
        ) 
        {
            Stellar.Server client = new Client().HorizonClient;
            Stellar.Account account =  new Stellar.Account(keypair, null); // null sequnece number for now
            Stellar.Transaction tx = _tx(account, cosignerKeypair, weight);

            tx.Sign(account.KeyPair);

            return client.SubmitTransaction(tx).Result;
        }

        ///<summary>Private: Generate setOptions transaction with given parameters.</summary>
        ///<param name="account">stellar_dotnet_sdk.Account</param>
        ///<param name="cosignerKeypair">stellar_dotnet_sdk.Keypair</param>
        ///<param name="weight">int</param>
        ///<returns>{StellarSdk.Transaction}</returns>
        private Stellar.Transaction _tx(
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
