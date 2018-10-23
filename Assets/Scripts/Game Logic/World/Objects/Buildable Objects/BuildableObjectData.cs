using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    /// <summary>
    /// Base Data class for all objects that can be built, ranging from trees to buildings to fields.
    /// </summary>
    [System.Serializable]    
    public class BuildableObjectData
    {
        /// <summary>
        /// Object footprint for serialization.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        public Vector2IntJSON footprint;

        /// <summary>
        /// Object origin tile coordinates for serialization.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        public Vector2IntJSON originCoords;

        /// <summary>
        /// Unique ID of this object.
        /// </summary>       
        public int ID;

        /// <summary>
        /// ID of this building's owner. -1 is the city; 0 is none.
        /// </summary>
        public int ownerID = 0;

        /// <summary>
        /// Tile dictionary key for this object type.
        /// </summary>
        public string tileDictionaryKey;

        /// <summary>
        /// Variant of this object's type that is used.
        /// </summary>
        public int variant;

        //public BuildableObjectType buildableObjectType;

        public BuildableObjectData()
        {

        }

        public BuildableObjectData(Vector2Int footprint, Vector3Int originTile, int objectVersion, string dictionaryKey, int uniqueID, int ownerID)
        {
            this.footprint = new Vector2IntJSON(footprint);
            originCoords = new Vector2IntJSON(originTile);
            tileDictionaryKey = dictionaryKey;
            variant = objectVersion;
            ID = uniqueID;
            this.ownerID = ownerID;
            //buildableObjectType = type;
        }
    }
}