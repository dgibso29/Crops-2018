using System.Collections;
using UnityEngine;

namespace Crops.World
{

    /// <summary>
    /// Denotes type of buildings/etc that a specific city block will develop.
    /// </summary>
    public enum BlockZoneType { Residential, Commercial, Industrial, Agricultural, Office, Mixed, Park, Special}

    /// <summary>
    /// Provides specific parameters for zone development. Intended for use with Special zones.
    /// </summary>
    public enum ZoningParameters { None, Fountain}

    /// <summary>
    /// Contains and manages a block of City tiles within a larger city.
    /// </summary>
    public class CityBlock
    {
        /// <summary>
        /// Footprint of the block in (X,Y) dimensions (ie 3x5)
        /// </summary>        
        [Newtonsoft.Json.JsonIgnore]
        public Vector2Int BlockFootprint
        {
            get { return blockFootprintData.ToVector2Int(); }
            set { blockFootprintData = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Block footprint for serialization.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        Vector2IntJSON blockFootprintData;

        /// <summary>
        /// Bottom-left tile of the block.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Vector2 BlockOriginTile
        {
            get { return blockOriginTileData.ToVector2Int(); }
            set { blockOriginTileData = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Origin tile position for serialization.
        /// </summary>
        [Newtonsoft.Json.JsonProperty]
        Vector2IntJSON blockOriginTileData;

        /// <summary>
        /// Array of tilemap positions of tiles on the block, where index 0,0 is the SW-most and index 4,4 is the NE-most.
        /// </summary>        
        [Newtonsoft.Json.JsonIgnore]
        public Vector3Int[,] BlockTiles;

        public BlockZoneType ZoneType { get; }

        public ZoningParameters ZoneParameters { get; }

        [Newtonsoft.Json.JsonIgnore]
        Map WorldMap;

        /// <summary>
        /// Tiles that are free to be built upon.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        int availableTiles;

        public CityBlock()
        {
            BlockTiles = SetUpBlockTiles();
        }

        public CityBlock(Vector3Int[,] blockTiles, BlockZoneType zoneType, Map map, ZoningParameters parameters = ZoningParameters.None)
        {
            // When deserializing, blockTiles will be null. In that case, set up BlockTiles before calling it.
            if (blockTiles == null)
            {
                BlockTiles = SetUpBlockTiles();
                BlockOriginTile = new Vector2(BlockTiles[0, 0].x, BlockTiles[0, 0].y);
                BlockFootprint = new Vector2Int(BlockTiles.GetLength(0), BlockTiles.GetLength(1));
            }
            else
            {
                BlockOriginTile = new Vector2(blockTiles[0, 0].x, blockTiles[0, 0].y);
                BlockFootprint = new Vector2Int(blockTiles.GetLength(0), blockTiles.GetLength(1));
                BlockTiles = blockTiles;
            }

            ZoneType = zoneType;
            WorldMap = Object.FindObjectOfType<Map>();
            //Debug.Log("Setting parameters to " + parameters);
            ZoneParameters = parameters;

            // Set up available tiles.
            availableTiles = BlockFootprint.x * BlockFootprint.y;
        }

        /// <summary>
        /// Sets up the block.
        /// </summary>
        public void InitialiseBlock()
        {
            SetUpBoundingRoads();
            DevelopBlock();
        }
        
        /// <summary>
        /// Return a block tile array based on this block's size & origin tile.
        /// </summary>
        /// <returns></returns>
        public Vector3Int[,] SetUpBlockTiles()
        {
            var tiles = new Vector3Int[blockFootprintData.x, blockFootprintData.y];
            for (int x = 0; x < blockFootprintData.x; x++)
            {
                for (int y = 0; y < blockFootprintData.y; y++)
                {
                    tiles[x, y] = new Vector3Int(blockOriginTileData.x + x, blockOriginTileData.y + y, 0);
                }
            }
            return tiles;
        }

        void SetUpBoundingRoads()
        {
            Vector3Int tilePos = new Vector3Int();
            bool centerTile = false;
            #region Corners
            // Set up corners
            // SW
            tilePos = BlockTiles[0, 0];
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
            {
                availableTiles--;
            }
            else
            {
                if (CheckIfRoadShouldBeBuilt(tilePos))
                {
                    WorldMap.PlaceRoad(tilePos, "Road_City");
                    availableTiles--;
                }
            }
            // NW
            tilePos = BlockTiles[0, BlockFootprint.y - 1];
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
            {
                availableTiles--;
            }
            else
            {
                if (CheckIfRoadShouldBeBuilt(tilePos))
                {
                    WorldMap.PlaceRoad(tilePos, "Road_City");
                    availableTiles--;
                }
            }
            // NE
            tilePos = BlockTiles[BlockFootprint.x - 1, BlockFootprint.y - 1];
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
            {
                availableTiles--;
            }
            else
            {
                if (CheckIfRoadShouldBeBuilt(tilePos))
                {
                    WorldMap.PlaceRoad(tilePos, "Road_City");
                    availableTiles--;
                }
            }
            // SE
            tilePos = BlockTiles[BlockFootprint.x - 1, 0];
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
            {
                availableTiles--;
            }
            else
            {
                if (CheckIfRoadShouldBeBuilt(tilePos))
                {
                    WorldMap.PlaceRoad(tilePos, "Road_City");
                    availableTiles--;
                }
            }
            #endregion
            // Set up S roads
            for (int x = 1; x < BlockFootprint.x - 1; x++)
            {
                tilePos = BlockTiles[x, 0];

                if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
                {
                    availableTiles--;
                    continue;
                }
                if (x > 1 && x < BlockFootprint.x - 1)
                {
                    centerTile = true;
                }
                if (!CheckIfRoadShouldBeBuilt(tilePos, centerTile))
                {
                    continue;
                }
                WorldMap.PlaceRoad(tilePos, "Road_City");
                availableTiles--;
            }
            // Set up N roads
            for (int x = 1; x < BlockFootprint.x - 1; x++)
            {
                tilePos = BlockTiles[x, BlockFootprint.y - 1];
                if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
                {
                    availableTiles--;
                    continue;
                }
                if (x > 1 && x < BlockFootprint.x - 1)
                {
                    centerTile = true;
                }
                if (!CheckIfRoadShouldBeBuilt(tilePos, centerTile))
                {
                    continue;
                }
                WorldMap.PlaceRoad(tilePos, "Road_City");
                availableTiles--;
            }
            // Set up W roads
            for (int y = 1; y < BlockFootprint.y - 1; y++)
            {
                tilePos = BlockTiles[0, y];
                if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
                {
                    availableTiles--;
                    continue;
                }
                if (y > 1 && y < BlockFootprint.y - 1)
                {
                    centerTile = true;
                }
                if (!CheckIfRoadShouldBeBuilt(tilePos, centerTile))
                {
                    continue;
                }
                WorldMap.PlaceRoad(tilePos, "Road_City");
                availableTiles--;
            }
            // Set up E roads
            for (int y = 1; y < BlockFootprint.y - 1; y++)
            {
                tilePos = BlockTiles[BlockFootprint.x - 1, y];
                if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
                {
                    availableTiles--;
                    continue;
                }
                if (y > 1 && y < BlockFootprint.y - 1)
                {
                    centerTile = true;
                }
                if (!CheckIfRoadShouldBeBuilt(tilePos, centerTile))
                {
                    continue;
                }
                WorldMap.PlaceRoad(tilePos, "Road_City");
                availableTiles--;
            }


            // Set up crosswalks.
            switch (BlockFootprint.x)
            {
                case 5:
                    {
                        SetCrosswalk(BlockTiles[2, 0]);
                        SetCrosswalk(BlockTiles[2, BlockFootprint.y - 1]);
                        break;
                    }
                case 6:
                    {
                        SetCrosswalk(BlockTiles[1, 0]);
                        SetCrosswalk(BlockTiles[4, 0]);
                        SetCrosswalk(BlockTiles[1, BlockFootprint.y - 1]);
                        SetCrosswalk(BlockTiles[4, BlockFootprint.y - 1]);
                        break;
                    }
                case 7:
                    {
                        SetCrosswalk(BlockTiles[1, 0]);
                        SetCrosswalk(BlockTiles[5, 0]);
                        SetCrosswalk(BlockTiles[1, BlockFootprint.y - 1]);
                        SetCrosswalk(BlockTiles[5, BlockFootprint.y - 1]);
                        break;
                    }
            }
            switch (BlockFootprint.y)
            {
                case 5:
                    {
                        SetCrosswalk(BlockTiles[0 , 2]);
                        SetCrosswalk(BlockTiles[BlockFootprint.x - 1, 2]);
                        break;
                    }
                case 6:
                    {
                        SetCrosswalk(BlockTiles[0, 1]);
                        SetCrosswalk(BlockTiles[0, 4]);
                        SetCrosswalk(BlockTiles[BlockFootprint.x - 1, 1]);
                        SetCrosswalk(BlockTiles[BlockFootprint.x - 1, 4]);
                        break;
                    }
                case 7:
                    {
                        SetCrosswalk(BlockTiles[0, 1]);
                        SetCrosswalk(BlockTiles[0, 5]);
                        SetCrosswalk(BlockTiles[BlockFootprint.x - 1, 1]);
                        SetCrosswalk(BlockTiles[BlockFootprint.x - 1, 5]);
                        break;
                    }
            }

        }

        void SetCrosswalk(Vector3Int tilePos)
        {
            // Only continue if tile not null.
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tilePos))
            {
                // Make sure adjacent tiles are not already crosswalks
                if(CheckIfAdjacentRoadInDirection(tilePos, "N"))
                {
                    if (WorldMap.objectTilemap.GetTile<RoadTile>(new Vector3Int(tilePos.x, tilePos.y + 1, 0)).isCrosswalk == true)
                    return;
                }
                if (CheckIfAdjacentRoadInDirection(tilePos, "S"))
                {
                    if (WorldMap.objectTilemap.GetTile<RoadTile>(new Vector3Int(tilePos.x, tilePos.y - 1, 0)).isCrosswalk == true)
                        return;
                }
                if (CheckIfAdjacentRoadInDirection(tilePos, "E"))
                {
                    if (WorldMap.objectTilemap.GetTile<RoadTile>(new Vector3Int(tilePos.x + 1, tilePos.y, 0)).isCrosswalk == true)
                        return;
                }
                if (CheckIfAdjacentRoadInDirection(tilePos, "W"))
                {
                    if (WorldMap.objectTilemap.GetTile<RoadTile>(new Vector3Int(tilePos.x - 1, tilePos.y, 0)).isCrosswalk == true)
                        return;
                }
                WorldMap.objectTilemap.GetTile<RoadTile>(tilePos).isCrosswalk = true;
                WorldMap.MapData[tilePos.x, tilePos.y].IsCrosswalk = true;
                WorldMap.objectTilemap.RefreshTile(tilePos);
            }

        }

        /// <summary>
        /// Returns true if a road should be built at the given location.
        /// </summary>
        /// <returns></returns>
        bool CheckIfRoadShouldBeBuilt(Vector3Int tilePos, bool centerTile = false)
        {
            int mask = HasRoadTile(tilePos + new Vector3Int(0, 1, 0)) ? 1 : 0;   // N  = 1
            mask += HasRoadTile(tilePos + new Vector3Int(1, 1, 0)) ? 2 : 0;      // NE = 2
            mask += HasRoadTile(tilePos + new Vector3Int(1, 0, 0)) ? 4 : 0;      // E  = 4
            mask += HasRoadTile(tilePos + new Vector3Int(1, -1, 0)) ? 8 : 0;     // SE = 8
            mask += HasRoadTile(tilePos + new Vector3Int(0, -1, 0)) ? 16 : 0;    // S  = 16
            mask += HasRoadTile(tilePos + new Vector3Int(-1, -1, 0)) ? 32 : 0;   // SW = 32
            mask += HasRoadTile(tilePos + new Vector3Int(-1, 0, 0)) ? 64 : 0;    // W  = 64
            mask += HasRoadTile(tilePos + new Vector3Int(-1, 1, 0)) ? 128 : 0;   // NW = 128

            // Check for illegal intersections
            switch (mask)
            {
                case 248:
                case 62:
                case 143:
                case 227:
                case 249:
                case 252:
                case 244:
                case 231:
                case 243:
                case 159:
                case 207:
                case 126:
                case 63:
                case 254:
                case 191:
                case 151:
                case 240:
                case 60:
                case 15:
                case 135:
                case 239:
                case 127:
                case 253:
                case 247:
                case 223:
                case 199:
                case 31:
                case 124:
                // Roundabouts
                //case 7:
                //case 28:
                //case 112:
                //case 193:
                //
                case 36:
                case 225:
                case 120:
                case 195:

                case 241: return false;                
            }
            // Return false if the tile is not on a corner
            if(centerTile == true)
            {              
                switch (mask)
                {
                    case 14:
                    case 56:
                    case 224:
                    case 131: return false;
                }
            }
            return true;
        }

        // This determines if the Tile at the position is the same RoadTile.
        bool HasRoadTile(Vector3Int position)
        {
            return WorldMap.objectTilemap.GetTile<RoadTile>(position);
        }

        /// <summary>
        /// Returns true if road found at adjacent tile in given direction.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        bool CheckIfAdjacentRoadInDirection(Vector3Int tilePos, string direction)
        {
            Vector3Int posToCheck;
            switch (direction)
            {
                case "N":
                    {
                        posToCheck = new Vector3Int(tilePos.x, tilePos.y + 1, 0);
                        if (WorldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "E":
                    {
                        posToCheck = new Vector3Int(tilePos.x + 1, tilePos.y, 0);
                        if (WorldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "S":
                    {
                        posToCheck = new Vector3Int(tilePos.x, tilePos.y - 1, 0);
                        if (WorldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "W":
                    {
                        posToCheck = new Vector3Int(tilePos.x - 1, tilePos.y, 0);
                        if (WorldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
            }
            return false;
        }      

        /// <summary>
        /// Develops block according to ZoningParameters and CityBlockZoneType.
        /// </summary>
        void DevelopBlock()
        {
            int sanityCheck = 0;
            // Debug.Log($"Attemping to build on a block of type {ZoneType.ToString()} and size {BlockSize} with {availableTiles} available tiles.");
            // Continue developing until there are no remaining available tiles.
            while (availableTiles > 0 && sanityCheck < 250)
            {
                Vector3Int[,] buildingPosition;
                BlockZoneType zoneToBuild = ZoneType;
                // Move forward based on zone type. 
                switch (zoneToBuild)
                {
                    // If type is Special
                    case BlockZoneType.Special:
                        {
                            // ZoningParameters come in to play here
                            switch (ZoneParameters)
                            {
                                case ZoningParameters.Fountain:
                                    {
                                        buildingPosition = GetRandomBuildingPositionFromSize(3, 3);
                                        string key = ("Special_Fountain");
                                        Debug.Log(key);
                                        BuildBuildingOnBlock(new CityBuildingData(new Vector2Int(3, 3), BlockTiles[buildingPosition[0, 0].x, buildingPosition[0, 0].y], 0, key, WorldMap.gameManager.AssignUniqueID(), -1, BuildableObjectType.CityBuilding, zoneToBuild, ZoneParameters));
                                        break;
                                    }
                            }
                            return;
                        }
                    case BlockZoneType.Mixed:
                        {
                            // Choose random zone type for building based on these weights.
                            MathHelper.IntRange[] zoneTypeWeights = new MathHelper.IntRange[7] { new MathHelper.IntRange(0, 0, 30), new MathHelper.IntRange(1, 1, 20), new MathHelper.IntRange(2, 2, 10), new MathHelper.IntRange(3, 3, 10), new MathHelper.IntRange(4, 4, 5), new MathHelper.IntRange(5, 5, 15), new MathHelper.IntRange(6, 6, 10) };
                            zoneToBuild = (BlockZoneType)MathHelper.RandomRange.WeightedRange(zoneTypeWeights);
                            break;
                        }
                        // Otherwise, continue to normal types.
                }
                // Get building size and position
                buildingPosition = GetRandomBuildingPosition();
                // Validate given position
                if(buildingPosition.GetLength(0) == 0 || buildingPosition.GetLength(1) == 0)
                {
                    sanityCheck++;
                    continue;
                }
                Vector2Int footprint = new Vector2Int(buildingPosition.GetLength(0), buildingPosition.GetLength(1));

                string buildingDictionaryKey = zoneToBuild.ToString();

                string tileSpriteKey = WorldMap.FindRandomBuildingOfTypeAndSize(buildingDictionaryKey, footprint);
                // If the key is for a different footprint, adjust buildingPosition & the footprint to reflect the new size.
                if(tileSpriteKey != $"{footprint.x}x{footprint.y}")
                {
                    // Split the tileSpriteKey in half to get the new size
                    string[] splitKey = tileSpriteKey.Split(new char[] { 'x' });
                    footprint = new Vector2Int(int.Parse(splitKey[0]), int.Parse(splitKey[1]));
                    // Replace the current buildingPosition array with the new smaller version
                    Vector3Int[,] resizedTiles = new Vector3Int[footprint.x, footprint.y];
                    for(int x = 0; x < footprint.x; x++)
                    {
                        for(int y = 0; y < footprint.y; y++)
                        {
                            resizedTiles[x, y] = buildingPosition[x, y];
                        }
                    }
                    buildingPosition = resizedTiles;
                }

                // Choose a version of the given building to place.
                BuildableObject blah = ((BuildableObject)Tileset.TilesetDictionary[buildingDictionaryKey]);
                int buildingVariant = Random.Range(0, blah.tileSprites[tileSpriteKey].Length);

                CityBuildingData newBuildingData = new CityBuildingData(footprint, BlockTiles[buildingPosition[0, 0].x, buildingPosition[0, 0].y], buildingVariant, buildingDictionaryKey, WorldMap.gameManager.AssignUniqueID(), -1, BuildableObjectType.CityBuilding, zoneToBuild, ZoneParameters);
                
                BuildBuildingOnBlock(newBuildingData);
                //Debug.Log("Avail Tiles = " + availableTiles);
                sanityCheck++;
            }
            
        }

        void BuildBuildingOnBlock(CityBuildingData building)
        {
            // Place the given version of the building.
            WorldMap.PlaceBuildableObject<CityBuilding, CityBuildingData, BuildableObjectTile>(building);
            //Debug.Log($"Avail tiles before {availableTiles}");
            availableTiles -= (building.footprint.x * building.footprint.y);
            //Debug.Log($"Avail tiles after {availableTiles}");


        }


        /// <summary>
        /// Returns random building position tiles based on available tiles in the block. 
        /// If returned array is of size 0,0, a valid position could not be found.
        /// </summary>
        /// <returns></returns>
        Vector3Int[,] GetRandomBuildingPosition()
        {
            // Size of position
            int sizeX;
            int sizeY;
            
            Vector3Int[,] tilesToCheck = new Vector3Int[0,0];

            bool checking = true;
            int sanityCheck = 0;
            while (checking && sanityCheck < 200)
            {
                // Randomly get size of building position to check.
                sizeX = Random.Range(1, BlockFootprint.x - 1);
                sizeY = Random.Range(1, BlockFootprint.y - 1);

                // Get bottom left starting tile of the position to check based on sizeX and sizeY.
                Vector3Int startingPoint = new Vector3Int(Random.Range(0, BlockFootprint.x - sizeX + 1), Random.Range(0, BlockFootprint.y - sizeY + 1), 0);

                /// Add one to these ^^^^^^^^^^^^^^ once weird city/road checking issue is resolved.

                // Make sure starting point can be built upon before continuing.
                if (!CheckIfTileCanBeBuiltUpon(BlockTiles[startingPoint.x, startingPoint.y]))
                {
                    // Skip if not
                    sanityCheck++;
                    continue;
                }

                // Set up tiles to check based on starting point.
                tilesToCheck = new Vector3Int[sizeX, sizeY];
                for(int x = 0; x < sizeX; x++)
                {
                    for(int y = 0; y < sizeY; y++)
                    {
                        tilesToCheck[x, y] = new Vector3Int(startingPoint.x + x, startingPoint.y + y, 0);
                    }
                }

                // Check if these tiles are valid...
                if (CheckIfValidPositionToBuild(tilesToCheck))
                {
                    // And break out of the loop if so.
                    break;
                }
                // Otherwise, we start over.
                tilesToCheck = new Vector3Int[0, 0];
                sanityCheck++;
            }
            return tilesToCheck;
        }

        /// <summary>
        /// Returns a random building position based on the given building size.
        /// If returned array is of size 0,0, a valid position could not be found.
        /// </summary>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <returns></returns>
        Vector3Int[,] GetRandomBuildingPositionFromSize(int sizeX, int sizeY)
        {
            Vector3Int[,] tilesToCheck = new Vector3Int[0, 0];
            bool checking = true;
            int sanityCheck = 0;
            while (checking && sanityCheck < 200)
            {
                // Get bottom left starting tile of the position to check based on sizeX and sizeY.
                Vector3Int startingPoint = new Vector3Int(Random.Range(0, BlockFootprint.x - sizeX), Random.Range(0, BlockFootprint.y - sizeY), 0);
                //Debug.Log("Checking starting point " + startingPoint);
                // Make sure starting point can be built upon before continuing.
                if (!CheckIfTileCanBeBuiltUpon(BlockTiles[startingPoint.x, startingPoint.y]))
                {
                    // Skip if not
                    sanityCheck++;
                    continue;
                }
                tilesToCheck = new Vector3Int[sizeX, sizeY];
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                       // Debug.Log($"Adding tile {x},{y} to tilesToCheck");

                        tilesToCheck[x, y] = new Vector3Int(startingPoint.x + x, startingPoint.y + y, 0);
                    }
                }
                // Check if these tiles are valid...
                if (CheckIfValidPositionToBuild(tilesToCheck))
                {
                    // And break out of the loop if so.
                    break;
                }
                // Otherwise, we start over.
                tilesToCheck = new Vector3Int[0, 0];
                sanityCheck++;
            }
            //Debug.Log($"Returning tiles with starting pos of {tilesToCheck[0, 0]}");
            return tilesToCheck;
        }

        /// <summary>
        /// Returns true if the given tiles can be built upon.
        /// </summary>
        /// <param name="tilesToCheck"></param>
        /// <returns></returns>
        bool CheckIfValidPositionToBuild(IEnumerable tilesToCheck)
        {
            foreach(Vector3Int tilePos in tilesToCheck)
            {
                // Check if each tile can be built upon.
                if (!CheckIfTileCanBeBuiltUpon(BlockTiles[tilePos.x, tilePos.y]))
                {
                    //Debug.Log("Unable to build at " + tilePos);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the given tiles can be built upon.
        /// </summary>
        /// <param name="tilesToCheck"></param>
        /// <returns></returns>
        bool CheckIfValidPositionToBuild(Vector3Int[,] tilesToCheck)
        {
            //int tilesChecked = 0;
            //Debug.Log($"Checking footprint with dimensions {tilesToCheck.GetLength(0)},{tilesToCheck.GetLength(1)}, starting at {BlockTiles[tilesToCheck[0,0].x, tilesToCheck[0,0].y]}");
            foreach (Vector3Int tilePos in tilesToCheck)
            {
                //Debug.Log($"Checking tile {BlockTiles[tilePos.x, tilePos.y]}");
                // Check if each tile can be built upon.
                //tilesChecked++;
                if (!CheckIfTileCanBeBuiltUpon(BlockTiles[tilePos.x, tilePos.y]))
                {
                    //Debug.Log("Unable to build at " + BlockTiles[tilePos.x, tilePos.y]);
                    //Debug.Log($"Failed checks. Checked {tilesChecked} tiles");
                    return false;
                }
            }
            //Debug.Log($"Passed checks. Checked {tilesChecked} tiles, starting at {BlockTiles[tilesToCheck[0, 0].x, tilesToCheck[0, 0].y]}");
            return true;
        }

        /// <summary>
        /// Returns true if the given tile can be built upon.
        /// </summary>
        /// <param name="tileToCheck"></param>
        /// <returns></returns>
        bool CheckIfTileCanBeBuiltUpon(Vector3Int tileToCheck)
        {
            if (WorldMap.objectTilemap.GetTile<RoadTile>(tileToCheck))
            {
                // Debug.Log("Blocked by road.");
                return false;
            }
            if (WorldMap.objectTilemap.GetTile<BuildableObjectTile>(tileToCheck))
            {
                if(WorldMap.MapData[tileToCheck.x, tileToCheck.y].ObjectOnTileID != 0)
                {
                    return false;
                }
            }

            return true;
        }


    }
}
