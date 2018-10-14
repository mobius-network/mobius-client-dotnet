using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class TokenFixture
    {
        public Stellar.KeyPair UserKeypair = Stellar.KeyPair.Random();
        public Stellar.KeyPair DevKeypair = Stellar.KeyPair.Random();
        public TokenFixture()
        {
            Stellar.Network.UseTestNetwork();
        }
    }

    public class TokenTest: IClassFixture<TokenFixture>
    {
        TokenFixture _fixture;
        public TokenTest(TokenFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void CurrentTimeIsWithinTimeBounds()
        {
            Stellar.Transaction tx = this.GenerateSignedChallenge(_fixture.UserKeypair, _fixture.DevKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.DevKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.UserKeypair.PublicKey);

            Assert.True(token.Validate());
        }

        [Fact]
        public void ThrowsErrorIfCurrentTimeIsOutsideTimeBounds()
        {
            Stellar.Transaction tx = this.GenerateSignedChallenge(_fixture.UserKeypair, _fixture.DevKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.DevKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.UserKeypair.PublicKey);

            System.Threading.Thread.Sleep(11000);

            Assert.ThrowsAny<Exception>(() => token.Validate());
        }

        [Fact]
        public void ReturnsTransactionHash()
        {
            Stellar.Transaction tx = this.GenerateSignedChallenge(_fixture.UserKeypair, _fixture.DevKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.DevKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.UserKeypair.PublicKey);

            Assert.NotNull(token.Hash());
        }

        private Stellar.Transaction GenerateSignedChallenge(Stellar.KeyPair UserKeypair, Stellar.KeyPair DevKeypair)
        {
            string challengeXdr = new Library.Auth.Challenge().Call(DevKeypair.SeedBytes);
            string signedXdr = new Library.Auth.Sign().Call(UserKeypair.SeedBytes, challengeXdr, DevKeypair.PublicKey);

            return Stellar.Transaction.FromEnvelopeXdr(signedXdr);
        }
    }
}
