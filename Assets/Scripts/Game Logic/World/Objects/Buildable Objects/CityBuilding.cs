using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [System.Serializable]
    [CreateAssetMenu]
    public class CityBuilding : BuildableObject
    {

        CityBuildingData data;               

        /// <summary>
        /// Zone to which this belongs.
        /// </summary>
        public BlockZoneType Zone
        {
            get { return data.zone; }
            set { data.zone = value; }
        }

        /// <summary>
        /// Parameters which will result in the construction of this.
        /// </summary>
        public ZoningParameters Parameters
        {
            get { return data.parameters; }
            set { data.parameters = value; }
        }

        public void InitializeObject(Vector2Int footprint, Vector3Int originTile, int objectVersion,
            string dictionaryKey, int uniqueID, BlockZoneType zoneType, ZoningParameters zoneParameters, int ownerID = 0)
        {
            // Call base initialization
            InitializeObject(footprint, originTile, objectVersion,
            dictionaryKey, uniqueID, ownerID);

            Zone = zoneType;
            Parameters = zoneParameters;

        }

        public void InitializeObject(CityBuildingData objectData, ref Dictionary<string, Sprite[][,]> tileSprites)
        {
            data = objectData;
            this.tileSprites = tileSprites;
            arrayInitialized = true;
        }
    }
}
