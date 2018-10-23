using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.Players
{
    [Serializable]
    public class PlayerData
    {
        /// <summary>
        /// Player display name.
        /// </summary>
        public string name;

        /// <summary>
        /// Unique player ID.
        /// </summary>
        public int ID;

        public FarmData farmData;

        public float funds;

        /// <summary>
        /// If true, this is a human player.
        /// </summary>
        public bool isHuman;

        public PlayerData(string playerName, int playerID, bool isHuman)
        {
            name = playerName;
            this.isHuman = isHuman;
            ID = playerID;
            farmData = new FarmData();
        }

        public PlayerData()
        {

        }


    }
}
