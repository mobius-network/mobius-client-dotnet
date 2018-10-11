using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class SignFixture
    {
        public Stellar.KeyPair userKeypair = Stellar.KeyPair.Random();
        public Stellar.KeyPair devKeypair = Stellar.KeyPair.Random();

        public SignFixture()
        {
            Stellar.Network.UseTestNetwork();
        }
    }

    public class SignTest: IClassFixture<SignFixture>
    {
        SignFixture _fixture;

        public SignTest(SignFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void SignsChallengeCorrectlyByUser()
        {
            Stellar.Transaction tx = _generateSignedChallenge(_fixture.userKeypair, _fixture.devKeypair);

            Assert.True(new Library.Utils.Keypair().verify(tx, _fixture.userKeypair));
        }

        private Stellar.Transaction _generateSignedChallenge(Stellar.KeyPair userKeypair, Stellar.KeyPair devKeypair)
        {
            string challengeXdr = new Library.Auth.Challenge().Call(devKeypair.SeedBytes);
            string signedXdr = new Library.Auth.Sign().Call(userKeypair.SeedBytes, challengeXdr, devKeypair.PublicKey);

            return Stellar.Transaction.FromEnvelopeXdr(signedXdr);
        }
    }
}
