using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test.AuthTests
{
    public class SignFixture
    {
        public Stellar.KeyPair UserKeypair = Stellar.KeyPair.Random();
        public Stellar.KeyPair DevKeypair = Stellar.KeyPair.Random();

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
            Stellar.Transaction tx = this.GenerateSignedChallenge(_fixture.UserKeypair, _fixture.DevKeypair);

            Assert.True(new Library.Utils.Keypair().Verify(tx, _fixture.UserKeypair));
        }

        private Stellar.Transaction GenerateSignedChallenge(Stellar.KeyPair UserKeypair, Stellar.KeyPair DevKeypair)
        {
            string challengeXdr = new Library.Auth.Challenge().Call(DevKeypair.SeedBytes);
            string signedXdr = new Library.Auth.Sign().Call(UserKeypair.SeedBytes, challengeXdr, DevKeypair.PublicKey);

            return Stellar.Transaction.FromEnvelopeXdr(signedXdr);
        }
    }
}
