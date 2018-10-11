using System;
using Xunit;
using Mobius.Library;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;

namespace Mobius.Test.AuthTests
{
	public class ChallengeFixture
	{
        public KeyPair keypair = KeyPair.Random();
        public DateTime date = new DateTime();
        public Transaction tx { get; private set; }

        public ChallengeFixture()
        {
            Network.UseTestNetwork();
            tx = Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(keypair.SeedBytes));
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
            MemoText memo = Memo.Text("Mobius authentication");
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
            Transaction tx = Transaction.FromEnvelopeXdr(new Library.Auth.Challenge().Call(_fixture.keypair.SeedBytes, 100));
            long timeNow = DateTimeOffset.Now.ToUnixTimeSeconds() + 100;

            Assert.Equal(tx.TimeBounds.MaxTime.ToString(), timeNow.ToString());
        }
    }
}
