using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.World
{
    [Serializable]
    public class MapData
    {

        /// <summary>
        /// Name of the tileset in use for this map.
        /// </summary>
        public string currentTilesetName;

        /// <summary>
        /// Holds all LandPlots.
        /// </summary>
        public LandPlot[,] mapData;

        /// <summary>
        /// Holds data for all buildable objects. Used for serialization.
        /// </summary>
        public List<BuildableObjectData> buildableObjectDataList;

        // BuildableObject List

            // Item List

        /// <summary>
        /// Starting point for land value assignment.
        /// </summary>
        public float baseLandValue;

        /// <summary>
        /// Map size. Map is always a square (mapSize * mapSize).
        /// </summary>
        public int mapSize = 200;

    }
}
