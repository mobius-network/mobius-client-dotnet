using System;
using System.Threading.Tasks;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;

namespace Mobius.Test.UtilsTests
{
    public class KeyPairFixture
    {
        public KeyPairFixture()
        {
            Stellar.Network.UseTestNetwork();
        }
    }

    public class KeyPairTest: IClassFixture<KeyPairFixture>
    {
        [Fact]
        public void TransactionIsCorrectlySigned()
        {
            Stellar.KeyPair keypair = Stellar.KeyPair.Random();
            Stellar.Transaction tx = _generateSignedTransaction(keypair).Result;

            Assert.True(new Library.Utils.Keypair().verify(tx, keypair));
        }
        
        [Fact]
        public void TransactionFailsIfIncorrectlySigned()
        {
            Stellar.KeyPair keypair = Stellar.KeyPair.Random();
            Stellar.KeyPair anotherKeypair = Stellar.KeyPair.Random();
            Stellar.Transaction tx = _generateSignedTransaction(keypair).Result;

            Assert.False(new Library.Utils.Keypair().verify(tx, anotherKeypair));
        }

        async private Task<Stellar.Transaction> _generateSignedTransaction(Stellar.KeyPair keypair)
        {
            long randomSequence = (long)(99999999 - Math.Floor((decimal)new Random().Next() *  65536));

            Stellar.Account account = new Stellar.Account(keypair, randomSequence);
            Stellar.Transaction.Builder txBuilder = new Stellar.Transaction.Builder(account);

            StellarResponses.AssetResponse asset = await new Library.Client().StellarAsset();
            
            Stellar.Operation op = new Stellar.PaymentOperation.Builder(keypair, asset.Asset, "0.000001").Build();
            Stellar.Transaction tx = txBuilder.AddOperation(op).Build();
            
            tx.Sign(keypair);
            
            return Stellar.Transaction.FromEnvelopeXdr(tx.ToEnvelopeXdr());
        }
    }
}
