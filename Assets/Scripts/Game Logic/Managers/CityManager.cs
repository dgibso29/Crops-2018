using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Crops.World
{
    /// <summary>
    /// Controls the in-game city.
    /// </summary>
    public class CityManager : MonoBehaviour, ILoadable
    {

        Map worldMap;

        public CityData Data
        {
            get { return gameManager.gameData.cityData; }
            set { gameManager.gameData.cityData = value; }
        }

        public GameManager gameManager;

        /// <summary>
        /// Center road tile of the city.
        /// </summary>
        Vector3Int CityCenter
        {
            get { return Data.cityCenter; }
            set { Data.cityCenter = value; }
        }

        /// <summary>
        /// List of all blocks in the city.
        /// </summary>
        public List<CityBlock> CityBlocks
        {
            get { return Data.cityBlocks; }
            set { Data.cityBlocks = value; }
        }

        /// <summary>
        /// A 'map' of the city limits, where 49,49 is the center tile, using booleans. 
        /// Where true, the city has built something, or the land is otherwise occupied.
        /// </summary>
        public bool[,] CityMap
        {
            get { return Data.cityMap; }
            set { Data.cityMap = value; }
        }

        int CityMapSize
        {
            get { return Data.cityMapSize; }
            set { Data.cityMapSize = value; }
        }

        /// <summary>
        /// Time in months between city growth.
        /// </summary>
        public int CityGrowthInterval
        {
            get { return Data.cityGrowthInterval; }
            set { Data.cityGrowthInterval = value; }
        }

        private int MonthsSinceLastExpansion
        {
            get { return Data.monthsSinceLastExpansion; }
            set { Data.monthsSinceLastExpansion = value; }
        }

        Vector3Int CityMapCenter
        {
            get { return Data.cityMapCenter; }
            set { Data.cityMapCenter = value; }
        }

        /// <summary>
        /// Tracks current expansion band, where band 1 includes tiles a radius of initialTileFootprint/2 + bandSize from the cityCenter.
        /// </summary>
        int CurrentExpansionBand
        {
            get { return Data.currentExpansionBand; }   
            set { Data.currentExpansionBand = value; }
        }

        int InitialConstruction
        {
            get { return Data.initialConstruction; }
            set { Data.initialConstruction = value; }
        }

        int BridgesBuilt
        {
            get { return Data.bridgesBuilt; }
            set { Data.bridgesBuilt = value; }
        }

        
        /// <summary>
        /// Farthest extents of the city from the center of 49,49 in the 4 cardinal directions.
        /// N/E/S/W = X/Y/Z/W.
        /// </summary>
        public Vector4 cityExtents;

        /// <summary>
        /// Required clear space to create minimum city -- downtown area.
        /// </summary>
        int initialCityFootprint = 9;

        /// <summary>
        /// Holds RCI values for the city. Used to determine zoning of new growth.
        /// </summary>
        RCI CityRCI
        {
            get { return Data.cityRCI; }
            set { Data.cityRCI = value; }
        }

        /// <summary>
        /// Holds RCI values for the city (blocks built of each type).
        /// </summary>
        [System.Serializable]
        public class RCI
        {
            public int agriculturalBlocks;
            public int commercialBlocks;
            public int industrialBlocks;
            public int mixedBlocks;
            public int officeBlocks;
            public int parkBlocks;
            public int residentialBlocks;
            public int specialBlocks;
        }

        private void Start()
        {
            worldMap = gameManager.worldMap;
            Data = gameManager.gameData.cityData;
        }

        /// <summary>
        /// Initialise the city for a new game.
        /// </summary>
        public void InitialiseNewCity()
        {
            worldMap = gameManager.worldMap;
            //data = new CityData
            //{
            //    cityBlocks = new List<CityBlock>()
            //};
            InitializeCityMap();
            CityRCI = new RCI();
            GenerateCityCenter();
            GenerateInitialCityBlocks();
            UpdateCityExtents();
            //ExpandCity(Random.Range(7, 20));
            ExpandCity(20);
        }

        /// <summary>
        /// Initialise the city from a saved game.
        /// </summary>
        public void InitializeFromSave()
        {
            worldMap = gameManager.worldMap;
            //ExpandCity(50);
        }

        IEnumerator DevelopCityOverTime(int blocksToAdd)
        {
            int i = 0;
            while(i < blocksToAdd)
            {
                ExpandCity(1);
                i++;
                yield return new WaitForSeconds(Random.Range(5, 20));
            }
        }

        /// <summary>
        /// Check if it is time to expand the city.
        /// </summary>
        public void CheckForCityExpansion()
        {
            if(MonthsSinceLastExpansion >= CityGrowthInterval)
            {
                StartCoroutine(DevelopCityOverTime(Random.Range(2, 6)));
                MonthsSinceLastExpansion = 0;
            }
            else
            {
                MonthsSinceLastExpansion++;
            }
        }

        /// <summary>
        /// Find and set a valid CityCenter position.
        /// </summary>
        void GenerateCityCenter()
        {
            Vector3Int testPosition;
            bool searching = true;
            bool valid = true;
            int sanityChecker = 0;

            int cityCenterEdgeOffset = 10;
            // Set default position, just in case.
            CityCenter = new Vector3Int(worldMap.MapSize / 2 - 1, worldMap.MapSize / 2 - 1, 0);

            /// Get CityCenter
            /// Validate initial footprint around citycenter
            /// adjust cityMapCenter based on  cityMapSize and distance of citycenter from edge of map.
            /// 
            while (searching)
            {
                valid = true;
                if (sanityChecker > 500)
                {
                    cityCenterEdgeOffset = 0;
                }
                if (sanityChecker > 2000)
                {

                    break;
                }
                // Try positions until a valid one is found.
                // Get a random position as the bottom-left tile of the city.
                testPosition = new Vector3Int
                {
                    x = Random.Range(1 + cityCenterEdgeOffset + initialCityFootprint, worldMap.MapSize - initialCityFootprint - 1 - cityCenterEdgeOffset),
                    y = Random.Range(1 + cityCenterEdgeOffset + initialCityFootprint, worldMap.MapSize - initialCityFootprint - 1 - cityCenterEdgeOffset)
                };
                //Debug.Log($"Testing {testPosition}...");
                //Debug.Log($"bottom left {testPosition.x - (initialCityFootprint / 2)},{testPosition.y - (initialCityFootprint / 2)}. top right {testPosition.x + (initialCityFootprint / 2)},{testPosition.y + (initialCityFootprint / 2)}");
                // Validate initial city footprint area, with testPosition as its center.

                for (int x = testPosition.x - (initialCityFootprint / 2); x < testPosition.x + (initialCityFootprint / 2) + 1; x++)
                {
                    for (int y = testPosition.y - (initialCityFootprint / 2); y < testPosition.y + (initialCityFootprint / 2) + 1; y++)
                    {
                        if (/*worldMap.CheckIfTileHasWater(x, y)*/            worldMap.MapData[x, y].IsWater
)
                        {
                            //Debug.Log($"Apparently tile at {x},{y} has water!");
                            valid = false;
                            break;
                        }
                    }
                    if (!valid)
                    {
                        break;
                    }
                }
                if (valid)
                {
                    CityCenter = new Vector3Int(testPosition.x, testPosition.y, 0);
                    Debug.Log($"Found valid city center at: {CityCenter}");
                    break;
                }
                sanityChecker++;
            }
            // Set up cityMapCenter, adjusting as needed to map edges.
            CityMapCenter = new Vector3Int((CityMapSize / 2) - 1, (CityMapSize / 2) - 1, 0);
             Debug.Log($"cityMapCenter before adjustment {CityMapCenter}.");
            // Check X
            int centerX = CityMapCenter.x;
            // If it's less than 1
            if(CityCenter.x - CityMapCenter.x < 1)
            {
                centerX -= Mathf.Abs(CityCenter.x - CityMapCenter.x) + 2;
            }
            // If it's greater than mapSize - 2 (so if mapSize = 150, if it's greater than 148
            else if(CityCenter.x + CityMapCenter.x > worldMap.MapSize - 2)
            {
                centerX = (CityMapSize - 1) - (Mathf.Abs(worldMap.MapSize - CityCenter.x) + 2);
            }
            // Check Y
            int centerY = CityMapCenter.y;
            // If it's less than 1
            if (CityCenter.y - CityMapCenter.y < 1)
            {
                centerY -= Mathf.Abs(CityCenter.y - CityMapCenter.y) + 2;
            }
            // If it's greater than mapSize - 2 (so if mapSize = 150, if it's greater than 148
            else if (CityCenter.y + CityMapCenter.y > worldMap.MapSize - 2)
            {
                centerY = (CityMapSize - 1) - (Mathf.Abs(worldMap.MapSize - CityCenter.y) + 2);
            }
            Debug.Log($"cityMapSize = {CityMapSize}, cityMapCenter = {CityMapCenter}, cityCenter = {CityCenter}.");
            // Place the center road.
            worldMap.PlaceRoad(CityCenter, "Road_City");
        }

        void GenerateInitialCityBlocks()
        {
            //Debug.Log("Creating blocks");
            // Create initial 5x5 Blocks around city center
            Vector2Int blockSize = new Vector2Int(5, 5);
            BuildCityBlock(GetTilemapPositionRelativeToCityCenter(0, 0), blockSize, BlockZoneType.Special, ZoningParameters.Fountain);
            BuildCityBlock(GetTilemapPositionRelativeToCityCenter(-4, 0), blockSize, BlockZoneType.Residential);
            BuildCityBlock(GetTilemapPositionRelativeToCityCenter(0, -4), blockSize, BlockZoneType.Commercial);
            BuildCityBlock(GetTilemapPositionRelativeToCityCenter(-4, -4), blockSize, BlockZoneType.Residential);
        }

        void ExpandCity(int blocksToAdd)
        {

            BlockZoneType zoning = BlockZoneType.Special;
            ZoningParameters parameters = ZoningParameters.None;


            for(int i = 0; i < blocksToAdd; i++)
            {
                // Determine zoning info
                // Small weighted change of a random type being chosen
                MathHelper.IntRange[] weightedChance = new MathHelper.IntRange[] { new MathHelper.IntRange(0, 0, 90), new MathHelper.IntRange(1, 1, 10) };
                int random = MathHelper.RandomRange.WeightedRange(weightedChance);
                if (random == 1)
                {
                    // Set random type
                    zoning = (BlockZoneType)Random.Range(0, 7);
                }
                else
                {
                    // Otherwise, get type from RCI demand
                    zoning = GetBlockZoneToBuild();
                }                
                BuildRandomCityBlock(zoning, parameters);
                // Check for bridges to build.

            }

            //Debug.Log($"Final RCI: A:{CityRCI.agriculturalBlocks}, C:{CityRCI.commercialBlocks}," +
            //    $" I:{CityRCI.industrialBlocks}, M:{CityRCI.mixedBlocks}, O:{CityRCI.officeBlocks}, P:{CityRCI.parkBlocks}," +
            //    $" R:{CityRCI.residentialBlocks}.");

        }

        /// <summary>
        /// Returns next zone to build based on RCI.
        /// </summary>
        /// <returns></returns>
        BlockZoneType GetBlockZoneToBuild()
        {
            BlockZoneType type = BlockZoneType.Residential;

            // Weight for each type
            int agriculturalWeight = 0, commercialWeight = 0, industrialWeight = 0, officeWeight = 0, parkWeight = 0;

            // Get weight for each type
            // Agricultural
            int agriculturalRatio = 5;
            agriculturalWeight = (CityRCI.residentialBlocks / agriculturalRatio - CityRCI.agriculturalBlocks) + CityRCI.residentialBlocks % agriculturalRatio;

            // Commercial
            int commercialRatio = 4;
            commercialWeight = (CityRCI.residentialBlocks / commercialRatio - CityRCI.commercialBlocks) + CityRCI.residentialBlocks % commercialRatio;
            // Industrial
            int industrialRatio = 8;
            industrialWeight = (CityRCI.residentialBlocks / industrialRatio - CityRCI.industrialBlocks) + CityRCI.residentialBlocks % industrialRatio;

            // Office
            int officeRatio = 2;
            officeWeight = (CityRCI.industrialBlocks / officeRatio - CityRCI.officeBlocks) + CityRCI.industrialBlocks % officeRatio;

            // Park
            int parkRatio = 6;
            parkWeight = (CityRCI.residentialBlocks / parkRatio - CityRCI.parkBlocks) + CityRCI.residentialBlocks % parkRatio;

            //Debug.Log($"Weights: A:{agriculturalWeight}, C:{commercialWeight}, I:{industrialWeight}, O:{officeWeight}, P:{parkWeight}.");
            //Debug.Log($"RCI: A:{cityRCI.agriculturalBlocks}, C:{cityRCI.commercialBlocks}," +
            //    $" I:{cityRCI.industrialBlocks}, M:{cityRCI.mixedBlocks}, O:{cityRCI.officeBlocks}, P:{cityRCI.parkBlocks}," +
            //    $" R:{cityRCI.residentialBlocks}.");

            int[] weights = new int[5] { agriculturalWeight, commercialWeight, industrialWeight, officeWeight, parkWeight };
            int maxWeight = 0;
            int maxIndex = -1;
            for(int i = 0; i < 5; i++)
            {
                if(weights[i] > maxWeight)
                {
                    maxWeight = weights[i];
                    maxIndex = i;
                }
                else if(maxWeight != 0 && weights[i] == maxWeight)
                {
                    // Chance of a random type being chosen
                    MathHelper.IntRange[] weightedChance = new MathHelper.IntRange[] { new MathHelper.IntRange(0, 0, 20), new MathHelper.IntRange(1, 1, 10), new MathHelper.IntRange(1, 2, 70) };
                    int random = MathHelper.RandomRange.WeightedRange(weightedChance);
                    if (random == 1)
                    {
                        // Set random type
                        return (BlockZoneType)Random.Range(0, 7);
                    }
                    else if(random == 2)
                    {
                        return BlockZoneType.Residential;
                    }
                    else
                    {
                        return BlockZoneType.Mixed;
                    }
                }
            }
            //Debug.Log($"Max Index {maxIndex} with weight of {maxWeight}");
            switch (maxIndex)
            {
                case -1: return BlockZoneType.Residential;
                case 0: return BlockZoneType.Agricultural;
                case 1: return BlockZoneType.Commercial;
                case 2: return BlockZoneType.Industrial;
                case 3: return BlockZoneType.Office;
                case 4: return BlockZoneType.Park;
            }
            return type;
        }

        void AddToRCI(BlockZoneType typeToAdd)
        {
            switch (typeToAdd)
            {
                case BlockZoneType.Agricultural:
                    {
                        CityRCI.agriculturalBlocks++;
                        break;
                    }
                case BlockZoneType.Commercial:
                    {
                        CityRCI.commercialBlocks++;
                        break;
                    }
                case BlockZoneType.Industrial:
                    {
                        CityRCI.industrialBlocks++;
                        break;
                    }
                case BlockZoneType.Mixed:
                    {
                        CityRCI.mixedBlocks++;
                        break;
                    }
                case BlockZoneType.Office:
                    {
                        CityRCI.officeBlocks++;
                        break;
                    }
                case BlockZoneType.Park:
                    {
                        CityRCI.parkBlocks++;
                        break;
                    }
                case BlockZoneType.Residential:
                    {
                        CityRCI.residentialBlocks++;
                        break;
                    }
                case BlockZoneType.Special:
                    {
                        CityRCI.specialBlocks++;
                        break;
                    }
            }
        }


        /// <summary>
        /// Creates a new block in the specified location, and performs necessary maintenace tasks.
        /// </summary>
        void BuildCityBlock(Vector2Int startTile, Vector2Int blockSize, BlockZoneType blockZoning, ZoningParameters parameters = ZoningParameters.None)
        {
            //Debug.Log($"Building city block of type {blockZoning.ToString()} aka zoneIndex {blockZoning}");
            CityBlock newBlock = new CityBlock(GetNewBlockTiles(startTile, blockSize), blockZoning, worldMap, parameters);
            newBlock.InitialiseBlock();
            CityBlocks.Add(newBlock);
            AddTilesToCityMap(newBlock.BlockTiles);
            AddToRCI(blockZoning);
            if (BridgesBuilt < CurrentExpansionBand / 1)
            {
                foreach (Vector3Int tileToCheck in newBlock.BlockTiles)
                {
                    if (worldMap.objectTilemap.GetTile<RoadTile>(tileToCheck))
                    {
                        Vector3Int endPos;
                        // N
                        if (CheckIfAdjacentWaterInDirection(tileToCheck, "N"))
                        {
                            endPos = GetBridgeEndTile(tileToCheck, "N");
                            if (endPos.x != -1 && endPos.y != -1)
                            {
                                if (BridgesBuilt < CurrentExpansionBand / 1)
                                    BuildBridge(tileToCheck, endPos, "N");
                            }
                        }
                        // S
                        if (CheckIfAdjacentWaterInDirection(tileToCheck, "S"))
                        {
                            endPos = GetBridgeEndTile(tileToCheck, "S");
                            if (endPos.x != -1 && endPos.y != -1)
                            {
                                if (BridgesBuilt < CurrentExpansionBand / 1)
                                    BuildBridge(tileToCheck, endPos, "S");
                            }
                        }
                        // E
                        if (CheckIfAdjacentWaterInDirection(tileToCheck, "E"))
                        {
                            endPos = GetBridgeEndTile(tileToCheck, "E");
                            if (endPos.x != -1 && endPos.y != -1)
                            {
                                if (BridgesBuilt < CurrentExpansionBand / 1)
                                    BuildBridge(tileToCheck, endPos, "E");
                            }
                        }
                        // W
                        if (CheckIfAdjacentWaterInDirection(tileToCheck, "W"))
                        {
                            endPos = GetBridgeEndTile(tileToCheck, "W");
                            if (endPos.x != -1 && endPos.y != -1)
                            {
                                if (BridgesBuilt < CurrentExpansionBand / 1)
                                    BuildBridge(tileToCheck, endPos, "W");
                            }
                        }
                    }
                }

            }
        }

        void BuildRandomCityBlock(BlockZoneType blockZoning, ZoningParameters parameters = ZoningParameters.None)
        {
            Vector2Int startTile = new Vector2Int();
            Vector2Int blockSize = new Vector2Int();
            bool attemptingToBuild = true;
            int sanityChecker = 0;
            while (attemptingToBuild)
            {
                if (sanityChecker > 25)
                {
                    //Debug.Log("Failed to find valid expansion block.");

                    CurrentExpansionBand++;
                    return;
                }
                // get block size
                blockSize = GetRandomBlockSize();
                // Get start position, which also validates start and block size.
                startTile = GetRandomBlockStartPosition(blockSize);

                // If startTile == (-1,-1), we were unable to find a valid start, so iterate again.
                if (startTile.x == -1 && startTile.y == -1)
                {
                    sanityChecker++;
                    continue;
                }
                else
                {
                    attemptingToBuild = false;
                    break;
                }
            }
            // Create the block!
            BuildCityBlock(startTile, blockSize, blockZoning, parameters);

        }

        /// <summary>
        /// Returns end tile of a bridge beginning at the given start position.
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        Vector3Int GetBridgeEndTile(Vector3Int startPosition, string direction)
        {
            //Debug.Log($"Checking for bridge end tile starting at {startPosition} and going {direction}");
            List<Vector3Int> bridgeTiles = new List<Vector3Int>();
            Vector3Int endTile = new Vector3Int(-1, -1, 0);
            bridgeTiles.Add(startPosition);
            Vector3Int nextTile = new Vector3Int();
            int i = 0;
            bool searching = true;
            while (searching)
            {
                i++;
                switch (direction)
                {
                    case "N":
                        {
                            nextTile = new Vector3Int(startPosition.x, startPosition.y + i, 0);
                            //Debug.Log($"Checking tile {nextTile}");
                            // No roads!
                            if (worldMap.objectTilemap.GetTile<RoadTile>(nextTile))
                            {
                                return endTile;
                            }
                            if (worldMap.terrainTilemap.GetTile<WaterTile>(nextTile))
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else if(i == 1)
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else
                            {
                                //Debug.Log("Returning " + nextTile);

                                return endTile = nextTile;
                            }
                            break;
                        }
                    case "S":
                        {
                            nextTile = new Vector3Int(startPosition.x, startPosition.y - i, 0);
                            //Debug.Log($"Checking tile {nextTile}");
                            // No roads!
                            if (worldMap.objectTilemap.GetTile<RoadTile>(nextTile))
                            {
                                return endTile;
                            }
                            if (worldMap.terrainTilemap.GetTile<WaterTile>(nextTile))
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else if (i == 1)
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else
                            {
                                //Debug.Log("Returning " + nextTile);

                                return endTile = nextTile;
                            }
                            break;
                        }
                    case "E":
                        {
                            nextTile = new Vector3Int(startPosition.x + i, startPosition.y, 0);
                            //Debug.Log($"Checking tile {nextTile}");                    
                            // No roads!
                            if (worldMap.objectTilemap.GetTile<RoadTile>(nextTile))
                            {
                                return endTile;
                            }
                            if (worldMap.terrainTilemap.GetTile<WaterTile>(nextTile))
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else if (i == 1)
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else
                            {
                                //Debug.Log("Returning " + nextTile);
                                return endTile = nextTile;
                            }
                            break;
                        }
                    case "W":
                        {
                            nextTile = new Vector3Int(startPosition.x - i, startPosition.y, 0);
                            //Debug.Log($"Checking tile {nextTile}");
                            // No roads!
                            if (worldMap.objectTilemap.GetTile<RoadTile>(nextTile))
                            {
                                return endTile;
                            }
                            if (worldMap.terrainTilemap.GetTile<WaterTile>(nextTile))
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else if (i == 1)
                            {
                                bridgeTiles.Add(nextTile);
                            }
                            else
                            {
                                //Debug.Log("Returning " + nextTile);

                                return endTile = nextTile;
                            }
                            break;
                        }
                }
            }
            return endTile;
        }

        void UpdateCityExtents()
        {
            List<int> cityTilesX = new List<int>();
            List<int> cityTilesY = new List<int>();
            //Debug.Log("Updating city extents...");
            int minX = CityMapCenter.x, maxX = CityMapCenter.x;
            int minY = CityMapCenter.y, maxY = CityMapCenter.y;

            for(int x = 0; x < CityMap.GetLength(0); x++)
            {
                for(int y = 0; y < CityMap.GetLength(1); y++)
                {
                    if (CityMap[x, y] == true)
                    {
                        cityTilesX.Add(x);
                        cityTilesY.Add(y);
                    }
                }
            }
            foreach(int x in cityTilesX)
            {
                if(x > maxX)
                {
                    maxX = x;
                }
                if(x < minX)
                {
                    minX = x;
                }
            }
            foreach (int y in cityTilesY)
            {
                if (y > maxY)
                {
                    maxY = y;
                }
                if (y < minY)
                {
                    minY = y;
                }
            }

            cityExtents = new Vector4(maxY, maxX, minY, minX);
        }

        /// <summary>
        /// Returns a weighted range, weighing 0 at 50%, 1 and 2 at 20%, and 3 at 10%.
        /// </summary>
        /// <returns></returns>
        MathHelper.IntRange[] GetWeightedRangeForNewDirection()
        {
            MathHelper.IntRange[] weightedRange = new MathHelper.IntRange[4];
            weightedRange[0] = new MathHelper.IntRange(0, 0, 50);
            weightedRange[1] = new MathHelper.IntRange(1, 1, 20);
            weightedRange[2] = new MathHelper.IntRange(2, 2, 20);
            weightedRange[3] = new MathHelper.IntRange(3, 3, 10);
            return weightedRange;
        }

        /// <summary>
        /// Get a random, valid block start position based the new block size.
        /// </summary>
        /// <returns></returns>
        Vector2Int GetRandomBlockStartPosition(Vector2Int newBlockSize)
        {
            Vector2Int startPos = new Vector2Int();

            int sanityChecker = 0;
            #region Set up blocksToCheck
            List<CityBlock> blocksToCheck = new List<CityBlock>();

            // Holds each "wave" of blocks before adding them to blocksToCheck.
            List<CityBlock> waveToAdd = new List<CityBlock>();
            //Debug.Log($"Checking cityBlocks of count {cityBlocks.Count}");
            // Generate each wave of blocks, add it to blocksToCheck at random, and then generate the next wave.
            for (int i = 7; i < CityMapSize; i += 7)
            {
                waveToAdd = CityBlocks.FindAll(block =>
                {
                    Vector2Int distanceFromCenter;
                    foreach (Vector3Int tilePos in block.BlockTiles)
                    {
                        distanceFromCenter = GetTilemapDistanceFromCityCenter(new Vector2Int(tilePos.x, tilePos.y));
                        if((distanceFromCenter.x <= i && distanceFromCenter.y <= i/* && distanceFromCenter.x >= 0 + (i / 7) && distanceFromCenter.y >= 0 + (i / 7)*/))
                        {
                            // Don't add the same block more than once.
                            if(waveToAdd.Contains(block) || blocksToCheck.Contains(block))
                            {
                                return false;
                            }
                            return (distanceFromCenter.x <= i && distanceFromCenter.y <= i/* && distanceFromCenter.x >= 0 + (i / 7) + (currentExpansionBand / 2) && distanceFromCenter.y >= 0 + (i / 7) + (currentExpansionBand / 2)*/);
                        }
                    }
                    return false;
                });
                // Randomise the list                
                MathHelper.Shuffle(waveToAdd);
                //Debug.Log($"Adding {waveToAdd.Count} blocks to blocksToCheck.");
                blocksToCheck.AddRange(waveToAdd);
                if(i / 7 > CurrentExpansionBand)
                {
                    break;
                }
            }
            //blocksToCheck.Reverse();
            blocksToCheck.Shuffle();
            //Debug.Log($"{blocksToCheck.Count} blocks to check");
            int blocksToCheckStartCount = blocksToCheck.Count;
            //blocksToCheck.Shuffle();
                #endregion
            //Debug.Log($"Checking {blocksToCheck.Count} blocks.");
            bool checkingTiles = true;
            while (checkingTiles)
            {
                // Break if we're stuck in a loop or out of blocks to check.
                if (sanityChecker > blocksToCheckStartCount + 10 || blocksToCheck.Count == 0)
                {
                    
                    //Debug.Log($"Breaking here. SanityChecker = {sanityChecker}, blocksToCheck = {blocksToCheck.Count}");
                    break;
                }
                // Get the next block.
                CityBlock block = blocksToCheck[blocksToCheck.Count - 1];

                // Remove block from blocksToCheck
                blocksToCheck.Remove(block);


                startPos = GetBlockStartPositionFromBlock(block, newBlockSize);
                // If startPos == (-1,-1), we were unable to find a valid start, so iterate again.
                if (startPos.x == -1 && startPos.y == -1)
                {
                    sanityChecker++;
                    continue;
                }
                else
                {
                    return startPos;
                }
            }

            return startPos = new Vector2Int(-1,-1);
        }
        
        /// <summary>
        /// Get a random block start position based off of the given block position and new block size.
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Vector2Int GetBlockStartPositionFromBlock(CityBlock block, Vector2Int newBlockSize)
        {
            // Array of tile block directions, where 0 = N, 1 = NE, 7 = NW.
            Vector3Int[][] tilesToCheck = new Vector3Int[8][];

            // Set up tilesToCheck, offsetting tile positions as needed to reflect newBlockSize and block.Blocksize.
            #region Set up tilesToCheck
            Vector3Int[] tempArray;
            // Set up tilesToCheck

            // N
            tempArray = new Vector3Int[block.BlockFootprint.x - 2];
            // Check all non-corner North blocks
            for (int x = 1; x < block.BlockFootprint.x - 1; x++)
            {
                tempArray[x - 1] = block.BlockTiles[x, block.BlockFootprint.y - 1];
            }
            tilesToCheck[0] = tempArray;

            // NE
            tempArray = new Vector3Int[2];
            // Check NE Block
            tempArray[0] = block.BlockTiles[block.BlockFootprint.x - 1, block.BlockFootprint.y - 1];
            // Check E Block
            tempArray[1] = block.BlockTiles[block.BlockFootprint.x - 1, 0];

            tilesToCheck[1] = tempArray;

            // E
            tempArray = new Vector3Int[block.BlockFootprint.y - 2];
            // Check all non-corner East blocks
            for (int y = 1; y < block.BlockFootprint.y - 1; y++)
            {
                tempArray[y - 1] = block.BlockTiles[block.BlockFootprint.x - 1, y];
            }
            tilesToCheck[2] = tempArray;

            // SE
            tempArray = new Vector3Int[2];
            // Check SE Block
            tempArray[0] = block.BlockTiles[block.BlockFootprint.x - 1, 0];
            // Adjust for new block size
            tempArray[0].y -= newBlockSize.y - 1;
            // Check S Block
            tempArray[1] = block.BlockTiles[0, 0];
            // Adjust for new block size
            tempArray[1].y -= newBlockSize.y - 1;

            tilesToCheck[3] = tempArray;

            // S
            tempArray = new Vector3Int[block.BlockFootprint.x - 2];
            // Check all non-corner South blocks
            for (int x = 1; x < block.BlockFootprint.x - 1; x++)
            {
                tempArray[x - 1] = block.BlockTiles[x, 0];
            }
            tilesToCheck[4] = tempArray;

            // SW
            tempArray = new Vector3Int[2];
            // Check SW Block
            tempArray[0] = block.BlockTiles[0, 0];
            // Adjust for new block size
            tempArray[0].x -= newBlockSize.x - 1;
            tempArray[0].y -= newBlockSize.y - 1;
            // Check W Block
            tempArray[1] = block.BlockTiles[0, 0];
            // Adjust for new block size
            tempArray[1].x -= newBlockSize.x - 1;

            tilesToCheck[5] = tempArray;

            // W
            tempArray = new Vector3Int[block.BlockFootprint.y - 2];
            // Check all non-corner West blocks
            for (int y = 1; y < block.BlockFootprint.y - 1; y++)
            {
                tempArray[y - 1] = block.BlockTiles[0, y];
                // Adjust for new block size
                tempArray[y - 1].x -= newBlockSize.x - 1;
            }
            tilesToCheck[6] = tempArray;

            // NW
            tempArray = new Vector3Int[2];
            // Check NW Block
            tempArray[0] = block.BlockTiles[0, block.BlockFootprint.y - 1];
            // Adjust for new block size
            tempArray[0].x -= newBlockSize.x - 1;
            // Check N Block
            tempArray[1] = block.BlockTiles[0, block.BlockFootprint.y - 1];

            tilesToCheck[7] = tempArray;

            #endregion

            #region Checking Tiles
            // Check corners first, then sides.
            bool checkingTiles = true;
            int sanityCheck2 = 0;

            // Small chance to skip corner checking.
            if (MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[] { new MathHelper.IntRange(0, 0, 50), new MathHelper.IntRange(1, 1, 50) }) == 1)
            {

                // Check corners
                List<int> cornerIndicesToCheck = new List<int> { 1, 3, 5, 7 };
                while (checkingTiles)
                {
                    // If there are no tiles left to check, or we fail the sanity check, go to the next tile.
                    if (cornerIndicesToCheck.Count == 0 || sanityCheck2 > 5)
                    {
                        break;
                    }
                    // Get a random index to check
                    int indexToCheck = cornerIndicesToCheck[Random.Range(0, cornerIndicesToCheck.Count)];
                    // Remove it from the list to check
                    cornerIndicesToCheck.Remove(indexToCheck);
                    // Check the given direction
                    foreach (Vector3Int toCheck in tilesToCheck[indexToCheck])
                    {
                        //Debug.Log($"Checking {indexToCheck}");
                        int iteration = 0;
                        Vector2Int testPos = new Vector2Int(toCheck.x, toCheck.y);
                        // Small chance to skip adjustments
                        if (MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[] { new MathHelper.IntRange(0, 0, 70), new MathHelper.IntRange(1, 1, 30) }) == 1)
                        {
                            // Check if Block can be moved to fill in gaps. Returns testPos if no valid changes.
                            testPos = CheckIfAdjacentBlockPositionsValid(testPos, indexToCheck, iteration, newBlockSize);
                        }
                        //Debug.Log($"Checking tile {toCheck}.");
                        if (ValidateBlockTiles(GetNewBlockTiles(testPos, newBlockSize)))
                        {
                            return testPos;
                        }
                        iteration++;
                    }
                    sanityCheck2++;

                }
            }
            // Reset sanity check2
            sanityCheck2 = 0;
            // Check sides
            List<int> sideIndicesToCheck = new List<int> { 0, 2, 4, 6 };
            while (checkingTiles)
            {
                // If there are no tiles left to check, or we fail the sanity check, go to the next tile.
                if (sideIndicesToCheck.Count == 0 || sanityCheck2 > 5)
                {
                    break;
                }
                // Get a random index to check
                int indexToCheck = sideIndicesToCheck[Random.Range(0, sideIndicesToCheck.Count)];
                // Remove it from the list to check
                sideIndicesToCheck.Remove(indexToCheck);
                // Check the given direction
                foreach (Vector3Int toCheck in tilesToCheck[indexToCheck])
                {
                    //Debug.Log($"Checking tile {toCheck}.");
                    if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(toCheck.x, toCheck.y), newBlockSize)))
                    {
                        return new Vector2Int(toCheck.x, toCheck.y);
                    }
                }
                sanityCheck2++;

            }
            #endregion
            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// Checks if a given startPos would fit better in a similar position. Returns input startPos if not.
        /// directionIndex 0 = N, 1 = NE, 7 = NW. Direction should be where new block would be built relative to base block.
        /// </summary>
        /// <returns></returns>
        Vector2Int CheckIfAdjacentBlockPositionsValid(Vector2Int startPos, int directionIndex, int iteration, Vector2Int newBlockSize)
        {
            Vector2Int testPosition = startPos;
            // True if roads found on the last pass -- used to determine when to stop.
            bool roadsLastPass = false;
            switch (directionIndex + iteration)
            {
                // N Block
                case 0:
                    {
                        //Debug.Log($"Checking adjacent block positions N. Original startPos {startPos}.");
                        // Check to the West
                        for (int x = 1; x < newBlockSize.x; x++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x - x, startPos.y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.y; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x - x, startPos.y + i, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }
                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "W"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "W"))
                                    {
                                        // If top or bottom tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.y - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // NE Block
                case 1:
                    {
                        //Debug.Log($"Checking adjacent block positions NE. Original startPos {startPos}.");
                        // Check to the West
                        for (int x = 1; x < newBlockSize.x; x++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x - x, startPos.y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.y; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x - x, startPos.y + i, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "W"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                    
                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "W"))
                                    {
                                        // If top or bottom tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.y - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if(ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // E Block
                case 2:
                    {
                        //Debug.Log($"Checking adjacent block positions E. Original startPos {startPos}.");
                        // Check to the North
                        for (int y = 1; y < newBlockSize.y; y++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x, startPos.y + y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.x; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + i, startPos.y + y, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "N"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "N"))
                                    {
                                        // If left or right tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.x - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // SE Block
                case 3:
                    {                        
                        //Debug.Log($"Checking adjacent block positions SE. Original startPos {startPos}.");
                        // Check to the North
                        for (int y = 1; y < newBlockSize.y; y++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x, startPos.y + y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.x; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + i, startPos.y + y, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "N"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "N"))
                                    {
                                        // If left or right tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.x - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // S Block
                case 4:
                    {
                        //Debug.Log($"Checking adjacent block positions S. Original startPos {startPos}.");
                        // Check to the East
                        for (int x = 1; x < newBlockSize.x; x++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x + x, startPos.y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.y; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + x, startPos.y + i, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "E"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "E"))
                                    {
                                        // If top or bottom tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.y - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // SW Block
                case 5:
                    {
                        //Debug.Log($"Checking adjacent block positions SW. Original startPos {startPos}.");
                        // Check to the East
                        for (int x = 1; x < newBlockSize.x; x++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x + x, startPos.y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.y; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + x, startPos.y + i, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "E"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "E"))
                                    {
                                        // If top or bottom tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.y - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // W Block
                case 6:
                    {
                        //Debug.Log($"Checking adjacent block positions W. Original startPos {startPos}.");
                        // Check to the South
                        for (int y = 1; y < newBlockSize.y; y++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x, startPos.y - y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.x; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + i, startPos.y - y, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "S"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "S"))
                                    {
                                        // If left or right tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.x - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
                // NW Block
                case 7:
                    {
                        //Debug.Log($"Checking adjacent block positions NW. Original startPos {startPos}.");
                        // Check to the South
                        for (int y = 1; y < newBlockSize.y; y++)
                        {
                            Vector3Int startTile = new Vector3Int(startPos.x, startPos.y - y, 0);
                            //Debug.Log($"Checking adjacent start position {startTile}.");
                            for (int i = 0; i < newBlockSize.x; i++)
                            {
                                Vector3Int currentTile = new Vector3Int(startPos.x + i, startPos.y - y, 0);
                                Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(currentTile.x, currentTile.y);
                                if (!CheckIfCityMapIndexValid(cityMapIndex))
                                {
                                    return testPosition;
                                }

                                if (!roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "S"))
                                    {
                                        roadsLastPass = true;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }

                                }
                                else if (roadsLastPass)
                                {
                                    if (CheckIfAdjacentRoadInDirection(currentTile, "S"))
                                    {
                                        // If left or right tile, ignore -- they're fine if they are one top of existing roads.
                                        if (i == newBlockSize.x - 1 || i == 0)
                                        {
                                            continue;
                                        }
                                        //Debug.Log($"Hit two roads in a row on pass {i} at tile {currentTile}.");
                                        // Two roads in a row in non-Top/Bottom positions = invalid.
                                        return testPosition;
                                    }
                                    else if (CityMap[cityMapIndex.x, cityMapIndex.y] == true)
                                    {
                                        //Debug.Log($"Hit city tile on pass {i} at tile {currentTile}.");
                                        // Something is built here! Invalid.
                                        return testPosition;
                                    }
                                }
                            }
                            //Debug.Log($"Pass {x - 1} clear. Now validating new start position {startTile}.");
                            // If we made it this far, see if new position is valid
                            if (ValidateBlockTiles(GetNewBlockTiles(new Vector2Int(startTile.x, startTile.y), newBlockSize)))
                            {
                                //Debug.Log($"Success!");
                                // If so, assign it to reflect the valid adjustment.
                                testPosition = new Vector2Int(startTile.x, startTile.y);
                            }
                            else
                            {
                                //Debug.Log($"Validation failed.");
                            }

                        }
                        break;
                    }
            }

            return startPos;
        }

        /// <summary>
        /// Return true if given cityMapIndex is valid.
        /// </summary>
        /// <param name="cityMapIndex"></param>
        /// <returns></returns>
        bool CheckIfCityMapIndexValid(Vector2Int cityMapIndex)
        {
            if(cityMapIndex.x > CityMapSize - 1 || cityMapIndex.y > CityMapSize - 1 || cityMapIndex.x < 0 || cityMapIndex.y < 0)
            {
                return false;
            }
            return true;
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
                        if (worldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "E":
                    {
                        posToCheck = new Vector3Int(tilePos.x + 1, tilePos.y, 0);
                        if (worldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "S":
                    {
                        posToCheck = new Vector3Int(tilePos.x, tilePos.y - 1, 0);
                        if (worldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "W":
                    {
                        posToCheck = new Vector3Int(tilePos.x - 1, tilePos.y, 0);
                        if (worldMap.objectTilemap.GetTile<RoadTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// Returns true if water found at adjacent tile in given direction.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        bool CheckIfAdjacentWaterInDirection(Vector3Int tilePos, string direction)
        {
            Vector3Int posToCheck;
            switch (direction)
            {
                case "N":
                    {
                        posToCheck = new Vector3Int(tilePos.x, tilePos.y + 1, 0);
                        if (worldMap.terrainTilemap.GetTile<WaterTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "E":
                    {
                        posToCheck = new Vector3Int(tilePos.x + 1, tilePos.y, 0);
                        if (worldMap.terrainTilemap.GetTile<WaterTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "S":
                    {
                        posToCheck = new Vector3Int(tilePos.x, tilePos.y - 1, 0);
                        if (worldMap.terrainTilemap.GetTile<WaterTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
                case "W":
                    {
                        posToCheck = new Vector3Int(tilePos.x - 1, tilePos.y, 0);
                        if (worldMap.terrainTilemap.GetTile<WaterTile>(posToCheck))
                        {
                            return true;
                        }
                        break;
                    }
            }
            return false;
        }

        /// <summary>
        /// Generate a random, valid block size to build.
        /// </summary>
        /// <returns></returns>
        Vector2Int GetRandomBlockSize()
        {
            Vector2Int blockSize;
            int sizeX = 5;
            int sizeY = 5;

            // Get the X size
            MathHelper.IntRange[] weightedRangeX = new MathHelper.IntRange[5];
            weightedRangeX[0] = new MathHelper.IntRange(3, 3, 5);
            weightedRangeX[0] = new MathHelper.IntRange(4, 4, 15);
            weightedRangeX[1] = new MathHelper.IntRange(5, 5, 40);
            weightedRangeX[2] = new MathHelper.IntRange(6, 6, 25);
            weightedRangeX[3] = new MathHelper.IntRange(7, 7, 15);
            sizeX = MathHelper.RandomRange.WeightedRange(weightedRangeX);
            // Get a viable matching Y size
            MathHelper.IntRange[] weightedRangeY = new MathHelper.IntRange[5];
            weightedRangeX[0] = new MathHelper.IntRange(3, 3, 5);
            weightedRangeY[0] = new MathHelper.IntRange(4, 4, 15);
            weightedRangeY[1] = new MathHelper.IntRange(5, 5, 40);
            weightedRangeY[2] = new MathHelper.IntRange(6, 6, 25);
            weightedRangeY[3] = new MathHelper.IntRange(7, 7, 15);
            sizeY = MathHelper.RandomRange.WeightedRange(weightedRangeY);

            return blockSize = new Vector2Int(sizeX, sizeY);
        }


        /// <summary>
        /// Builds a bridge of the given length in the given direction. Two city blocks at its end as well.
        /// </summary>
        /// <param name="bridgeStartPos"></param>
        /// <param name="bridgeStopPos"></param>
        /// <param name="direction">Direction N/E/S/W in which to build bridge.</param>
        void BuildBridge(Vector3Int bridgeStartPos, Vector3Int bridgeStopPos, string direction)
        {
            //Debug.Log($"Building bridge starting at {bridgeStartPos}, ending at {bridgeStopPos}, going {direction}.");
            Vector3Int[] bridgeTiles = new Vector3Int[0];
            Vector2Int blockPosOne = new Vector2Int();
            Vector2Int blockPosTwo = new Vector2Int();

            // Get bridge tiles
            switch (direction)
            {
                case "N":
                    {
                        bridgeTiles = new Vector3Int[Mathf.Abs(bridgeStartPos.y - bridgeStopPos.y)];
                        int i = 0;
                        for (int y = bridgeStartPos.y; y < bridgeStopPos.y; y++)
                        {
                            bridgeTiles[i] = new Vector3Int(bridgeStartPos.x, y, 0);
                            i++;
                        }
                        blockPosOne = new Vector2Int(bridgeStopPos.x - 4, bridgeStopPos.y);
                        blockPosTwo = new Vector2Int(bridgeStopPos.x, bridgeStopPos.y);
                        break;
                    }
                case "S":
                    {
                        bridgeTiles = new Vector3Int[Mathf.Abs(bridgeStartPos.y - bridgeStopPos.y)];
                        int i = 0;
                        for (int y = bridgeStartPos.y; y > bridgeStopPos.y; y--)
                        {
                            bridgeTiles[i] = new Vector3Int(bridgeStartPos.x, y, 0);
                            i++;
                        }
                        blockPosOne = new Vector2Int(bridgeStopPos.x - 4, bridgeStopPos.y - 4);
                        blockPosTwo = new Vector2Int(bridgeStopPos.x, bridgeStopPos.y - 4);
                        break;
                    }
                case "E":
                    {
                        bridgeTiles = new Vector3Int[Mathf.Abs(bridgeStartPos.x - bridgeStopPos.x)];
                        int i = 0;
                        for (int x = bridgeStartPos.x; x < bridgeStopPos.x; x++)
                        {
                            bridgeTiles[i] = new Vector3Int(x, bridgeStartPos.y, 0);
                            i++;
                        }
                        blockPosOne = new Vector2Int(bridgeStopPos.x, bridgeStopPos.y);
                        blockPosTwo = new Vector2Int(bridgeStopPos.x, bridgeStopPos.y - 4);
                        break;
                    }
                case "W":
                    {
                        bridgeTiles = new Vector3Int[Mathf.Abs(bridgeStartPos.x - bridgeStopPos.x)];
                        int i = 0;
                        for (int x = bridgeStartPos.y; x > bridgeStopPos.y; x--)
                        {
                            bridgeTiles[i] = new Vector3Int(x, bridgeStartPos.y, 0);
                            i++;
                        }
                        blockPosOne = new Vector2Int(bridgeStopPos.x - 4, bridgeStopPos.y);
                        blockPosTwo = new Vector2Int(bridgeStopPos.x - 4, bridgeStopPos.y - 4);
                        break;
                    }
            }
            if(bridgeTiles.Length > 15)
            {
                return;
            }
            //Debug.Log($"Verifying new bridge blocks starting at {blockPosOne} and {blockPosTwo}.");
            // Verify bridge end blocks..
            if(!ValidateBlockTiles(GetNewBlockTiles(blockPosOne, new Vector2Int(5,5))) && !(ValidateBlockTiles(GetNewBlockTiles(blockPosTwo, new Vector2Int(5, 5)))))
            {
                //Debug.Log("Failed!");
                return;
            }
            //Debug.Log("Success!");
            // Build the bridge
            for(int i = 0; i < bridgeTiles.Length; i++)
            {
                BuildTileInCity(bridgeTiles[i], "Road_City");
            }
            BridgesBuilt++;
            CurrentExpansionBand--;
            // Build two blocks at its' end, if they validate.
            if (ValidateBlockTiles(GetNewBlockTiles(blockPosOne, new Vector2Int(5, 5))))
                BuildCityBlock(blockPosOne, new Vector2Int(5, 5), BlockZoneType.Special, ZoningParameters.None);
            if (ValidateBlockTiles(GetNewBlockTiles(blockPosTwo, new Vector2Int(5, 5))))
                BuildCityBlock(blockPosTwo, new Vector2Int(5, 5), BlockZoneType.Special, ZoningParameters.None);
            CurrentExpansionBand++;
        }

        /// <summary>
        /// Build given tileKey at given location, if location is within the city. Adds land to cityMap.
        /// </summary>
        /// <param name="positionToBuild"></param>
        /// <param name="tileKey"></param>
        void BuildTileInCity(Vector3Int positionToBuild, string tileKey)
        {
            Vector2Int cityMapIndex = GetCityMapIndexFromTilePos(positionToBuild.x, positionToBuild.y);
            if (!CheckIfCityMapIndexValid(cityMapIndex))
            {
                return;
            }
            // If building a road, do things differently.
            if (tileKey.Contains("Road"))
            {
                worldMap.PlaceRoad(positionToBuild, tileKey);
            }
            else
            {
                //worldMap.PlaceTileAtLocation(tileKey, positionToBuild, worldMap.objectTilemap);
                Debug.Log($"Attempting to build {tileKey} at {positionToBuild}... Unfortunately, this method currently only handles road tiles.");
            }
            CityMap[cityMapIndex.x, cityMapIndex.y] = true;
        }

        void AddTilesToCityMap(Vector3Int[,] tilesToAdd)
        {
            for(int x = 0; x < tilesToAdd.GetLength(0); x++)
            {
                for (int y = 0; y < tilesToAdd.GetLength(1); y++)
                {
                    Vector2Int index = GetCityMapIndexFromTilePos(tilesToAdd[x,y].x, tilesToAdd[x,y].y);
                    //Debug.Log($"Adding tile {index.x},{index.y} to city map.");
                    CityMap[index.x, index.y] = true;
                    // Mark tiles as owned by city.
                    worldMap.MapData[tilesToAdd[x, y].x, tilesToAdd[x, y].y].IsOwnedByCity = true;
                }
            }
            UpdateCityExtents();
        }

        void RemoveTilesFromCityMap(Vector3Int[,] tilesToRemove)
        {
            for (int x = 0; x < tilesToRemove.GetLength(0); x++)
            {
                for (int y = 0; y < tilesToRemove.GetLength(1); y++)
                {
                    Vector2Int index = GetCityMapIndexFromTilePos(x, y);
                    CityMap[index.x, index.y] = false;
                    // Mark tiles as unowned by city.
                    worldMap.MapData[tilesToRemove[x, y].x, tilesToRemove[x, y].y].IsOwnedByCity = false;

                }
            }
            UpdateCityExtents();
        }

        void InitializeCityMap()
        {
            // Get cityMapSize!
            CityMapSize = worldMap.MapSize;
            CityMap = new bool[CityMapSize, CityMapSize];

            for(int x = 0; x < CityMap.GetLength(0); x++)
            {
                for(int y = 0; y < CityMap.GetLength(1); y++)
                {
                    // Mark any tiles with water as occupied.
                    if(worldMap.CheckIfTileHasWater(x, y))
                    {
                        CityMap[x, y] = true;
                    }
                    CityMap[x, y] = false;
                }
            }
        }

        /// <summary>
        /// Returns true if given tile is owned by city.
        /// </summary>
        /// <param name="tileMapPosToCheck"></param>
        /// <returns></returns>
        public bool CheckIfOwnedByCity(Vector3Int tileMapPosToCheck)
        {
            //Vector2Int index = GetCityMapIndexFromTilePos(tileMapPosToCheck.x, tileMapPosToCheck.y);
            //// Make sure index is within the array...
            //bool owned = false;
            //try
            //{
            //    owned = CityMap[index.x, index.y];
            //    return owned;
            //}
            //catch
            //{
            //    return owned;
            //}
            return worldMap.GetTile(tileMapPosToCheck).IsOwnedByCity;
        }

        /// <summary>
        /// Returns true if the provided tiles are valid to build upon.
        /// </summary>
        /// <param name="tilesToCheck">The tilemap positions of the tiles to check.</param>
        /// <returns></returns>
        bool ValidateBlockTiles(Vector3Int[,] tilesToCheck)
        {
            bool valid = true;
            //Debug.Log("Validating tiles...");
            for(int x = 0; x < tilesToCheck.GetLength(0); x++)
            {
                for(int y = 0; y < tilesToCheck.GetLength(1); y++)
                {
                    if(!ValidateBlockTile(tilesToCheck[x, y]))
                    {
                        return valid = false;                        
                    }
                }
            }

            return valid;
        }

        /// <summary>
        /// Returns true if the give tilemap position is valid to build a new block on.
        /// </summary>
        /// <param name="tileToCheck">The tilemap position of the tile to check.</param>
        /// <returns></returns>
        bool ValidateBlockTile(Vector3Int tileToCheck)
        {
            bool valid = true;
            Vector2Int mapIndex = GetCityMapIndexFromTilePos(tileToCheck.x, tileToCheck.y);
            //Debug.Log($"Validing tile {tileToCheck} with a mapIndex of {mapIndex}.");
            // Make sure the tile position is within the bounds of the tilemap.
            if(tileToCheck.x < 0 || tileToCheck.x >= worldMap.MapSize || tileToCheck.y < 0 || tileToCheck.y >= worldMap.MapSize)
            {
                //Debug.Log("Tilemap position outside of map size! Invalid!");
                //Debug.Log($"Invalid position {tileToCheck}.");
                return valid = false;
            }
            // Make sure the tile position is within the bounds of the citymap.
            if (mapIndex.x < 0 || mapIndex.x >= CityMapSize || mapIndex.y < 0 || mapIndex.y >= CityMapSize)
            {
                //Debug.Log("Index out of cityMap range! Invalid!");
                //Debug.Log($"Invalid index {mapIndex} with position {tileToCheck}.");
                return valid = false;
            }
            // If there city has built here
            if (CityMap[mapIndex.x, mapIndex.y] == true)
            {
                //Debug.Log("Tile is part of the city...");
                // And it is a road
                if (worldMap.objectTilemap.GetTile<RoadTile>(CityMapIndexToVector3TileMapCoordinates(mapIndex)))
                {
                    //Debug.Log("It is a road. Valid!");
                    // Return valid
                    return valid = true;
                }
                else
                {
                    //Debug.Log("And it is not a road. Invalid.");
                    // Otherwise, it's a non-road city tile -- invalid position.
                    return valid = false;
                }
            }
            // If the tile is not on the city map, but is a water tile, it's invalid.
            else if (worldMap.terrainTilemap.GetTile<WaterTile>(CityMapIndexToVector3TileMapCoordinates(mapIndex)))
            {
                return valid = false;
            }
            // Otherwise, the tile is valid!
                return valid;
        }

        /// <summary>
        /// Return a Vector2Int reflecting the given index relative to the city center index in the cityMap.
        /// </summary>
        /// <returns></returns>
        Vector2Int GetCityMapIndexFromTilePos(int tilePosX, int tilePosY)
        {
            Vector2Int index;
            int indexX = CityMapCenter.x;
            int indexY = CityMapCenter.y;
            // Find the proper index by adding or subtracting the position to add from the city center position.
            if (CityCenter.x > tilePosX)
            {
                indexX -= CityCenter.x - tilePosX;
            }
            else if (CityCenter.x < tilePosX)
            {
                indexX += tilePosX - CityCenter.x;
            }
            if (CityCenter.y > tilePosY)
            {
                indexY -= CityCenter.y - tilePosY;
            }
            else if (CityCenter.y < tilePosY)
            {
                indexY += tilePosY - CityCenter.y;
            }
            index = new Vector2Int(indexX, indexY);

            return index;
        }

        /// <summary>
        /// Returns a Vector3Int holding the tilemap position based on the offset from the city center tile.
        /// Positive X/Y translate to N/E, while negative X/Y translate to S/W.
        /// </summary>
        /// <param name="cityCenterOffset"></param>
        /// <returns></returns>
        Vector2Int GetTilemapPositionRelativeToCityCenter(Vector2Int cityCenterOffset)
        {
            Vector2Int distance = new Vector2Int(CityCenter.x, CityCenter.y);

            if (cityCenterOffset.x > 0)
            {
                distance.x += cityCenterOffset.x;
            }
            else if (cityCenterOffset.x < 0)
            {
                distance.x += cityCenterOffset.x;
            }
            if (cityCenterOffset.y > 0)
            {
                distance.y += cityCenterOffset.y;
            }
            else if (cityCenterOffset.y < 0)
            {
                distance.y += cityCenterOffset.y;
            }

            return distance;
        }

        /// <summary>
        /// Returns a Vector3Int holding the tilemap position based on the offset from the city center tile.
        /// Positive X/Y translate to N/E, while negative X/Y translate to S/W.
        /// </summary>
        /// <param name="cityCenterOffset"></param>
        /// <returns></returns>
        Vector2Int GetTilemapPositionRelativeToCityCenter(int cityCenterOffsetX, int cityCenterOffsetY)
        {
            Vector2Int distance = new Vector2Int(CityCenter.x, CityCenter.y);

            if (cityCenterOffsetX > 0)
            {
                distance.x += cityCenterOffsetX;
            }
            else if (cityCenterOffsetX < 0)
            {
                distance.x += cityCenterOffsetX;
            }
            if (cityCenterOffsetY > 0)
            {
                distance.y += cityCenterOffsetY;
            }
            else if (cityCenterOffsetY < 0)
            {
                distance.y += cityCenterOffsetY;
            }

            return distance;
        }

        /// <summary>
        /// Returns a Vector2Int holding the distance of the start position from the city center.
        /// Always return positive values.
        /// </summary>
        /// <param name="startPos"></param>
        /// <returns></returns>
        Vector2Int GetTilemapDistanceFromCityCenter(Vector2Int startPos)
        {
            int distanceX = Mathf.Abs(CityCenter.x - startPos.x);
            int distanceY = Mathf.Abs(CityCenter.y - startPos.y);
            return new Vector2Int(distanceX, distanceY);
        }

        /// <summary>
        /// Converts given cityMapIndex to tile map coordinates.
        /// </summary>
        /// <param name="cityMapIndex"></param>
        /// <returns></returns>
        Vector3Int CityMapIndexToVector3TileMapCoordinates(Vector2Int cityMapIndex)
        {
            int posX = CityCenter.x;
            int posY = CityCenter.y;

            if(cityMapIndex.x > CityMapCenter.x)
            {
                posX += (cityMapIndex.x - CityMapCenter.x);
            }
            else if(cityMapIndex.x < CityMapCenter.x)
            {
                posX -= (CityMapCenter.x - cityMapIndex.x);
            }

            if (cityMapIndex.y > CityMapCenter.y)
            {
                posY += (cityMapIndex.y - CityMapCenter.y);
            }
            else if (cityMapIndex.y < CityMapCenter.y)
            {
                posY -= (CityMapCenter.y - cityMapIndex.y);
            }
            return new Vector3Int(posX, posY, 0);
        }

        /// <summary>
        /// Converts given cityMapIndex to tile map coordinates.
        /// </summary>
        /// <param name="cityMapIndex"></param>
        /// <returns></returns>
        Vector2Int CityMapIndexToVector2TileMapCoordinates(Vector2Int cityMapIndex)
        {
            int posX = CityCenter.x;
            int posY = CityCenter.y;

            if (cityMapIndex.x > CityMapCenter.x)
            {
                posX += (cityMapIndex.x - CityMapCenter.x);
            }
            else if (cityMapIndex.x < CityMapCenter.x)
            {
                posX -= (CityMapCenter.x - cityMapIndex.x);
            }

            if (cityMapIndex.y > CityMapCenter.y)
            {
                posY += (cityMapIndex.y - CityMapCenter.y);
            }
            else if (cityMapIndex.y < CityMapCenter.y)
            {
                posY -= (CityMapCenter.y - cityMapIndex.y);
            }
            return new Vector2Int(posX, posY);
        }

        /// <summary>
        /// Return block tiles' positions on tilemap
        /// </summary>
        /// <param name="blockStartPos">Block start tilemap position.</param>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        Vector3Int[,] GetNewBlockTiles(Vector2Int blockStartPos, Vector2Int blockSize)
        {
            Vector3Int[,] tiles = new Vector3Int[blockSize.x, blockSize.y];
            for (int x = 0; x < blockSize.x; x++)
            {
                for (int y = 0; y < blockSize.y; y++)
                {
                    tiles[x, y] = new Vector3Int(blockStartPos.x + x, blockStartPos.y + y, 0);
                }
            }

            return tiles;
        }
    }
}
