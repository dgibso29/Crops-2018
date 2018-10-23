using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.Players;

namespace Crops
{
    public class PlayerManager : MonoBehaviour, ILoadable
    {
        public GameManager gameManager;

        public GameObject playerPrefab;

        /// <summary>
        /// Master list of all players.
        /// </summary>
        public List<Player> PlayerList = new List<Player>();

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Add a new player with the given info.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="playerID"></param>
        public void AddNewPlayer(string playerName, int playerID, bool isHuman)
        {
            // Create the new player data
            PlayerData newPlayerData = new PlayerData(playerName, playerID, isHuman);
            // Instantiate the player prefab
            GameObject newPlayerObject = Instantiate(playerPrefab);
            // Get the prefab's Player component
            Player newPlayer = newPlayerObject.GetComponent<Player>();

            // Do things if not human
            if (!isHuman)
            {

            }
            // Do things if human
            else
            {

            }

            // Set the component's data to the new data            
            newPlayer.data = newPlayerData;
            // Add the player and playerdata to their respective lists.
            gameManager.gameData.playerData.Add(newPlayerData);
            PlayerList.Add(newPlayer);
        }

        /// <summary>
        /// Add a new player with the given playerData.
        /// </summary>
        /// <param name="playerData"></param>
        public void AddNewPlayer(PlayerData playerData)
        {
            // Instantiate the player prefab
            GameObject newPlayerObject = Instantiate(playerPrefab);
            // Get the prefab's Player component
            Player newPlayer = newPlayerObject.GetComponent<Player>();

            // Do things if not human
            if (!playerData.isHuman)
            {

            }
            // Do things if human
            else
            {

            }

            // Set the component's data to the given data            
            newPlayer.data = playerData;
            // Add the player and playerdata to their respective lists.
            gameManager.gameData.playerData.Add(playerData);
            PlayerList.Add(newPlayer);
        }

        Player GetNewPlayer(PlayerData playerData)
        {       
            // Instantiate the player prefab
            GameObject newPlayerObject = Instantiate(playerPrefab);

            // Get the prefab's Player component
            Player newPlayer = newPlayerObject.GetComponent<Player>();

            // Do things if not human
            if (!playerData.isHuman)
            {

            }
            // Do things if human
            else
            {

            }

            // Set the component's data to the given data            
            newPlayer.data = playerData;
            return newPlayer;
        }


        /// <summary>
        /// Add the given player to the player list.
        /// </summary>
        /// <param name="playerToAdd"></param>
        public void AddPlayer(Player playerToAdd)
        {
            if (!PlayerList.Contains(playerToAdd))
            {
                PlayerList.Add(playerToAdd);
            }
        }

        public Player GetPlayer(int playerID)
        {
            return PlayerList.Find(entry => entry.ID == playerID);
        }

        public void InitializeFromSave()
        {
            foreach(PlayerData data in gameManager.gameData.playerData)
            {
                AddPlayer(GetNewPlayer(data));
            }
        }

    }
}
