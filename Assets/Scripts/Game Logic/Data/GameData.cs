using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.World;
using Crops.Utilities;
using Crops.Events;
using Crops.Economy;
using Crops.Construction;
using Crops.Players;

namespace Crops
{
    /// <summary>
    /// Holds all serializable game data for saving & loading.
    /// </summary>    
    [System.Serializable]
    public class GameData
    {
        // General Game Info
        /// <summary>
        /// Name of the saved game.
        /// </summary>
        public string GameName { get { return gameInfo.gameName; } set{ gameInfo.gameName = value; } }

        /// <summary>
        /// All info for this particular game.
        /// </summary>
        public GameInfo gameInfo;        

        // Data Classes
        public CityData cityData;

        public EconomyData economyData;

        /// <summary>
        /// Holds the playerData of each player.
        /// </summary>
        public List<PlayerData> playerData;

        public MapData mapData;

        public TimeData timeData;

        public WeatherData weatherData;


        /// <summary>
        /// Creates new game data with default settings.
        /// </summary>
        public GameData()
        {
            gameInfo = new GameInfo();
            cityData = new CityData()
            {
                cityBlocks = new List<CityBlock>()
            };
            economyData = new EconomyData();
            playerData = new List<PlayerData>();
            mapData = new MapData()
            {
                currentTilesetName = gameInfo.tilesetName,
                baseLandValue = gameInfo.baseLandValue,
                mapSize = gameInfo.mapSize
            };
            timeData = new TimeData()
            {
                date = new System.DateTime(1, 1, 1)
            };
            weatherData = new WeatherData();
        }
        /// <summary>
        /// Creates new game data with the given settings.
        /// </summary>
        /// <param name="newGameInfo"></param>
        public GameData(GameInfo newGameInfo)
        {
            gameInfo = newGameInfo;
            cityData = new CityData()
            {
                cityBlocks = new List<CityBlock>()
            };
            economyData = new EconomyData()
            {
                stateOfEconomy = newGameInfo.stateOfEconomy,
                interestRate = newGameInfo.interestRate,
                purchasePriceModifier = newGameInfo.purchasePriceModifier,
                salePriceModifier = newGameInfo.salePriceModifier
            };
            playerData = new List<PlayerData>();
            mapData = new MapData()
            {
                currentTilesetName = newGameInfo.tilesetName,
                baseLandValue = newGameInfo.baseLandValue,
                mapSize = newGameInfo.mapSize
            };
            timeData = new TimeData()
            {
                date = new System.DateTime(1, 1, 1)
            };
            weatherData = new WeatherData()
            {
                activeClimateName = newGameInfo.climateName,
            };
        }

    }
}
