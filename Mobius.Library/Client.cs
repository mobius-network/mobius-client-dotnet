using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stellar = stellar_dotnet_sdk;
using StellarResponses = stellar_dotnet_sdk.responses;
using StellarRequests = stellar_dotnet_sdk.responses.page;

namespace Mobius.Library {
    public class Client {
        private static Dictionary<string, string> Issuers = new Dictionary<string, string>() {
            { "PUBLIC", "GA6HCMBLTZS5VYYBCATRBRZ3BZJMAFUDKYYF6AH6MVCMGWMRDNSWJPIH" },
            { "TESTNET", "GDRWBLJURXUKM4RWDZDTPJNX6XBYFO3PSE4H4GPUL6H6RCUQVKTSD4AT" }
        };
        public static Dictionary<string, string> Urls = new Dictionary<string, string>() {
            { "TESTNET", "https://horizon-testnet.stellar.org" },
            { "PUBLIC", "https://horizon.stellar.org" }
        };
        private const string AssetCode = "MOBI";
        private static Stellar.Server InnerHorizonClient;
        private static Stellar.Network InnerNetwork;
        private static StellarResponses.AssetResponse InnerStellarAsset;

        ///<summary>In strict mode, session must be not older than 10 seconds from now</summary>
        ///<returns>int strict interval value in seconds (10 by default)</returns>
        public int StrictInterval = 10;

        ///<returns>Returns Challenge expiration value in seconds (1d by default)</returns>
        public const int ChallengeExpiresIn = 60 * 60 * 24;

        public Client() {
            if (Stellar.Network.Current == null) SetNetwork("TESTNET");
            
            InnerNetwork = Stellar.Network.Current;
        }

        ///<returns>Returns Mobius API host</returns>
        public string MobiusHost = "https://mobius.network";

        ///<returns>Returns the Asset Issuers Public or Testnet Key</returns>
        public string GetAssetIssuer() {
            string assetIssuer =
                Stellar.Network.Current != null && 
                Stellar.Network.IsPublicNetwork(Stellar.Network.Current)
                    ? Issuers["PUBLIC"]
                    : Issuers["TESTNET"];

            return assetIssuer;
        }

        ///<summary>Get Stellar Asset instance of asset used for payments</summary>
        ///<returns>Returns StellarSDK Asset instance of asset used for payments</returns>
        async public Task<StellarResponses.AssetResponse> StellarAsset(string assetIssuer = null, string assetCode = AssetCode) {
            if (InnerStellarAsset != null) return InnerStellarAsset;

            if (assetIssuer == null) assetIssuer = GetAssetIssuer();

            StellarRequests.Page<StellarResponses.AssetResponse> responses = 
                await HorizonClient.Assets.AssetIssuer(assetIssuer).AssetCode(assetCode).Execute();

            InnerStellarAsset = responses.Records.FirstOrDefault();

            return InnerStellarAsset;
        }

        ///<summary>Set Stellar network to use</summary>
        ///<param name="value">string: network to use</param>
        public void SetNetwork(string value = "TESTNET") {
            if (value == "TESTNET") Stellar.Network.UseTestNetwork();
            else if (value == "PUBLIC") Stellar.Network.UsePublicNetwork();
            else throw new Exception("Must provide value network. TESTNET or PUBLIC");

            InnerNetwork = Stellar.Network.Current;
        }

        ///<summary>Get current network instance</summary>
        ///<returns>Returns StellarSdk.Network instance</returns>
        public Stellar.Network Network
        {
            get { return InnerNetwork; }
        }

        ///<summary>Get StellarSdk.Server instance</summary>
        ///<returns>Returns StellarSdk.Server instance</returns>
        public Stellar.Server HorizonClient {
            get {
                if (InnerHorizonClient != null) return InnerHorizonClient;

                InnerHorizonClient =
                    Stellar.Network.Current != null && 
                    Stellar.Network.IsPublicNetwork(Stellar.Network.Current)
                        ? new Stellar.Server(Urls["PUBLIC"])
                        : new Stellar.Server(Urls["TESTNET"]);

                return InnerHorizonClient;
            }
        }

        ///<summary>Get Stellar Account Type for this account</summary>
        public Stellar.Account GetStellarAccount(Blockchain.Account account)
        {
            return new Stellar.Account(account.KeyPair(), null); // null sequnece number for now
        }
    }
}