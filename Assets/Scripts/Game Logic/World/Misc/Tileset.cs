using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.World
{
    [System.Serializable]
    public class Tileset : MonoBehaviour
    {

        public string TilesetName;
         
        /// <summary>
        /// Tileset objects and keys in a form visible in the inspector to allow easy access. 
        /// Becomes TilesetDictionary at runtime.
        /// </summary>   
        [System.Serializable]
        public struct TilesetIndex
        {
            public string key;
            public ScriptableObject scriptableObject;
        }

        /// <summary>
        /// Amalgamation of all following public TilesetIndex arrays. Used to construct tileset
        /// </summary>
        List<TilesetIndex> allObjects = new List<TilesetIndex>();

        [SerializeField]
        public TilesetIndex[] terrainObjects;

        [SerializeField]
        public TilesetIndex[] waterObjects;

        [SerializeField]
        public TilesetIndex[] roadObjects;

        [SerializeField]
        public TilesetIndex[] fenceObjects;

        [SerializeField]
        public TilesetIndex[] sceneryObjects;

        [SerializeField]
        public TilesetIndex[] farmBuildingObjects;

        [SerializeField]
        public TilesetIndex[] cityBuildingObjects;

        [SerializeField]
        public TilesetIndex[] weatherTypes;

        [System.Serializable]
        public struct Season
        {
            public string Name;
            public int StartDay;
            public int StartMonth;
        }

        /// <summary>
        /// Holds average monthly climate data for a region using this tileset, where index 0 is January and index 11 is December.
        /// Should be modified on a per-region basis.
        /// </summary>
        [System.Serializable]
        public struct ClimateData
        {
            public string Name;
            public float[] AveragePrecipitationInMM;
            public float[] AverageLowTempInCelcius;
            public float[] AverageHighTempInCelcius;
            public Season[] Seasons;
        }

        /// <summary>
        /// List of climate info this tileset has.
        /// </summary>
        public List<ClimateData> climateDataList;

        /// <summary>
        /// Climate currently in use.
        /// </summary>
        public static ClimateData activeClimateData;

        public Dictionary<string, ScriptableObject> Dictionary { get; } = new Dictionary<string, ScriptableObject>();

        public static Dictionary<string, ScriptableObject> TilesetDictionary { get; set; } = new Dictionary<string, ScriptableObject>();

        public static Dictionary<string, ScriptableObject> CropDictionary { get; set; } = new Dictionary<string, ScriptableObject>();

        public void Start()
        {
            AmalgamateTilesetIndexArrays();
            InitializeDictionary();
        }

        public void SetActiveClimate()
        {
            try
            {
                activeClimateData = climateDataList.Find(entry => entry.Name == GameManager.instance.gameData.gameInfo.climateName);
            }
            catch
            {
                activeClimateData = climateDataList[0];
            }
            if(activeClimateData.Name == "Null")
            {
                Debug.Log("Aborted");
                activeClimateData = climateDataList[0];
            }
        }

        public void SetActiveClimate(string climateName)
        {
            try
            {
                activeClimateData = climateDataList.Find(entry => entry.Name == climateName);
            }
            catch
            {
                activeClimateData = climateDataList[0];
            }
            if (activeClimateData.Name == "Null")
            {
                Debug.Log("Aborted");
                activeClimateData = climateDataList[0];
            }
        }

        public static T GetObjectDictionaryReference<T>(string objectDictionaryKey) where T : ScriptableObject
        {
            return TilesetDictionary[objectDictionaryKey] as T;
        }

        void AmalgamateTilesetIndexArrays()
        {

            allObjects.AddRange(terrainObjects);

            allObjects.AddRange(waterObjects);

            allObjects.AddRange(roadObjects);

            allObjects.AddRange(fenceObjects);

            allObjects.AddRange(sceneryObjects);

            foreach (TilesetIndex index in farmBuildingObjects)
            {
                ((BuildableObject)index.scriptableObject).InitTileSpritesArray();
            }
            allObjects.AddRange(farmBuildingObjects);

            allObjects.AddRange(weatherTypes);

            foreach(TilesetIndex index in cityBuildingObjects)
            {
                ((BuildableObject)index.scriptableObject).InitTileSpritesArray();
            }
            allObjects.AddRange(cityBuildingObjects);

        }

        public void InitializeDictionary()
        {
            AmalgamateTilesetIndexArrays();
            //Debug.Log("Tileset Dict. Created.");
            for (int i = 0; i < allObjects.Count; i++)
            {
                //Debug.Log($"Adding {allTiles[i].key},{allTiles[i].tile}");
                Dictionary.Add(allObjects[i].key, allObjects[i].scriptableObject);
            }
        }

        public static void InitializeCropDictionary(TilesetIndex[] cropSource)
        {
            for (int i = 0; i < cropSource.Length; i++)
            {
                CropDictionary.Add(cropSource[i].key, cropSource[i].scriptableObject);
            }
        }

        public static void SetActiveTilesetDictionary(Dictionary<string, ScriptableObject> tilesetDictionary)
        {
            TilesetDictionary = tilesetDictionary;
        }

    }
}
