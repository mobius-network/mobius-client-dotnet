using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class TokenFixture
    {
        public Stellar.KeyPair userKeypair = Stellar.KeyPair.Random();
        public Stellar.KeyPair devKeypair = Stellar.KeyPair.Random();
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
            Stellar.Transaction tx = _generateSignedChallenge(_fixture.userKeypair, _fixture.devKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.devKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.userKeypair.PublicKey);

            Assert.True(token.validate());
        }

        [Fact]
        public void ThrowsErrorIfCurrentTimeIsOutsideTimeBounds()
        {
            Stellar.Transaction tx = _generateSignedChallenge(_fixture.userKeypair, _fixture.devKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.devKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.userKeypair.PublicKey);

            System.Threading.Thread.Sleep(11000);

            Assert.ThrowsAny<Exception>(() => token.validate());
        }

        [Fact]
        public void ReturnsTransactionHash()
        {
            Stellar.Transaction tx = _generateSignedChallenge(_fixture.userKeypair, _fixture.devKeypair);
            Library.Auth.Token token = new Library.Auth.Token(_fixture.devKeypair.SeedBytes, tx.ToEnvelopeXdrBase64(), _fixture.userKeypair.PublicKey);

            Assert.NotNull(token.hash());
        }

        private Stellar.Transaction _generateSignedChallenge(Stellar.KeyPair userKeypair, Stellar.KeyPair devKeypair)
        {
            string challengeXdr = new Library.Auth.Challenge().Call(devKeypair.SeedBytes);
            string signedXdr = new Library.Auth.Sign().Call(userKeypair.SeedBytes, challengeXdr, devKeypair.PublicKey);

            return Stellar.Transaction.FromEnvelopeXdr(signedXdr);
        }
    }
}
