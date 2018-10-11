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
        private const string _assetCode = "MOBI";
        private static Stellar.Server _horizonClient;
        private static Stellar.Network _network;
        private static StellarResponses.AssetResponse _stellarAsset;

        ///<summary>In strict mode, session must be not older than 10 seconds from now</summary>
        ///<returns>int strict interval value in seconds (10 by default)</returns>
        public int strictInterval = 10;

        ///<returns>Returns Challenge expiration value in seconds (1d by default)</returns>
        public const int challengeExpiresIn = 60 * 60 * 24;

        public Client() {
            if (Stellar.Network.Current == null) SetNetwork("TESTNET");
            
            _network = Stellar.Network.Current;
        }

        ///<returns>Returns Mobius API host</returns>
        public string mobiusHost = "https://mobius.network";

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
        async public Task<StellarResponses.AssetResponse> StellarAsset(string assetIssuer = null, string assetCode = _assetCode) {
            if (_stellarAsset != null) return _stellarAsset;

            if (assetIssuer == null) assetIssuer = GetAssetIssuer();

            StellarRequests.Page<StellarResponses.AssetResponse> responses = 
                await HorizonClient.Assets.AssetIssuer(assetIssuer).AssetCode(assetCode).Execute();

            _stellarAsset = responses.Records.FirstOrDefault();

            return _stellarAsset;
        }

        ///<summary>Set Stellar network to use</summary>
        ///<param name="value">string: network to use</param>
        public void SetNetwork(string value = "TESTNET") {
            if (value == "TESTNET") Stellar.Network.UseTestNetwork();
            else if (value == "PUBLIC") Stellar.Network.UsePublicNetwork();
            else throw new Exception("Must provide value network. TESTNET or PUBLIC");

            _network = Stellar.Network.Current;
        }

        ///<summary>Get current network instance</summary>
        ///<returns>Returns StellarSdk.Network instance</returns>
        public Stellar.Network Network
        {
            get { return _network; }
        }

        ///<summary>Get StellarSdk.Server instance</summary>
        ///<returns>Returns StellarSdk.Server instance</returns>
        public Stellar.Server HorizonClient {
            get {
                if (_horizonClient != null) return _horizonClient;

                _horizonClient =
                    Stellar.Network.Current != null && 
                    Stellar.Network.IsPublicNetwork(Stellar.Network.Current)
                        ? new Stellar.Server(Urls["PUBLIC"])
                        : new Stellar.Server(Urls["TESTNET"]);

                return _horizonClient;
            }
        }

        ///<summary>Get Stellar Account Type for this account</summary>
        public Stellar.Account GetStellarAccount(Blockchain.Account account)
        {
            return new Stellar.Account(account.KeyPair(), null); // null sequnece number for now
        }
    }
}