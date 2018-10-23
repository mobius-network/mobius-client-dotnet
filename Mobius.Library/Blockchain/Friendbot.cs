using System;
using Stellar = stellar_dotnet_sdk;
using StellarRequests = stellar_dotnet_sdk.requests;

namespace Mobius.Library.Blockchain
{
    public class Friendbot
    {
        ///<summary>Fund keypair and get response</summary>
        ///<param name="keypair">Keypair of account to fund</param>
        ///<returns>Promise returns response of funded account</returns>
        public static StellarRequests.FriendBotRequestBuilder Call(Stellar.KeyPair keypair) {
            return new Client().HorizonClient.TestNetFriendBot.FundAccount(keypair);
        }
    }
}
