using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.World;

namespace Crops.Utilities
{
    public static class AssetBundleHelper
    {

        static List<AssetBundle> LoadedAssetBundles = new List<AssetBundle>();

        public static Dictionary<string, Tileset> LoadedTilesets = new Dictionary<string, Tileset>();

        /// <summary>
        /// Loads all asset bundles and incorporates their contents as needed.
        /// </summary>
        public static void InitializeAssetBundles()
        {
            LoadAssetBundles();
            LoadTilesets();
        }

        static void LoadAssetBundles()
        {
            List<string> bundlePaths = new List<string>();
            foreach (string path in Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, "Mods")))
            {
                // Skip path if it has an extension. So far, assetbundles have no extension.
                if (path.Contains("."))
                {
                    continue;
                }
                AssetBundle.LoadFromFile(path);
            }
        }

        /// <summary>
        /// Get all tilesets from loaded assetbundles.
        /// </summary>
        /// <returns></returns>
        static void LoadTilesets()
        {
            foreach(AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                foreach(GameObject obj in bundle.LoadAllAssets<GameObject>())
                {
                    if(obj.GetComponent<Tileset>() != null)
                    {
                        obj.GetComponent<Tileset>().InitializeDictionary();
                        LoadedTilesets.Add(obj.GetComponent<Tileset>().TilesetName, obj.GetComponent<Tileset>());
                    }

                    if (obj.GetComponent<CropSet>() != null)
                    {
                        Tileset.InitializeCropDictionary(obj.GetComponent<CropSet>().cropIndex);
                    }
                }

            }
        }
    }
}