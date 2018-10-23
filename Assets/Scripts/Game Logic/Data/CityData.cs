using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [System.Serializable]
    public class CityData
    {

        /// <summary>
        /// Center road tile of the city.
        /// </summary> 
        [SerializeField]
        public Vector3Int cityCenter;

        /// <summary>
        /// List of all blocks in the city.
        /// </summary>
        public List<CityBlock> cityBlocks;

        /// <summary>
        /// A 'map' of the city limits, where 49,49 is the center tile, using booleans. 
        /// Where true, the city has built something, or the land is otherwise occupied.
        /// </summary>
        public bool[,] cityMap;

        public int cityMapSize;

        /// <summary>
        /// Time in months between city growth.
        /// </summary>
        public int cityGrowthInterval = 9;

        public int monthsSinceLastExpansion = 1;

        [SerializeField]
        public Vector3Int cityMapCenter;

        /// <summary>
        /// Tracks current expansion band, where band 1 includes tiles a radius of initialTileFootprint/2 + bandSize from the cityCenter.
        /// </summary>
        public int currentExpansionBand = 0;

        public int initialConstruction = 0;

        public int bridgesBuilt = 0;

        public CityManager.RCI cityRCI;
    }
}
