using System;
using Xunit;
using Mobius.Library;
using stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class SignFixture
    {
        public KeyPair userKeypair = KeyPair.Random();
        public KeyPair devKeypair = KeyPair.Random();

        public SignFixture()
        {
            Network.UseTestNetwork();
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
            Transaction tx = _generateSignedChallenge(_fixture.userKeypair, _fixture.devKeypair);

            Assert.True(new Library.Utils.Keypair().verify(tx, _fixture.userKeypair));
        }

        private Transaction _generateSignedChallenge(KeyPair userKeypair, KeyPair devKeypair)
        {
            string challengeXdr = new Library.Auth.Challenge().Call(devKeypair.SeedBytes);
            string signedXdr = new Library.Auth.Sign().Call(userKeypair.SeedBytes, challengeXdr, devKeypair.PublicKey);

            return Transaction.FromEnvelopeXdr(signedXdr);
        }
    }
}
