using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.World
{ 
    public class CityBuildingData : BuildableObjectData
    {

        /// <summary>
        /// Zone to which this belongs.
        /// </summary>
        public BlockZoneType zone;

        /// <summary>
        /// Parameters which will result in the construction of this.
        /// </summary>
        public ZoningParameters parameters = ZoningParameters.None;

        public CityBuildingData(Vector2Int footprint, Vector3Int originTile, int objectVersion, string dictionaryKey, int uniqueID, int ownerID, BuildableObjectType type,
            BlockZoneType zoneType, ZoningParameters zoneParameters) : base(footprint, originTile, objectVersion, dictionaryKey, uniqueID, ownerID)
        {
            zone = zoneType;
            parameters = zoneParameters;
        }
    }
}
