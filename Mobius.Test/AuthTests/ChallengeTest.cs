using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class ChallengeFixture
    {
        public Stellar.KeyPair keypair = Stellar.KeyPair.Random();
        public Stellar.Transaction tx { get; private set; }

        public ChallengeFixture()
        {
            Stellar.Network.UseTestNetwork();
            tx = Stellar.Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(keypair.SeedBytes));
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
            Assert.True(new Library.Utils.Keypair().verify(_fixture.tx, _fixture.keypair));
        }

        [Fact]
        public void ContainsMemo()
        {
            Stellar.MemoText memo = Stellar.Memo.Text("Mobius authentication");
            
            Assert.Equal(memo, _fixture.tx.Memo);
        }

        [Fact]
        public void ContainsTimeBounds()
        {
            Assert.NotNull(_fixture.tx.TimeBounds);
        }

        [Fact]
        public void ContainsCorrectMinimumTimeBounds()
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds();

            Assert.Equal(_fixture.tx.TimeBounds.MinTime.ToString(), timeNow.ToString());
        }

        [Fact]
        public void ContainsCorrectMaximumTimeBounds()
        {
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds() + Library.Client.challengeExpiresIn;

            Assert.Equal(_fixture.tx.TimeBounds.MaxTime.ToString(), timeNow.ToString());
        }

        [Fact]
        public void ContainsCorrectCustomTimeBounds()
        {
            Stellar.Transaction tx = Stellar.Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(_fixture.keypair.SeedBytes, 100));
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds() + 100;

            Assert.Equal(tx.TimeBounds.MaxTime.ToString(), timeNow.ToString());
        }
    }
}
