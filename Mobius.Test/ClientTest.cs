using System;
using Xunit;
using Mobius.Library;
using Stellar = stellar_dotnet_sdk;

namespace Mobius.Test
{
    public class ClientTest
    {
        [Fact]
        public void ShouldUseTestNetwork()
        {
            Stellar.Network.UseTestNetwork();

            Library.Client _client = new Library.Client();

            Assert.Equal(_client.Network.NetworkPassphrase, Stellar.Network.Current.NetworkPassphrase);
            Assert.False(Stellar.Network.IsPublicNetwork(_client.Network));
        }

        [Fact]
        public void ShouldUsePublicNetwork()
        {
            Stellar.Network.UsePublicNetwork();

            Library.Client _client = new Library.Client();

            Assert.Equal(_client.Network.NetworkPassphrase, Stellar.Network.Current.NetworkPassphrase);
            Assert.True(Stellar.Network.IsPublicNetwork(_client.Network));
        }
    }
}
