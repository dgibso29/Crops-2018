using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using Crops;

namespace Crops.World
{
    /// <summary>
    /// Representation of one map tile.
    /// </summary>
    [System.Serializable]
    public class LandPlot
    {
        public bool IsOwnedByPlayer;

        public bool IsOwnedByCity;

        public bool HasFence;

        public bool HasRoad;

        public bool IsCrosswalk;

        /// <summary>
        /// If true, plot is water.
        /// </summary>
        public bool IsWater;

        /// <summary>
        /// Unique ID of the player owning this plot, if any. Value is -1 when un-owned.
        /// </summary>
        public int OwnerID;

        /// <summary>
        /// Dictionary key of the fence on this tile, if there is one.
        /// </summary>
        public string FenceDictionaryKey;

        /// <summary>
        /// Dictionary key of the road on this tile, if there is one.
        /// </summary>
        public string RoadDictionaryKey;

        /// <summary>
        /// Base land value assigned at start of game.
        /// </summary>
        public float BaseLandValue;

        /// <summary>
        /// String key of this tile's terrain type.
        /// </summary>
        public string TerrainTileTypeKey;

        /// <summary>
        /// Variant index of this terrain tile.
        /// </summary>
        public int TerrainTileVariant;

        /// <summary>
        /// Rotation of the terrain tile.
        /// </summary>
        public float TerrainTileRotation;

        /// <summary>
        /// Current land value.
        /// </summary>
        public float CurrentLandValue;

        /// <summary>
        /// ID of the object on the tile, if there is one. Defaults to 0 if no object.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public int ObjectOnTileID = 0;

        [Newtonsoft.Json.JsonIgnore]
        public Vector2Int PlotCoordinates2D
        {
            get { return coordinates.ToVector2Int(); }
            set { coordinates = new Vector2IntJSON(value); }
        }

        [Newtonsoft.Json.JsonIgnore]
        public Vector3Int PlotCoordinates3D
        {
            get { return coordinates.ToVector3Int(); }
            set { coordinates = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Plot coordinates for serialization.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        Vector2IntJSON coordinates;

        #region Custom JSON Reader & Writer

        /// <summary>
        /// Serializes the given LandPlot to Json.
        /// </summary>
        /// <param name="plot"></param>
        /// <returns></returns>
        public static string ToJson(LandPlot plot)
        {           
            System.IO.StringWriter sw = new System.IO.StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            writer.WritePropertyName("OwnedByPlayer");
            writer.WriteValue(plot.IsOwnedByPlayer);

            writer.WritePropertyName("OwnedByCity");
            writer.WriteValue(plot.IsOwnedByCity);

            writer.WritePropertyName("Fence");
            writer.WriteValue(plot.HasFence);

            writer.WritePropertyName("Road");
            writer.WriteValue(plot.HasRoad);

            writer.WritePropertyName("Crosswalk");
            writer.WriteValue(plot.IsCrosswalk);

            writer.WritePropertyName("Water");
            writer.WriteValue(plot.IsWater);

            writer.WritePropertyName("OwnerID");
            writer.WriteValue(plot.OwnerID);

            writer.WritePropertyName("FenceDictKey");
            writer.WriteValue(plot.FenceDictionaryKey);

            writer.WritePropertyName("RoadDictKey");
            writer.WriteValue(plot.RoadDictionaryKey);

            writer.WritePropertyName("BaseLandVal");
            writer.WriteValue(plot.BaseLandValue);

            writer.WritePropertyName("TerrTileKey");
            writer.WriteValue(plot.TerrainTileTypeKey);

            writer.WritePropertyName("TerrTileVar");
            writer.WriteValue(plot.TerrainTileVariant);

            writer.WritePropertyName("TerrTileRot");
            writer.WriteValue(plot.TerrainTileRotation);

            writer.WritePropertyName("CurrLandVal");
            writer.WriteValue(plot.CurrentLandValue);

            writer.WritePropertyName("Coords");
            writer.WriteValue(plot.coordinates);

            writer.WriteEndObject();

            return sw.ToString();
        }

        #endregion


        public LandPlot(bool isOwnedByPlayer, bool isOwnedByCity, bool hasFence, bool isWater, int ownerID, string fenceDictionaryKey, 
            string terrainTileTypeKey, int terrainTileVariant, float terrainTileRotation, float baseLandValue, 
            Vector2Int plotCoordinates2D, Vector3Int plotCoordinates3D)
        {
            IsOwnedByPlayer = isOwnedByPlayer;
            IsOwnedByCity = isOwnedByCity;
            HasFence = hasFence;
            IsWater = isWater;
            OwnerID = ownerID;
            FenceDictionaryKey = fenceDictionaryKey;
            TerrainTileTypeKey = terrainTileTypeKey;
            TerrainTileVariant = terrainTileVariant;
            TerrainTileRotation = terrainTileRotation;
            BaseLandValue = baseLandValue;
            PlotCoordinates2D = plotCoordinates2D;
            PlotCoordinates3D = plotCoordinates3D;
        }
    

        /// <summary>
        /// Adds given fence data to this plot.
        /// </summary>
        public void AddFenceData(string fenceDictionaryKey)
        {
            HasFence = true;
            FenceDictionaryKey = fenceDictionaryKey;
        }
        /// <summary>
        /// Removes fence data from this plot.
        /// </summary>
        public void RemoveFence()
        {
            HasFence = false;
            FenceDictionaryKey = "None";
        }

        /// <summary>
        /// Adds given road data to this plot.
        /// </summary>
        public void AddRoadData(string roadDictionaryKey)
        {
            HasRoad = true;
            RoadDictionaryKey = roadDictionaryKey;
        }
        /// <summary>
        /// Removes road data from this plot.
        /// </summary>
        public void RemoveRoad()
        {
            HasRoad = false;
            IsCrosswalk = false;
            RoadDictionaryKey = "None";
        }

    }
}
