using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.Economy;

namespace Crops
{
    /// <summary>
    /// All game info. 
    /// </summary>
    [System.Serializable]
    public class GameInfo
    {
        /// <summary>
        /// Name of the saved game.
        /// </summary>
        public string gameName;

        public string tilesetName;
        
        /// <summary>
        /// Name of the climate to use for this game.
        /// </summary>
        public string climateName;

        /// <summary>
        /// Used for assigning unique IDs to all in-game objects.
        /// </summary>
        public int nextUniqueID;

        /// <summary>
        /// Time/Date at which the game was saved.
        /// </summary>
        public System.DateTime timeSaved;

        #region Map Settings

        public int mapSize;

        public float baseLandValue;

        public int[] moistureBandBreakpointDefaults;

        #endregion

        #region Economy Settings
        /// <summary>
        /// Current state of the economy.
        /// </summary>
        public EconomyState stateOfEconomy;

        /// <summary>
        /// Percentage by which to modify sale prices.
        /// </summary>
        public float salePriceModifier;

        /// <summary>
        /// Percentage by which to modify purchase prices.
        /// </summary>
        public float purchasePriceModifier;

        /// <summary>
        /// Current interest rate. Applies to loans.
        /// </summary>
        public float interestRate;
        #endregion

        /// <summary>
        /// Creates new game settings with default values.
        /// </summary>
        public GameInfo()
        {
            nextUniqueID = 1;
            gameName = "testSave";
            tilesetName = "Midwest";
            climateName = "Climate_Normal";
            mapSize = 200;
            baseLandValue = 1000;
            moistureBandBreakpointDefaults = new int[4] { 8, 36, 80, 128 };
            stateOfEconomy = EconomyState.Stable;
            salePriceModifier = 0.95f;        
            purchasePriceModifier = 1.05f;
            interestRate = 0.05f;           
        }

    }
}
