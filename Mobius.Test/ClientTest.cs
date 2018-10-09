using System;
using Xunit;
using Mobius.Library;
using stellar_dotnet_sdk;

namespace Mobius.Test
{
    public class ClientTest
    {
        [Fact]
        public void ShouldUseTestNetwork()
        {
            Network.UseTestNetwork();

            Library.Client _client = new Library.Client();

            Assert.Equal(_client.Network.NetworkPassphrase, Network.Current.NetworkPassphrase);
            Assert.False(Network.IsPublicNetwork(_client.Network));
        }

        [Fact]
        public void ShouldUsePublicNetwork()
        {
            Network.UsePublicNetwork();

            Library.Client _client = new Library.Client();

            Assert.Equal(_client.Network.NetworkPassphrase, Network.Current.NetworkPassphrase);
            Assert.True(Network.IsPublicNetwork(_client.Network));
        }
    }
}
