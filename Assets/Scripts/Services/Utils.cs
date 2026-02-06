using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Constant;

using Cysharp.Threading.Tasks;

using Senspark;

using Game.UI;

using JetBrains.Annotations;

using Scenes.ConnectScene.Scripts;

using Sfs2X.Util;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using Utils;

using Object = UnityEngine.Object;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace App {
    /// <summary>
    /// General purpose utility class for the application.
    /// Includes methods for networking, time formatting, store redirection, and session management.
    /// </summary>
    public static class Utils {
        private static readonly string[] SensitiveKeys = {
            "password", "token", "access_token", "refresh_token", "secret", "signature", "key", "wallet_hex",
            "private_key", "input_token", "api_key", "email"
        };

        private static readonly Regex JsonSensitiveKeysRegex = new Regex(
            $@"(""({string.Join("|", SensitiveKeys)})""\s*:\s*)""(?:[^""\\]|\\.)*""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex UrlSensitiveKeysRegex = new Regex(
            $@"([?&]({string.Join("|", SensitiveKeys)})=)([^&]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex UsernameRegex = new Regex(@"^[a-zA-Z0-9]{6,20}$", RegexOptions.Compiled);
        private static readonly Regex PasswordRegex = new Regex(@"^[^\s]{6,20}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string RedactSensitiveData(string json) {
            if (string.IsNullOrEmpty(json)) return json;
            try {
                return JsonSensitiveKeysRegex.Replace(json, "$1\"[REDACTED]\"");
            } catch (Exception) {
                return json; // Fallback to original if regex fails
            }
        }

        public static string RedactUrl(string url) {
            if (string.IsNullOrEmpty(url)) return url;
            try {
                return UrlSensitiveKeysRegex.Replace(url, "$1[REDACTED]");
            } catch (Exception) {
                return url;
            }
        }

        /// <summary>
        /// Opens the appropriate app store page based on the runtime platform.
        /// </summary>
        public static void GoToStore() {
            switch (Application.platform) {
                case RuntimePlatform.IPhonePlayer: {
                    Application.OpenURL("https://apps.apple.com/us/app/bombsquad/1673632517");
                    break;
                }
                case RuntimePlatform.Android: {
                    Application.OpenURL(
                        "https://play.google.com/store/apps/details?id=com.senspark.bomber.land.boom.battle.bombgames");
                    break;
                }
                default:
                    Application.OpenURL("https://app.bombcrypto.io/");
                    break;
            }
        }

        /// <summary>
        /// Retrieves the version code of the application (Android only).
        /// </summary>
        /// <returns>The version code int, or 20 for non-Android platforms.</returns>
        public static int GetVersionCode() {
#if UNITY_ANDROID
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var ca = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
            var pInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", Application.identifier, 0);
            return pInfo.Get<int>("versionCode");
#else
            return 20;
#endif
        }

        /// <summary>
        /// Polls the blockchain storage for a balance change.
        /// </summary>
        /// <param name="type">The token category to check.</param>
        /// <param name="blockchainManager">The blockchain manager instance.</param>
        /// <param name="blockchainStorage">The blockchain storage instance.</param>
        /// <returns>A tuple: (is balance changed ?, new balance)</returns>
        public static async Task<(bool, double)> WaitForBalanceChange(RpcTokenCategory type,
            IBlockchainManager blockchainManager, IBlockchainStorageManager blockchainStorage) {
            var coinBefore = blockchainStorage.GetBalance(type);
            for (var times = 0; times < 3; ++times) {
                var coinAfter = await blockchainManager.GetBalance(type);
                if (!MathUtils.Approximately(coinBefore, coinAfter)) {
                    return (true, coinAfter);
                }
                await WebGLTaskDelay.Instance.Delay(10000);
            }
            return (false, coinBefore);
        }

        /// <summary>
        /// Handles logout when initiated by Unity. Triggers a scene reload and notifies React.
        /// </summary>
        public static void KickToConnectScene() {
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            unityCommunication.ResetSession();
            ReloadToConnectScene();
            UniTask.Void(async () => {
                await unityCommunication.UnityToReact.SendToReact(ReactCommand.LOGOUT);
            });
        }
        
        /// <summary>
        /// Handles reload when initiated by React. Does NOT send a logout signal back to React.
        /// </summary>
        public static void ReloadByReact() {
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            unityCommunication.ResetSession();
            ReloadToConnectScene();
        }
        
        /// <summary>
        /// Logs out the user and resets the session.
        /// </summary>
        public static void Logout() {
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            unityCommunication.ResetSession();
            if (Application.isEditor || AppConfig.IsMobile()) {
                ReloadToConnectScene();
            } else {
                UniTask.Void(async () => {
                    await unityCommunication.UnityToReact.SendToReact(ReactCommand.LOGOUT);
                });
            }
        }

        private static void ReloadToConnectScene() {
            SceneManager.LoadScene(nameof(ConnectScene));
        }

        /// <summary>
        /// Formats a BCoin value with 4 decimal places.
        /// </summary>
        public static string FormatBcoinValue(double value) {
            return $"{value:#,0.####}";
        }
        
        /// <summary>
        /// Formats a base value with 6 decimal places.
        /// </summary>
        public static string FormatBaseValue(double value) {
            return $"{value:#,0.######}";
        }

        /// <summary>
        /// Converts a large number to a short string format (K, M, B, T).
        /// </summary>
        public static string ConvertToShortString(int value) {
            string[] suffixes = { "", "K", "M", "B", "T" };
            var suffixIndex = 0;
            while (Math.Abs(value) >= 1000f && suffixIndex < suffixes.Length - 1) {
                value /= 1000;
                suffixIndex++;
            }

            return value.ToString("F0") + suffixes[suffixIndex];
        }

        public static string GetDisconnectReason(string reason) {
            if (reason == ClientDisconnectionReason.UNKNOWN || reason == ClientDisconnectionReason.MANUAL) {
                return "Connection to the server is unstable, please wait 10 minutes, then login again";
            }
            if (reason == ClientDisconnectionReason.IDLE) {
                return "The account automatically exits because it has not been used for a long time";
            }
            return reason;
        }

        public static string FormatWalletId(string walletId) {
            return walletId.Length <= 10
                ? walletId
                : $"{walletId[..5]}...{walletId.Substring(walletId.Length - 4, 4)}";
        }

        /// <summary>
        /// Loads a Sprite from a local path or URL.
        /// </summary>
        public static async Task<Sprite> LoadImageFromPath(string path) {
            if (IsUrl(path)) {
                return await LoadImageFromUrl(path);
            }

            var data = await File.ReadAllBytesAsync(path);
            var txt = new Texture2D(2, 2);
            txt.LoadImage(data);
            return LoadImageFromTexture(txt);
        }

        /// <summary>
        /// Loads a Sprite from a web URL using UnityWebRequestTexture.
        /// </summary>
        public static async Task<Sprite> LoadImageFromUrl(string url) {
            if (string.IsNullOrWhiteSpace(url)) {
                return null;
            }

            var wr = UnityWebRequestTexture.GetTexture(url);
            await wr.SendWebRequest();
            if (wr.result != UnityWebRequest.Result.Success) {
                return null;
            }

            var txt = ((DownloadHandlerTexture) wr.downloadHandler).texture;
            return LoadImageFromTexture(txt);
        }

        public static Sprite LoadImageFromTexture(Texture2D data) {
            var spr = Sprite.Create(data, new Rect(0, 0, data.width, data.height), new Vector2(0.5f, 0.5f));
            return spr;
        }

        public static void ClearAllChildren(Transform container) {
            foreach (Transform child in container) {
                Object.Destroy(child.gameObject);
            }
        }

        public static bool IsUrl(string path) {
            return path.Contains("://");
        }

        /// <summary>
        /// Delays execution concurrently with a task.
        /// </summary>
        public static async Task IgnoreAfter<T>(this Task<T> task, int ms, ITaskDelay taskDelay = null) {
            taskDelay ??= WebGLTaskDelay.Instance;
            var delay = taskDelay.Delay(ms);
            await Task.WhenAny(task, delay);
        }

        /// <summary>
        /// Adds a timeout to a TaskCompletionSource.
        /// </summary>
        public static async Task<T> TimeoutAfter<T>(this TaskCompletionSource<T> source, int ms,
            ITaskDelay taskDelay = null) {
            taskDelay ??= WebGLTaskDelay.Instance;
            var mainTask = source.Task;
            var delay = taskDelay.Delay(ms);
            var completeTask = await Task.WhenAny(mainTask, delay);
            if (completeTask == delay) {
                var ex = new TimeoutException();
                if (mainTask.IsCanceled || mainTask.IsFaulted || mainTask.IsCompleted) {
                    throw ex;
                }
                source.SetException(ex);
            }
            return await mainTask;
        }

        /// <summary>
        /// Retries a function a specified number of times if it fails.
        /// </summary>
        public static async Task<T> Retry<T>(Func<Task<T>> func, int retryTime) {
            var retry = 0;
            while (retry <= retryTime) {
                try {
                    var result = await func();
                    return result;
                } catch (Exception) {
                    if (++retry > retryTime) {
                        throw;
                    }
                    Debug.Log($"Retry {retry}");
                }
            }
            throw new Exception("Failed");
        }

        /// <summary>
        /// Performs a GET request.
        /// </summary>
        public static async Task<(long, string)> GetWebResponse(ILogManager logManager, string url) {
            logManager.Log($"GET Web Request: {RedactUrl(url)}");
            using var request = UnityWebRequest.Get(url);
            var res = await AwaitWebResponse(request);
            logManager.Log($"result = {res}");
            return res;
        }

        /// <summary>
        /// Performs a GET request with custom headers.
        /// </summary>
        public static async Task<(long, string)> GetWebResponse(ILogManager logManager, string url, string addHeader,
            string addHeaderContent) {
            logManager.Log($"GET Web Request: {RedactUrl(url)}");
            logManager.Log($"GET header: {addHeader} {addHeaderContent}");
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader(addHeader, addHeaderContent);
            var res = await AwaitWebResponse(request);
            logManager.Log($"result = {res}");
            return res;
        }

        /// <summary>
        /// Performs a POST request with custom headers.
        /// </summary>
        public static async Task<(long, string)> PostWebResponse(ILogManager logManager, string url, string jsonBody,
            string addHeader, string addHeaderContent) {
            logManager.Log($"POST Web Request: {RedactUrl(url)}");
            logManager.Log($"POST body: {RedactSensitiveData(jsonBody)}");
            logManager.Log($"POST header: {addHeader} {addHeaderContent}");
            using var request = CreatePostWebRequest(url, jsonBody);
            request.SetRequestHeader(addHeader, addHeaderContent);
            var res = await AwaitWebResponse(request);
            logManager.Log($"result = {res}");
            return res;
        }

        /// <summary>
        /// Performs a POST request.
        /// </summary>
        public static async Task<(long, string)> PostWebResponse(ILogManager logManager, string url, string jsonBody) {
            logManager.Log($"POST Web Request: {RedactUrl(url)}");
            logManager.Log($"POST body: {RedactSensitiveData(jsonBody)}");
            using var request = CreatePostWebRequest(url, jsonBody);
            var res = await AwaitWebResponse(request);
            logManager.Log($"result = {res}");
            return res;
        }

        private static UnityWebRequest CreatePostWebRequest(string url, string jsonBody) {
            var request = new UnityWebRequest(url, "POST");
            var rawData = Encoding.UTF8.GetBytes(jsonBody);
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = new UploadHandlerRaw(rawData);
            request.downloadHandler = new DownloadHandlerBuffer();
            return request;
        }

        /// <summary>
        /// Awaits the UnityWebRequest and returns the response code and text.
        /// </summary>
        public static async Task<(long, string)> AwaitWebResponse(UnityWebRequest req) {
            await req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success) {
                return (req.responseCode, req.downloadHandler.text);
            }
            var result = req.downloadHandler.text;
            return (req.responseCode, result);
        }

        public static string CheckUsernameAndPassword(string username, string password) {
            if (string.IsNullOrEmpty(username) || !UsernameRegex.IsMatch(username)) {
                return "Invalid username. Check the policy again.";
            }
            if (string.IsNullOrEmpty(password) || !PasswordRegex.IsMatch(password)) {
                return "Invalid password. Check the policy again.";
            }
            return null;
        }

        public static string CheckEmail(string email) {
            if (string.IsNullOrEmpty(email) || !EmailRegex.IsMatch(email)) {
                return "Invalid email. Check the policy again.";
            }
            return null;
        }

        /// <summary>
        /// Checks if an IP address is within a given subnet.
        /// </summary>
        public static bool IsInSubnet(string ipString, string subnetMask) {
            var address = IPAddress.Parse(ipString);
            var slashIdx = subnetMask.IndexOf("/");
            if (slashIdx == -1) {
                // We only handle netmasks in format "IP/PrefixLength".
                throw new NotSupportedException("Only SubNetMasks with a given prefix length are supported.");
            }

            // First parse the address of the netmask before the prefix length.
            var maskAddress = IPAddress.Parse(subnetMask.Substring(0, slashIdx));

            if (maskAddress.AddressFamily != address.AddressFamily) {
                // We got something like an IPV4-Address for an IPv6-Mask. This is not valid.
                return false;
            }

            // Now find out how long the prefix is.
            var maskLength = int.Parse(subnetMask.Substring(slashIdx + 1));

            if (maskLength == 0) {
                return true;
            }

            if (maskLength < 0) {
                throw new NotSupportedException("A Subnetmask should not be less than 0.");
            }

            if (maskAddress.AddressFamily == AddressFamily.InterNetwork) {
                // Convert the mask address to an unsigned integer.
                var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);

                // And convert the IpAddress to an unsigned integer.
                var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);

                // Get the mask/network address as unsigned integer.
                var mask = uint.MaxValue << (32 - maskLength);

                // https://stackoverflow.com/a/1499284/3085985
                // Bitwise AND mask and MaskAddress, this should be the same as mask and IpAddress
                // as the end of the mask is 0000 which leads to both addresses to end with 0000
                // and to start with the prefix.
                return (maskAddressBits & mask) == (ipAddressBits & mask);
            }
            throw new NotSupportedException("Only InterNetworkV6 or InterNetwork address families are supported.");
        }

        public static async Task<string> GetTextFile(ILogManager logManager, string path) {
            if (IsUrl(path)) {
                var (code, res) = await GetWebResponse(logManager, path);
                return res;
            }
            return await File.ReadAllTextAsync(path);
        }

        public static string AppendTimeDay([NotNull] this string str, long seconds) {
            // TODO: Tạm tắt chờ update hiệu ứng xuống dòng
#if true
            return str;
#endif
            if (seconds <= 0) {
                return str;
            }
            // Add subs 1D, 7D, 30D
            // 86400 = total seconds 1 day
            str += $" {seconds / 86400000}D";
            return str;
        }
        
        public static string GetShortenName(string uname) {
            //DevHoang: Add new airdrop
            if (uname.EndsWith("ron") || 
                uname.EndsWith("bas") ||
                uname.EndsWith("vic")) {
                return uname.Substring(0, uname.Length - 3);
            }
            return uname;
        }
    }

    /// <summary>
    /// Utility for calculating grid layout sizes.
    /// </summary>
    public static class GridLayoutGroupUtil {
        public static Vector2Int GetColumnAndRow(GridLayoutGroup grid) {
            var itemsCount = grid.transform.childCount;
            var size = Vector2Int.zero;

            if (itemsCount == 0) {
                return size;
            }

            switch (grid.constraint) {
                case GridLayoutGroup.Constraint.FixedColumnCount:
                    size.x = grid.constraintCount;
                    size.y = GetAnotherAxisCount(itemsCount, size.x);
                    break;

                case GridLayoutGroup.Constraint.FixedRowCount:
                    size.y = grid.constraintCount;
                    size.x = GetAnotherAxisCount(itemsCount, size.y);
                    break;

                case GridLayoutGroup.Constraint.Flexible:
                    size = FlexibleSize(grid);
                    break;

                default:
                    throw new ArgumentOutOfRangeException($"Unexpected constraint: {grid.constraint}");
            }

            return size;
        }

        private static Vector2Int FlexibleSize(GridLayoutGroup grid) {
            var itemsCount = grid.transform.childCount;
            var prevX = float.NegativeInfinity;
            var xCount = 0;

            for (var i = 0; i < itemsCount; i++) {
                var pos = ((RectTransform) grid.transform.GetChild(i)).anchoredPosition;

                if (pos.x <= prevX) {
                    break;
                }

                prevX = pos.x;
                xCount++;
            }

            var yCount = GetAnotherAxisCount(itemsCount, xCount);
            return new Vector2Int(xCount, yCount);
        }

        private static int GetAnotherAxisCount(int totalCount, int axisCount) {
            return totalCount / axisCount + Mathf.Min(1, totalCount % axisCount);
        }
    }

    public static class ColorTypeConverter {
        public static string ToRGBHex(Color c) {
            return $"#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";
        }

        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte) (f * 255);
        }
        
        public static Color ToHexRGB(string hex) {
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }
            var r = ConvertHexToFloat(hex.Substring(0, 2));
            var g = ConvertHexToFloat(hex.Substring(2, 2));
            var b = ConvertHexToFloat(hex.Substring(4, 2));
            return new Color(r, g, b);
        }
        
        private static float ConvertHexToFloat(string hex)
        {
            return int.Parse(hex, System.Globalization.NumberStyles.HexNumber) / 255f;
        }
    }

    /// <summary>
    /// Thêm vào để dùng tính năng TracePoint của Rider
    /// </summary>
    public static class RiderUtil {
        [JetBrains.Annotations.UsedImplicitly]
        public static T Log<T>(T s) {
            Debug.Log(s);
            return s;
        }
    }

    /// <summary>
    /// Helper for formatting time durations.
    /// </summary>
    public static class TimeUtil {
        public static string ConvertTimeToString(long duration) {
            return ConvertTimeToString(TimeSpan.FromMilliseconds(duration));
        }

        public static string ConvertTimeToStringDay(TimeSpan timeSpan) {
            if (timeSpan.Days > 0) {
                if (timeSpan.Days == 1) {
                    return "1 day";
                } else {
                    return $@"{timeSpan.Days} days";
                }
            } else {
                return "0 day";
            }
        }

        public static string ConvertTimeToStringDhm(TimeSpan timeSpan) {
            var r = "";
            var isDidAdd = false;
            if (timeSpan.Days > 0) {
                r += $@"{timeSpan.Days}d";
                isDidAdd = true;
            }
            if (timeSpan.Hours > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Hours}h";
                isDidAdd = true;
            }
            if (timeSpan.Minutes > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Minutes}m";
                isDidAdd = true;
            }
            if (!isDidAdd) {
                return "0M";
            }
            return r;
        }
        
        public static string ConvertTimeToString(TimeSpan timeSpan) {
            var r = "";
            var isDidAdd = false;
            if (timeSpan.Days > 0) {
                r += $@"{timeSpan.Days}d";
                isDidAdd = true;
            }
            if (timeSpan.Hours > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Hours}h";
                isDidAdd = true;
            }
            if (timeSpan.Minutes > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Minutes}m";
                isDidAdd = true;
            }
            if (timeSpan.Seconds > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Seconds}s";
                isDidAdd = true;
            }
            if (!isDidAdd) {
                return "0s";
            }
            return r;
        }

        public static string ConvertTimeToStringFull(long duration) {
            return ConvertTimeToStringFull(TimeSpan.FromMilliseconds(duration));
        }

        public static string ConvertTimeToStringFull(TimeSpan timeSpan) {
            var r = "";
            var isDidAdd = false;
            if (timeSpan.Days > 0) {
                r += $@"{timeSpan.Days} Day";
                if (timeSpan.Days > 1) {
                    r += "s";
                }
                isDidAdd = true;
            }
            if (timeSpan.Hours > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Hours} Hour";
                if (timeSpan.Hours > 1) {
                    r += "s";
                }
                isDidAdd = true;
            }
            if (timeSpan.Minutes > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Minutes} Minute";
                if (timeSpan.Minutes > 1) {
                    r += "s";
                }
                isDidAdd = true;
            }
            if (timeSpan.Seconds > 0) {
                if (isDidAdd) {
                    r += " ";
                }
                r += $@"{timeSpan.Seconds} Second";
                if (timeSpan.Seconds > 1) {
                    r += "s";
                }
                isDidAdd = true;
            }
            if (!isDidAdd) {
                return "0 Second";
            }
            return r;
        }
    }

    public class TimeTick {
        private readonly Action _call;
        private readonly float _duration;
        private float _countDown;

        public TimeTick(float countDown, Action call) {
            _call = call;
            _duration = countDown;
            _countDown = countDown;
        }

        public void Update(float dt) {
            _countDown -= dt;
            if (_countDown <= 0) {
                _countDown += _duration;
                _call?.Invoke();
            }
        }

        public void Call() {
            _call?.Invoke();
        }
    }

    public static class Utility {
        public static T Pop<T>(this IList<T> list) {
            if (!list.Any<T>()) {
                throw new InvalidOperationException("Attempting to pop item on empty list.");
            }
            var index = list.Count - 1;
            var obj = list[index];
            list.RemoveAt(index);
            return obj;
        }

        public static bool TryGetValue<T>(
            this IDictionary<string, object> dictionary,
            string key,
            out T value) {
            object obj1;
            if (dictionary.TryGetValue(key, out obj1) && obj1 is T obj2) {
                value = obj2;
                return true;
            }
            value = default(T);
            return false;
        }
    }
}