using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Crops.World;
using Crops.Utilities;
using Crops.Events;
using Crops.Economy;
using Crops.Construction;
using Crops.Players;
using Crops.UI;

namespace Crops
{
    /// <summary>
    /// Manages all in-game functions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public MasterManager masterManager = MasterManager.instance;

        public Map worldMap;
        public LandManager land;
        public CityManager city;
        public TimeManager time;
        public EventManager events;
        public EconomyManager economy;
        public ConstructionManager construction;
        public PlayerManager players;
        public CameraController cameraController;
        public WeatherManager weather;

        /// <summary>
        /// Current game's data.
        /// </summary>
        public GameData gameData;        

        /// <summary>
        /// List of all constructed buildable objects of all types.
        /// </summary>
        public List<BuildableObjectData> MasterBuildableObjectList = new List<BuildableObjectData>();

        public static Tileset CurrentTileset;

        // Use this for initialization
        void Start()
        {
            //AssetBundleHelper.InitializeAssetBundles();
            //masterManager = FindObjectOfType<MasterManager>();
            //StartNewGame(new GameSettings());
           
        }

        private void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if(instance == null)
            {
                instance = this;
            }
        }
        
        #region Save/Load
        /// <summary>
        /// Save the current game.
        /// </summary>
        public void SaveGame()
        {
            Debug.Log("Saving game");
            IOHelper.SaveGameToDisk(gameData, "testSave");
        }
        
        /// <summary>
        /// Load the specified game.
        /// </summary>
        void LoadGame(string saveName)
        {
            gameData = IOHelper.LoadGameFromDisk(saveName);
            Debug.Log(gameData.gameInfo.timeSaved);
            Debug.Log(gameData.cityData.cityBlocks[0].BlockOriginTile);

        }

        #endregion

        #region Game Start

        /// <summary>
        /// Starts a new game with the given Game Settings.
        /// </summary>
        public void StartNewGame(GameInfo gameSettings)
        {
            gameData = new GameData(gameSettings);
            worldMap.StartNewMap();
            CurrentTileset = worldMap.currentTileset;        
            time.Initialize();
            weather.Initialize();
            players.AddNewPlayer("Player One", 1, true);
            // Make initial save
        }

        /// <summary>
        /// Loads a game with the given save file.
        /// </summary>
        public void StartLoadedGame(string saveName)
        {
            LoadGame(saveName); /// Done
            worldMap.InitializeFromSave(); /// Done
            CurrentTileset = worldMap.currentTileset; /// Done
            city.InitializeFromSave(); /// Done?
            time.Initialize(); /// Done
            players.InitializeFromSave();
        }

        #endregion

        #region Time-Based Checks

        /// <summary>
        /// Checks to perform each new day.
        /// </summary>
        public void CheckOnNewDay()
        {
            //Debug.Log("New Day! Day is " + time.CurrentDay);
        }
        /// <summary>
        /// Checks to perform each new week.
        /// </summary>
        public void CheckOnNewWeek()
        {
            weather.AdvanceWeatherOneWeek();
            Debug.Log("New week! Week is W" + time.CurrentWeek);
        }
        /// <summary>
        /// Checks to perform each new month.
        /// </summary>
        public void CheckOnNewMonth()
        {
            //Debug.Log("New Month! Month is " + timeManager.GetRawDate.Month.ToString());
            city.CheckForCityExpansion();

            economy.CheckForStateEvaluation();
        }
        /// <summary>
        /// Checks to perform each new year.
        /// </summary>
        public void CheckOnNewYear()
        {
            //Debug.Log("New year! Year is " + timeManager.GetRawDate.Year.ToString());
        }
        /// <summary>
        /// Checks to perform each new season.
        /// </summary>
        public void CheckOnNewSeason()
        {
            Debug.Log($"New season! It's now {time.CurrentSeason}!");
        }

        #endregion

        /// <summary>
        /// Returns unique ID for object.
        /// </summary>
        /// <returns></returns>
        public int AssignUniqueID()
        {
            int ID = gameData.gameInfo.nextUniqueID;
            gameData.gameInfo.nextUniqueID++;
            return ID;
        }

        /// <summary>
        /// Returns the player with the given ID.
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public Player GetPlayer(int ID)
        {
            return players.GetPlayer(ID);
        }

        /// <summary>
        /// Returns the object with the given ID, if there is one.
        /// </summary>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public BuildableObject GetObject(int objectID)
        {            
            return worldMap.GetBuildableObjectWithID<BuildableObject>(objectID);
        }

        public Vector3Int GetCurrentTileCoords()
        {
            return worldMap.currentTileCoordinates;
        }

        public LandPlot GetCurrentLandPlot()
        {
            return worldMap.GetCurrentLandPlot();
        }

    }
}
