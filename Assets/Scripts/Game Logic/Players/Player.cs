using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.Players;

namespace Crops
{
    public class Player : MonoBehaviour
    {
        /// <summary>
        /// Player display name.
        /// </summary>
        public string Name
        {
            get { return data.name; }
            private set { value = data.name; }
        }

        /// <summary>
        /// Unique player ID.
        /// </summary>
        public int ID
        {
            get { return data.ID; }
            private set { value = data.ID; }
        }

        /// <summary>
        /// Farm owned by this player.
        /// </summary>
        public FarmData Farm
        {
            get { return data.farmData; }
            set { data.farmData = value; }
        }

        /// <summary>
        /// If true, this is a human player.
        /// </summary>
        public bool IsHuman
        {
            get { return data.isHuman; } 
            set { data.isHuman = value; }            
        }

        /// <summary>
        /// Reference to specific player's data class.
        /// </summary>
        public PlayerData data;

        /// <summary>
        /// Create a new player with the given name and ID.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ID"></param>
        public Player(string playerName, int playerID, bool isHuman)
        {
            data = new PlayerData();
            Name = playerName;
            IsHuman = isHuman;
            ID = playerID;
        }

        public Player()
        {

        }


        /// <summary>
        /// Create a player from the given PlayerData.
        /// </summary>
        /// <param name="playerDataToLoad"></param>
        public Player(PlayerData playerDataToLoad)
        {
            data = playerDataToLoad;
        }

        public void SetPlayerName(string newName)
        {
            Name = newName;
        }

        public void SetPlayerID(int newID)
        {
            ID = newID;
        }

    }
}
