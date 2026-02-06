using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

using Exceptions;

using Game.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using PvpSchedule.Models;

using UnityEngine;
using UnityEngine.Networking;

using Application = UnityEngine.Device.Application;

namespace App {
    /// <summary>
    /// Manages API communication with the backend server.
    /// Handles requests for coin balance, server time, CCU, and PvP room/match information.
    /// Implements <see cref="IApiManager"/>.
    /// </summary>
    public class DefaultApiManager : IApiManager {
        // API Endpoint Constants
        private const string GET_COIN_BALANCE = "coin_balance?address=";
        private const string GET_CCU = "ccu";
        private const string GET_PVP_ROOM_LIST = "pvp-matching-2/tournament/room/status";
        private const string GET_PVP_MATCHES = "pvp-matching-2/tournament/status";
        private const string GET_MY_MATCHES = "pvp-matching-2/tournament/my-matches";

        private readonly string ApiHost;
        private readonly string ApiTestHost;
        private const string BASE_API_TEST_HOST_LOCAL = "http://localhost:8101/";

        /// <summary>
        /// Gets the current domain based on the production flag.
        /// </summary>
        public string Domain => _isProduction ? ApiHost : ApiTestHost;

        private readonly bool _isProduction;
        private readonly ILogManager _logManager;
        private readonly string _network;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiManager"/> class.
        /// Configures the API hosts based on the network configuration and production flag.
        /// </summary>
        /// <param name="networkConfig">Configuration for the current network (e.g., Binance, Polygon).</param>
        /// <param name="logManager">Manager for logging operations.</param>
        /// <param name="isProduction">If true, uses production API hosts; otherwise, uses test hosts.</param>
        public DefaultApiManager(INetworkConfig networkConfig, ILogManager logManager, bool isProduction) {
            _logManager = logManager;
            _isProduction = isProduction;
            _network = networkConfig.NetworkName;

            // Determine API Host based on configuration
            if (AppConfig.IsTournament()) {
                ApiHost = AppConfig.TournamentBaseApiHost;
                ApiTestHost = Application.isEditor ? BASE_API_TEST_HOST_LOCAL : AppConfig.BaseApiTestHost;
            } else {
                if (networkConfig.NetworkType == NetworkType.Binance) {
                    ApiHost = AppConfig.BaseApiHost;
                    ApiTestHost = AppConfig.BaseApiTestHost;
                } else {
                    ApiHost = $"{AppConfig.BaseApiHost}{networkConfig.NetworkName}/";
                    ApiTestHost = $"{AppConfig.BaseApiTestHost}{networkConfig.NetworkName}/";
                }
            }
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        /// <summary>
        /// Retrieves the coin balance for a specific wallet address.
        /// </summary>
        /// <param name="walletAddress">The wallet address to query.</param>
        /// <returns>The balance as a double.</returns>
        /// <exception cref="Exception">Thrown when the API returns an error code or an empty response.</exception>
        public async Task<double> GetCoinBalance(string walletAddress) {
            var url = GetHost(Domain, GET_COIN_BALANCE, Uri.EscapeDataString(walletAddress));

            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            var message = "Could not get BCoin Balance";
            if (!string.IsNullOrEmpty(res)) {
                var data = JObject.Parse(res);
                // Validate response structure
                if (data["code"] != null && data["message"] != null) {
                    // Check for success code (0)
                    if (data["code"].Value<int>() == 0) {
                        var result = data["message"].Value<double>();
                        return result;
                    }
                    message = data["message"].Value<string>();
                }
            }
            throw new Exception(message);
        }

        /// <summary>
        /// Checks if the client's local time is synchronized with the server time.
        /// </summary>
        /// <returns>True if the time difference is within acceptable limits (10 minutes).</returns>
        /// <exception cref="Exception">Thrown if the time difference is greater than 10 minutes.</exception>
        public async Task<bool> CheckServerTime() {
            long longTime = -1;
            try {
                longTime = await RequestServerUnixTime();
            } catch (Exception e) {
                // ignore
            }
            // if (longTime < 0) {
            //     throw new NoInternetException();
            // }

            var unixTime = DateTimeOffset.FromUnixTimeMilliseconds(longTime).DateTime;
            var serverTime = TimeZoneInfo.ConvertTimeFromUtc(unixTime, TimeZoneInfo.Local);
            var clientTime = DateTime.Now;
            var difference = DateTime.Now - serverTime;

            _logManager.Log(
                $"ServerTime: {serverTime}, ClientTime: {clientTime}, Difference: {difference.TotalSeconds}");

            if (Math.Abs(difference.TotalMinutes) > 10) {
                throw new Exception("Error logging in, your computer time is not correct");
            }

            return true;
        }

        [Obsolete("This method is not used anymore")]
        public async Task<long> RequestServerUnixTime() {
            // var url = GetHost(BASE_API_HOST, GET_TIME);
            //
            // using var request = UnityWebRequest.Get(url);
            // request.timeout = 3;
            // await request.SendWebRequest();
            //
            // if (request.result == UnityWebRequest.Result.Success) {
            //     var obj = JObject.Parse(request.downloadHandler.text);
            //     request.Dispose();
            //     if (obj["code"] != null && obj["code"].Value<int>() == 0 && obj["message"] != null) {
            //         return obj["message"].Value<long>();
            //     }
            // }
            //
            // request.Dispose();
            return -1;
        }

        /// <summary>
        /// Gets the Concurrent Users (CCU) and Maximum CCU from the server.
        /// </summary>
        /// <returns>A tuple containing (CCU, MaxCCU). Returns (0, 0) on failure.</returns>
        public async Task<(int, int)> GetCcu() {
            var url = GetHost(Domain, GET_CCU);

            var (code, res) = await Utils.GetWebResponse(_logManager, url);
            if (!string.IsNullOrWhiteSpace(res)) {
                var obj = JObject.Parse(res);
                var msg = (JObject)obj["message"];
                if (msg?["ccu"] != null && msg["maxCcu"] != null) {
                    return (msg["ccu"].Value<int>(), msg["maxCcu"].Value<int>());
                }
            }
            return (0, 0);
        }

        /// <summary>
        /// Helper to construct the full API URL.
        /// </summary>
        private static string GetHost(string host, string command, string param = null) {
            return param == null
                ? $"{host}{command}"
                : $"{host}{command}{param}";
        }

        /// <summary>
        /// Retrieves the list of available PvP rooms for the tournament.
        /// </summary>
        /// <returns>A list of <see cref="IPvpRoomInfo"/> objects.</returns>
        public async Task<List<IPvpRoomInfo>> GetPvpRoomList() {
            var url = GetHost(Domain, GET_PVP_ROOM_LIST);
            var (code, response) = await Utils.GetWebResponse(_logManager, url);
            var infoList = new List<IPvpRoomInfo>();
            if (string.IsNullOrWhiteSpace(response)) {
                return infoList;
            }
            var obj = JObject.Parse(response);
            var msg = (JObject)obj["message"];
            var details = (JArray)msg["details"];
            foreach (var item in details) {
                var zone = (JObject)item;
                var rooms = (JArray)zone["rooms"];
                foreach (var item2 in rooms) {
                    var info = JsonConvert.DeserializeObject<PvpRoomInfo>(item2.ToString());
                    infoList.Add(info);
                }
            }
            return infoList;
        }

        /// <summary>
        /// Retrieves the schedule of PvP matches.
        /// </summary>
        /// <returns>A list of <see cref="IPvpMatchSchedule"/> objects.</returns>
        public async Task<List<IPvpMatchSchedule>> GetPvpMatches() {
            var url = GetHost(Domain, GET_PVP_MATCHES);
            var (code, response) = await Utils.GetWebResponse(_logManager, url);
            var matchList = new List<IPvpMatchSchedule>();
            if (string.IsNullOrWhiteSpace(response)) {
                return matchList;
            }
            var obj = JObject.Parse(response);
            var message = (JObject)obj["message"];
            var matches = (JArray)message["matches"];
            foreach (var item in matches) {
                var match = JsonConvert.DeserializeObject<PvpMatchSchedule>(item.ToString());
                matchList.Add(match);
            }
            return matchList;
        }
        
        /// <summary>
        /// Retrieves the match history for a specific user.
        /// </summary>
        /// <param name="userName">The username to query matches for.</param>
        /// <returns>A list of match IDs or descriptions (strings).</returns>
        public async Task<List<string>> GetMyMatches(string userName) {
            var url = GetHost(Domain, GET_MY_MATCHES);
            var body = new JObject(){
                {"username", userName}
            };

            var (code, response) = await Utils.PostWebResponse(_logManager, url, body.ToString());
            var matchList = new List<string>();
            if (string.IsNullOrWhiteSpace(response)) {
                return matchList;
            }
            var obj = JObject.Parse(response);

            var message = (JObject)obj["message"];
            var matches = (JArray)message["my_match"];
            foreach (var item in matches) {
                var match = JsonConvert.DeserializeObject<string>(item.ToString());
                matchList.Add(match);
            }
            return matchList;
        }
    }
}