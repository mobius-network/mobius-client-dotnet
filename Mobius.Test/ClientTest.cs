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

            Library.Client client = new Library.Client();

            Assert.Equal(client.Network.NetworkPassphrase, Stellar.Network.Current.NetworkPassphrase);
            Assert.False(Stellar.Network.IsPublicNetwork(client.Network));
        }

        [Fact]
        public void ShouldUsePublicNetwork()
        {
            Stellar.Network.UsePublicNetwork();

            Library.Client client = new Library.Client();

            Assert.Equal(client.Network.NetworkPassphrase, Stellar.Network.Current.NetworkPassphrase);
            Assert.True(Stellar.Network.IsPublicNetwork(client.Network));
        }
    }
}
