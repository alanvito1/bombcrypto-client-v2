using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services {
    /// <summary>
    /// Utility class to centralize asset loading.
    /// Currently wraps Resources.Load, but prepared for migration to Addressables.
    /// Heavy folders identified for potential migration: Assets/Resources/BLMap, Assets/Resources/Prefabs, Assets/Resources/Sounds.
    /// </summary>
    public static class AssetLoader {
        /// <summary>
        /// Synchronously loads an asset from Resources.
        /// </summary>
        /// <param name="path">Path relative to Resources folder, without extension.</param>
        /// <typeparam name="T">Type of asset.</typeparam>
        /// <returns>The loaded asset.</returns>
        public static T Load<T>(string path) where T : Object {
            return Resources.Load<T>(path);
        }

        /// <summary>
        /// Asynchronously loads an asset from Resources.
        /// </summary>
        /// <param name="path">Path relative to Resources folder, without extension.</param>
        /// <typeparam name="T">Type of asset.</typeparam>
        /// <returns>The loaded asset.</returns>
        public static async UniTask<T> LoadAsync<T>(string path) where T : Object {
            var resourceRequest = Resources.LoadAsync<T>(path);
            await resourceRequest;
            return resourceRequest.asset as T;
        }
    }
}
