using System;
using stellar_dotnet_sdk;
using stellar_dotnet_sdk.requests;

namespace Mobius.Library.Blockchain
{
    public class Friendbot
    {
        ///<summary>Fund keypair and get response</summary>
        ///<param name="keypair">keypair - Keypair of account to fund</param>
        ///<returns>Promise - response of funded account</returns>
        public FriendBotRequestBuilder Call(KeyPair keypair) {
            return new Client().HorizonClient.TestNetFriendBot.FundAccount(keypair);
        }
    }
}
