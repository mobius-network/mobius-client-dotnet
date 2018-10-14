using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class ChallengeFixture
    {
        public Stellar.KeyPair Keypair = Stellar.KeyPair.Random();
        public Stellar.Transaction Tx { get; private set; }

        public ChallengeFixture()
        {
            Stellar.Network.UseTestNetwork();
            Tx = Stellar.Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(Keypair.SeedBytes));
        }
    }
    
    public class ChallengeTest: IClassFixture<ChallengeFixture>
    {
        ChallengeFixture _fixture;
        public ChallengeTest(ChallengeFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void SignsChallengeCorrectlyByDeveloper()
        {
            Assert.True(new Library.Utils.Keypair().Verify(_fixture.Tx, _fixture.Keypair));
        }

        [Fact]
        public void ContainsMemo()
        {
            Stellar.MemoText memo = Stellar.Memo.Text("Mobius authentication");
            
            Assert.Equal(memo, _fixture.Tx.Memo);
        }

        [Fact]
        public void ContainsTimeBounds()
        {
            Assert.NotNull(_fixture.Tx.TimeBounds);
        }

        [Fact]
        public void ContainsCorrectMinimumTimeBounds()
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds();

            Assert.Equal(_fixture.Tx.TimeBounds.MinTime.ToString(), timeNow.ToString());
        }

        [Fact]
        public void ContainsCorrectMaximumTimeBounds()
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds() + Library.Client.ChallengeExpiresIn;

            Assert.Equal(_fixture.Tx.TimeBounds.MaxTime.ToString(), timeNow.ToString());
        }

        [Fact]
        public void ContainsCorrectCustomTimeBounds()
        {
            Stellar.Transaction tx = Stellar.Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(_fixture.Keypair.SeedBytes, 100));
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds() + 100;

            Assert.Equal(tx.TimeBounds.MaxTime.ToString(), timeNow.ToString());
        }
    }
}
