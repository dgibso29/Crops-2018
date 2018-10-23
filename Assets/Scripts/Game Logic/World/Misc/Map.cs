using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using UnityEngine.Networking;

namespace Crops.World
{
    public class Map : MonoBehaviour, ILoadable
    {

        public GameManager gameManager;

        public MapData Data
        {
            get { return gameManager.gameData.mapData; }
            set { gameManager.gameData.mapData = value; }
        }

        /// <summary>
        /// Reference to entire map grid.
        /// </summary>
        public Grid tileMapGrid;

        /// <summary>
        /// Tilemap for terrain.
        /// </summary>
        public Tilemap terrainTilemap;

        /// <summary>
        /// Tilemap for vegetation, roads, and constructions.
        /// </summary>
        public Tilemap objectTilemap;

        /// <summary>
        /// Tilemap for building interiors (barns, etc).
        /// </summary>
        public Tilemap interiorTilemap;

        /// <summary> 
        /// Tilemap for fences.
        /// </summary>
        public Tilemap fenceTilemap;

        /// <summary>
        /// Tilemap for vehicles, animals, and other agents.
        /// </summary>
        public Tilemap agentTilemap;

        /// <summary>
        /// Tilemap for any 'roof' tiles. Specifically intended for sci-fi style domes and etc.
        /// </summary>
        public Tilemap roofTilemap;

        /// <summary>
        /// Tilemap for weather effects.
        /// </summary>
        public Tilemap weatherTilemap;

        /// <summary>
        /// Name of the tileset in use for this map.
        /// </summary>
        public string CurrentTilesetName
        {
            get { return Data.currentTilesetName; }
            set { Data.currentTilesetName = value; }
        }

        /// <summary>
        /// Starting point for land value assignment.
        /// </summary>
        public float BaseLandValue
        {
            get { return Data.baseLandValue; }
            set { Data.baseLandValue = value; }
        }      
        
        /// <summary>
        /// Holds all LandPlots.
        /// </summary>
        public LandPlot[,] MapData
        {
            get { return Data.mapData; }
            set { Data.mapData = value; }
        }

        /// <summary>
        /// Holds all BuildableObjectData for serialization.
        /// </summary>
        List<BuildableObjectData> BuildableObjectDataList
        {
            get { return Data.buildableObjectDataList; }
            set { Data.buildableObjectDataList = value; }
        }

        /// <summary>
        /// Holds all BuildableObjects at runtime. This list is not serialized, and is recreated on deserialization.
        /// </summary>
        public List<BuildableObject> MasterBuildableObjectList { get; internal set; } = new List<BuildableObject>();
        
        /// <summary>
        /// River tiles to delete after terrain generation. 
        /// Used to ensure moisture generation doesn't abruptly change at map edges.
        /// </summary>
        List<Vector3Int> offMapRiverTilesToDelete = new List<Vector3Int>();

        List<Vector3Int> allWaterTiles = new List<Vector3Int>();

        /// <summary>
        /// Holds distance from water values for all tiles.
        /// </summary>
        int[,] moistureMap;

        /// <summary>
        /// Holds the default upper limit of the first 4 moisture bands. 
        /// Each entry is used to generate some variation in the actual value used at map generation.
        /// NOTE: Should not be used for generation.
        /// </summary>
        public int[] moistureBandBreakpointDefaults = new int[4];

        /// <summary>
        /// Holds the upper limit of the first 4 moisture bands. Should be used for generation.
        /// </summary>
        int[] moistureBandBreakpoints = new int[4];

        /// <summary>
        /// Dictionary of the tileset currently in use. 
        /// All dictionaries should have the same keys to allow seamless swapping between tilesets.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ScriptableObject> activeTileDictionary;

        public Tileset currentTileset;

        public Vector3Int currentTileCoordinates;

        /// <summary>
        /// Map size. Map is always a square (mapSize * mapSize).
        /// </summary>
        public int MapSize
        {
            get { return Data.mapSize; }
        }

        /// <summary>
        /// Map size reference for custom Tiles. Messy but necessary implementation.
        /// </summary>
        public static int StaticMapSize;

        /// <summary>
        /// Sets size of map. Should only be run before generation.
        /// </summary>
        /// <param name="newSize"></param>
        public void SetMapSize(int newSize)
        {
            Data.mapSize = newSize;
        }

        int riverStartPositionOffset = 10;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.instance;
                gameManager.worldMap = this;
            }
        }

        // Use this for initialization
        void Start()
        {
            if(gameManager == null)
            {
                gameManager = GameManager.instance;
                gameManager.worldMap = this;
            }
            Data = gameManager.gameData.mapData;
        }

        // Update is called once per frame
        void Update()
        {
            GetCurrentTileCoordinates();
        }

        void GetCurrentTileCoordinates()
        {
            currentTileCoordinates = terrainTilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        /// <summary>
        /// Returns the land plot under the mouse cursor.
        /// </summary>
        /// <returns></returns>
        public LandPlot GetCurrentLandPlot()
        {
            return MapData[currentTileCoordinates.x, currentTileCoordinates.y];
        }

        void GenerateNewMap()
        {
            GenerateMap(0, Random.Range(1, 4));
        }



        /// <summary>
        /// Start map from scratch, including generation. Used for new games.
        /// </summary>
        public void StartNewMap()
        {
            InitializeTilesetInfo();
            SetUpStaticMapSize();
            InitializeMapData();
            GenerateNewMap();
        }

        /// <summary>
        /// Sets up tileset dictionary, and performs any additional maintenance tasks.
        /// </summary>
        void InitializeTilesetInfo()
        {
            currentTileset = Utilities.AssetBundleHelper.LoadedTilesets[CurrentTilesetName];
            InitializeTilesetDictionaries();
        }

        void InitializeMapData()
        {
            MapData = new LandPlot[MapSize, MapSize];
            BuildableObjectDataList = new List<BuildableObjectData>();
        }

        void InitializeTilesetDictionaries()
        {
            // Set active tileset
            activeTileDictionary = currentTileset.Dictionary;
            Tileset.SetActiveTilesetDictionary(activeTileDictionary);

        }

        void SetUpStaticMapSize()
        {
            StaticMapSize = MapSize;
        }

        /// <summary>
        /// Returns tile at given position.
        /// </summary>
        /// <param name="tilePosX"></param>
        /// <param name="tilePosY"></param>
        /// <returns></returns>
        public LandPlot GetTile(int tilePosX, int tilePosY)
        {
            return MapData[tilePosX, tilePosY];
        }

        /// <summary>
        /// Returns tile at given position.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public LandPlot GetTile(Vector2Int tilePos)
        {
            return MapData[tilePos.x, tilePos.y];
        }

        /// <summary>
        /// Returns tile at given position.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        public LandPlot GetTile(Vector3Int tilePos)
        {
            return MapData[tilePos.x, tilePos.y];
        }

        /// <summary>
        /// Loads the map from a save.
        /// </summary>
        public void InitializeFromSave()
        {
            InitializeTilesetInfo();
            SetUpStaticMapSize();
            LoadTerrainTilemap();
            LoadObjectTilemap();

        }

        void LoadTerrainTilemap()
        {
            for(int x = 0; x < MapSize; x++)
            {
                for(int y = 0; y < MapSize; y++)
                {
                    if (MapData[x, y].TerrainTileTypeKey.Contains("Terrain"))
                    {
                        TerrainTile newTile = ScriptableObject.CreateInstance<TerrainTile>();
                        newTile.SetAssetReference(activeTileDictionary[MapData[x, y].TerrainTileTypeKey]);

                        newTile.SetTileCoordinates(x, y);
                        newTile.SetTileVariant(MapData[x, y].TerrainTileVariant);
                        newTile.SetTileRotation(MapData[x, y].TerrainTileRotation);
                        terrainTilemap.SetTile(new Vector3Int(x, y, 0), newTile);
                    }
                    else
                    {
                        PlaceTileAtLocation<WaterTile>(MapData[x, y].PlotCoordinates3D, terrainTilemap, activeTileDictionary["Water"]);
                    }

                    if (MapData[x,y].HasFence == true)
                    {
                        PlaceFence(MapData[x, y].PlotCoordinates3D, MapData[x, y].FenceDictionaryKey);
                    }
                    if(MapData[x,y].HasRoad == true)
                    {
                        PlaceRoad(MapData[x, y].PlotCoordinates3D, MapData[x, y].RoadDictionaryKey);
                        // Set up crosswalks if relevant
                        if(MapData[x,y].IsCrosswalk == true)
                        {
                            objectTilemap.GetTile<RoadTile>(MapData[x,y].PlotCoordinates3D).isCrosswalk = true;
                            objectTilemap.RefreshTile(MapData[x, y].PlotCoordinates3D);
                        }
                    }
                }
            }
            terrainTilemap.RefreshAllTiles();
        }

        void LoadObjectTilemap()
        {
            // Do not load roads -- handled as part of landplot loading.
            foreach(BuildableObjectData objectData in BuildableObjectDataList)
            {
                PlaceBuildableObject<BuildableObject, BuildableObjectData, BuildableObjectTile>(objectData);
            }
        }




        #region World Modification


        /// <summary>
        /// Places given tile of the indicated type at given location. Position must be validated before calling function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tilePos"></param>
        /// <param name="tileMap"></param>
        public void PlaceTileAtLocation<T>(Vector3Int tilePos, Tilemap tileMap) where T : CustomTile
        {
            var newTile = ScriptableObject.CreateInstance<T>();
            tileMap.SetTile(tilePos, newTile);
        }

        /// <summary>
        /// Places given tile of the indicated type at given location. The tile will reference the given asset. Position must be validated before calling function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tilePos"></param>
        /// <param name="tileMap"></param>
        /// <param name="assetReference"></param>
        public void PlaceTileAtLocation<T>(Vector3Int tilePos, Tilemap tileMap, ScriptableObject assetReference) where T : CustomTile
        {
            var newTile = ScriptableObject.CreateInstance<T>();
            newTile.SetAssetReference(assetReference);
            tileMap.SetTile(tilePos, newTile);
        }
        /// <summary>
        /// Places given tile of the indicated type at given location, with the given sprite. The tile will reference the given asset. Position must be validated before calling function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spriteToUse"></param>
        /// <param name="tilePos"></param>
        /// <param name="tileMap"></param>
        public void PlaceTileAtLocation<T>(Sprite spriteToUse, Vector3Int tilePos, Tilemap tileMap) where T : CustomTile
        {
            var newTile = ScriptableObject.CreateInstance<T>();
            newTile.sprite = spriteToUse;            
            tileMap.SetTile(tilePos, newTile);
        }

        /// <summary>
        /// Places given tile of the indicated type at given location, with the given sprite. The tile will reference the given asset. Position must be validated before calling function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="spriteToUse"></param>
        /// <param name="tilePos"></param>
        /// <param name="tileMap"></param>
        /// <param name="assetReference"></param>
        public void PlaceTileAtLocation<T>(Sprite spriteToUse, Vector3Int tilePos, Tilemap tileMap, ScriptableObject assetReference) where T : CustomTile
        {
            var newTile = ScriptableObject.CreateInstance<T>();
            newTile.sprite = spriteToUse;
            newTile.SetAssetReference(assetReference);
            tileMap.SetTile(tilePos, newTile);
        }

        /// <summary>
        /// Removes the tile at the given location on the given tilemap.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="tilemap"></param>
        public void RemoveTileAtLocation(Vector3Int tilePos, Tilemap tilemap)
        {
            tilemap.SetTile(tilePos, ScriptableObject.CreateInstance<Tile>());
        }

        #endregion
        #region World Generation

        /// <summary>
        /// Generate world map based on input parameters.
        /// </summary>
        void GenerateMap(int numOfLakes, int numOfRivers)
        {
            GenerateRivers(numOfRivers);
            GenerateTerrain();
            GenerateCity();
        }

        #region River Generation

        void DeleteOffMapRiverTiles()
        {
            foreach(Vector3Int pos in offMapRiverTilesToDelete)
            {
                allWaterTiles.Remove(pos);
                terrainTilemap.SetTile(pos, ScriptableObject.CreateInstance("Tile") as Tile);
            }
        }

        void GenerateRivers(int numOfRivers)
        {
            int[] numberOfRivers = new int[numOfRivers];
            Vector3Int startTilePos;
            // Get start/end tile for each river and generate it.
            for (int i = 0; i < numOfRivers; i++)
            {
                startTilePos = GetRandomRiverStartTile();
                while (!ValidateRiverStartTile(startTilePos))
                {
                    startTilePos = GetRandomRiverStartTile();
                }

                GenerateRiver(startTilePos);
            }
        }

        void GenerateRiver(Vector3Int startTilePos)
        {
            Vector3Int[] riverPath = GetRiverPath(startTilePos).ToArray();

            // Account for all off-map tiles for later deletion.
            foreach (Vector3Int pos in riverPath)
            {
                if (pos.x < 0 || pos.x > MapSize - 1 || pos.y < 0 || pos.y > MapSize - 1)
                {
                    offMapRiverTilesToDelete.Add(pos);
                }
                /// Add all water tiles to a list
                allWaterTiles.Add(pos);
            }
            ScriptableObject waterAsset = activeTileDictionary["Water"];
            foreach (Vector3Int tilePos in riverPath)
            {
                PlaceTileAtLocation<WaterTile>(tilePos, terrainTilemap, waterAsset);
            }

        }

        bool ValidateRiverStartTile(Vector3Int startTilePos)
        {
            bool valid = true;
            //Debug.Log(startTilePos);
            if ((startTilePos.x == 0 || startTilePos.x == MapSize - 1) && (startTilePos.y < 25 || startTilePos.y > MapSize - 26))
            {
                    valid = false;                
            }
            else if ((startTilePos.y == 0 || startTilePos.y == MapSize - 1) && (startTilePos.x < 25 || startTilePos.x > MapSize - 26))
            {
                valid = false;
            }


            return valid;
        }

        /// <summary>
        /// Find and return river path.
        /// </summary>
        /// <param name="startTilePos"></param>
        /// <param name="endTilePos"></param>
        /// <returns></returns>
        List<Vector3Int> GetRiverPath(Vector3Int startTilePos)
        {
            List<Vector3Int> riverTiles = new List<Vector3Int>();
            int maxWidth = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange(7, 11, 55), new MathHelper.IntRange(10, 16, 45));
            int minWidth = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange(3, 5, 55), new MathHelper.IntRange(4, 7, 45));

            // Make sure max width is never smaller than min width.
            if(minWidth > maxWidth)
            {
                maxWidth = minWidth + 2 + Random.Range(1, 5);
            }
            //Debug.Log($"minWidth: {minWidth}, maxWidth: {maxWidth}.");
            // Tile we are currently working with.
            Vector3Int currentTilePos = startTilePos;
            Vector3Int previousTilePos = startTilePos;

            string previousDirection = GetStartDirection(startTilePos);
            string newDirection = previousDirection;
            //Debug.Log($"Start direction {previousDirection}");
            //Debug.Log(startTilePos);
            int currentWidth = Random.Range(3, maxWidth);

            bool clearToChangeDirection = true;
            int passWhenDirectionChanged = 0;

            bool clearToChangeWidth = false;
            int passWhenWidthChanged = 0;

            int sanitychecker = 0;
            // Quick test
            int attemptedDirectionChange = 0;
            int directionChangeSucceeded = 0;
            bool pathing = true;
            while (pathing)
            {
                if (sanitychecker > 500)
                {
                    pathing = false;
                    break;
                }

                sanitychecker++;

                //Debug.Log($"currentTilePos {currentTilePos}");
                //Debug.Log($"New direction: {newDirection}");
                // Add current row of river tiles
                for (int i = 0; i < currentWidth; i++)
                {
                    // If we were going East or West -- Corners: DONE
                    if (previousDirection == "E" || previousDirection == "W")
                    {
                        // And we are going North or South
                        if (newDirection == "N" && newDirection == "S")
                        {
                            // Add width along X axis.
                            riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                        }
                        else if(newDirection == "NE")
                        {
                            // Add width diagonally to the NE
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - 1, 0));

                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - c, 0));
                                } 
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));

                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - c - 1, 0));
                                }
                            }
                        }
                        else if(newDirection == "SE")
                        {
                            // Add width diagonally to the SE
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + 1, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + c, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i , currentTilePos.y + i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + c + 1, 0));
                                }
                            }
                        }
                        else if(newDirection == "SW")
                        {
                            // Add width diagonally to the SW
                            if (i == 0)
                            { 
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + 1, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + c, 0));
                                }

                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2 + 1; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + c + 1, 0));
                                }
                            }
                        }
                        else if (newDirection == "NW")
                        {
                            // Add width diagonally to the NW
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - 1, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - c, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));                
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - c - 1, 0));
                                }
                            }
                        }
                        // Otherwise, we're going East or West
                        else
                        {
                            // Add width along Y axis.
                            riverTiles.Add(new Vector3Int(currentTilePos.x, ((currentTilePos.y - (currentWidth / 2)) + i), 0));
                        }
                    }
                    // If we were going North or South -- Corners: DONE
                    else if(previousDirection == "N" || previousDirection == "S")
                    {
                        // And we are going East or West
                        if (newDirection == "E" && newDirection == "W")
                        {
                            // Add width along Y axis.
                            riverTiles.Add(new Vector3Int(currentTilePos.x, ((currentTilePos.y - (currentWidth / 2)) + i), 0));
                        }
                        else if (newDirection == "NE")
                        {
                            // Add width diagonally to the NE
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                // Add Corner tiles
                                for(int c = 1; c < currentWidth/2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - c, currentTilePos.y, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {                                
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - c - 1, currentTilePos.y + i, 0));
                                }                                
                            }
                        }
                        else if (newDirection == "SE")
                        {
                            // Add width diagonally to the SE
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - c, currentTilePos.y, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - c - 1, currentTilePos.y - i, 0));
                                }
                            }
                        }
                        else if (newDirection == "SW")
                        {
                            // Add width diagonally to the SW
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + c, currentTilePos.y, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + c + 1, currentTilePos.y - i, 0));
                                }
                            }
                        }
                        else if (newDirection == "NW")
                        {
                            // Add width diagonally to the NW
                            if (i == 0)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y, 0));
                                // Add Corner tiles
                                for (int c = 1; c < currentWidth / 2 + 1; c++)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + c, currentTilePos.y, 0));
                                }
                            }
                            else if (i < currentWidth / 2)
                            {
                                riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i, 0));
                                riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));
                                // Add corner tiles as needed
                                for (int c = i; c < currentWidth / 2; c++)
                                {
                                    //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + c + 1, currentTilePos.y + i, 0));
                                }
                            }
                        }
                        // Otherwise, we're going North or South
                        else
                        {
                            // Add width along X axis.
                            riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                        }
                    }
                    // Otherwise, we were going diagonally -- Corners: CHECKED. Still have single corner creation -- should be obsolete.
                    else
                    {
                        // If we were going NE -- Corners: CHECKED, still has single corner creation
                        if(previousDirection == "NE")
                        {
                            // And we're now going N
                            if(newDirection == "N")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y - 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + c, currentTilePos.y - 1, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + c + 1, currentTilePos.y - i - 1, 0));
                                    }
                                }
                                // Add width along X axis.
                                riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                            }
                            else if(newDirection == "NE")
                            {
                                // Add width diagonally to the NE
                                if (i == 0)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                    // First 2 backfill tiles
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - 1, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y, 0));
                                }
                                else if (i < currentWidth / 2)
                                {
                                        // 'Backfill' tiles
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i - 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y + i, 0));

                                        // 'Transition' Backfill tiles -- make width changes smoother on diagonals. Only add when i is >1.
                                        //Debug.Log($"Backfilled! Prev dir: {previousDirection}, new dir: {newDirection}. i = {i}. CurrTile {currentTilePos}. CurrWidth {currentWidth}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y + i - 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i - 1, currentTilePos.y - i - 1, 0));

                                    //riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y + i - 1, 0));
                                    //riverTiles.Add(new Vector3Int(currentTilePos.x + i - 1, currentTilePos.y - i - 1, 0));

                                    // Regular width
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i , 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));
                                }
                            }
                            // And we're now going E
                            else if(newDirection == "E")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y + 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y + c, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y + c + 1, 0));
                                    }
                                }
                                // Add width along Y axis.
                                riverTiles.Add(new Vector3Int(currentTilePos.x, (currentTilePos.y - (currentWidth / 2)) + i, 0));


                            }
                        }
                        // If we were going SE -- Corners: CHECKED, still has single corner creation
                        else if (previousDirection == "SE")
                        {
                            if (newDirection == "S")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y + 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + c, currentTilePos.y + 1, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + c + 1, currentTilePos.y + i + 1, 0));
                                    }
                                }
                                // Add width along X axis.
                                riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                            }
                            else if (newDirection == "SE")
                            {
                                // Add width diagonally to the SE
                                if (i == 0)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                    // First 2 backfill tiles
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + 1, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y, 0));
                                }
                                else if (i < currentWidth / 2)
                                {                             
                                        // 'Backfill' tiles
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i + 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y - i, 0));
                                        
                                        // 'Transition' Backfill tiles -- make width changes smoother on diagonals. Only add when i is >1.
                                        //Debug.Log($"Backfilled! Prev dir: {previousDirection}, new dir: {newDirection}. i = {i}. CurrTile {currentTilePos}. CurrWidth {currentWidth}.");

                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y - i + 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i - 1, currentTilePos.y + i + 1, 0));
                                    
                                    // Regular width
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));
                                }
                            }
                            else if (newDirection == "E")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y - 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y - c, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i - 1, currentTilePos.y - c - 1, 0));
                                    }
                                }
                                // Add width along Y axis.
                                riverTiles.Add(new Vector3Int(currentTilePos.x, (currentTilePos.y - (currentWidth / 2)) + i, 0));

                            }
                        }
                        // If we were going SW -- Corners: CHECKED, still has single corner creation
                        else if (previousDirection == "SW") 
                        {
                            if (newDirection == "S")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y + 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - c, currentTilePos.y + 1, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - c - 1, currentTilePos.y + i + 1, 0));
                                    }
                                }

                                // Add width along X axis.
                                riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                            }
                            else if (newDirection == "SW")
                            {
                                // Add width diagonally to the SW
                                if (i == 0)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                    // First 2 backfill tiles
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y + 1, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y, 0));
                                }
                                else if (i < currentWidth / 2)
                                {
                                        // 'Backfill' tiles
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i + 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y - i, 0));


                                        // 'Transition' Backfill tiles -- make width changes smoother on diagonals. Only add when i is >1.
                                        //Debug.Log($"Backfilled! Prev dir: {previousDirection}, new dir: {newDirection}. i = {i}. CurrTile {currentTilePos}. CurrWidth {currentWidth}.");

                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y - i + 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i + 1, currentTilePos.y + i + 1, 0));
                                    
                                    // Regular width
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y + i, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y - i, 0));
                                }

                            }
                            else if (newDirection == "W")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y - 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y - c, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y - c - 1, 0));
                                    }
                                }
                                // Add width along Y axis.
                                riverTiles.Add(new Vector3Int(currentTilePos.x, (currentTilePos.y - (currentWidth / 2)) + i, 0));
                            }
                        }
                        // If we were going NW -- Corners: CHECKED, still has single corner creation
                        else if (previousDirection == "NW") 
                        {
                            if (newDirection == "N")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - 1, currentTilePos.y - 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2 + 1; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - c, currentTilePos.y - 1, 0));
                                    }
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}. CurrPos: {currentTilePos}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - c - 1, currentTilePos.y - i - 1, 0));
                                    }
                                }
                                // Add width along X axis.
                                riverTiles.Add(new Vector3Int((currentTilePos.x - (currentWidth / 2)) + i, currentTilePos.y, 0));
                            }
                            else if (newDirection == "NW")
                            {
                                // Add width diagonally to the NW
                                if (i == 0)
                                {
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y, 0));
                                    // First 2 backfill tiles
                                    riverTiles.Add(new Vector3Int(currentTilePos.x, currentTilePos.y - 1, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y, 0));
                                }
                                else if (i < currentWidth / 2)
                                {                       
                                        // 'Backfill' tiles
                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i - 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y + i, 0));


                                        // 'Transition' Backfill tiles -- make width changes smoother on diagonals. Only add when i is >1.
                                        //Debug.Log($"Backfilled! Prev dir: {previousDirection}, new dir: {newDirection}. i = {i}. CurrTile {currentTilePos}. CurrWidth {currentWidth}.");

                                        riverTiles.Add(new Vector3Int(currentTilePos.x - i + 1, currentTilePos.y - i - 1, 0));
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y + i - 1, 0));                                 

                                    

                                    // Regular width
                                    riverTiles.Add(new Vector3Int(currentTilePos.x - i, currentTilePos.y - i, 0));
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + i, currentTilePos.y + i, 0));
                                }

                            }
                            else if (newDirection == "W")
                            {
                                if (i == 0)
                                {
                                    // Add the corner tile
                                    riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y + 1, 0));
                                    // Add Corner tiles
                                    for (int c = 1; c < currentWidth / 2; c++)
                                    {
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + 1, currentTilePos.y + c, 0));
                                    }
                                    //Debug.Log("NW - W, i = 0");
                                }
                                else
                                {
                                    // Add corner tiles as needed
                                    for (int c = i; c < currentWidth / 2; c++)
                                    {
                                        
                                        //Debug.Log($"This happened! Prev dir: {previousDirection}, new dir: {newDirection}. i = {i}. CurrTile {currentTilePos}. CurrWidth {currentWidth}.");
                                        riverTiles.Add(new Vector3Int(currentTilePos.x + i + 1, currentTilePos.y + c + 1, 0));
                                    }
                                }
                                // Add width along Y axis.
                                riverTiles.Add(new Vector3Int(currentTilePos.x, (currentTilePos.y - (currentWidth / 2)) + i, 0));
                            }
                        }
                    }
                }

                // Save previous direction before proceeding.
                previousDirection = newDirection;

                if (sanitychecker - passWhenDirectionChanged > 12)
                {
                    clearToChangeDirection = true;
                }
                // generate a new direction IF we pass the random check.
                if (clearToChangeDirection && MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange(1, 1, 20), new MathHelper.IntRange(0, 0, 80)) == 1)
                {
                    attemptedDirectionChange++;
                    newDirection = GetNextRiverTileDirection(previousDirection, sanitychecker);
                }
                if (newDirection != previousDirection)
                {
                    clearToChangeDirection = false;
                    passWhenDirectionChanged = sanitychecker;
                    directionChangeSucceeded++;
                }

                Vector3Int newTilePos = new Vector3Int();
                // Get the next tile position
                switch (newDirection)
                {
                    case "N":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x, currentTilePos.y + 1, currentTilePos.z);
                            break;
                        }
                    case "NE":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x + 1, currentTilePos.y + 1, currentTilePos.z);
                            break;
                        }
                    case "E":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x + 1, currentTilePos.y, currentTilePos.z);
                            break;
                        }
                    case "SE":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x + 1, currentTilePos.y - 1, currentTilePos.z);
                            break;
                        }
                    case "S":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x, currentTilePos.y - 1, currentTilePos.z);
                            break;
                        }
                    case "SW":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x - 1, currentTilePos.y - 1, currentTilePos.z);
                            break;
                        }
                    case "W":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x - 1, currentTilePos.y, currentTilePos.z);
                            break;
                        }
                    case "NW":
                        {
                            newTilePos = new Vector3Int(currentTilePos.x - 1, currentTilePos.y + 1, currentTilePos.z);
                            break;
                        }
                }

                // Sanity Check -- If the new tile is the same tile, start this pass over.
                if (newTilePos == currentTilePos)
                {
                    continue;
                }

                // Assign the new tile.
                previousTilePos = currentTilePos;
                currentTilePos = newTilePos;

                if (sanitychecker - passWhenWidthChanged > 4)
                {
                    clearToChangeWidth = true;
                }

                // generate a new width IF we pass the random check AND if the cooldown has expired.
                if (clearToChangeWidth && MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange(1, 1, 50), new MathHelper.IntRange(0, 1, 50)) == 1)
                {
                    currentWidth = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange(currentWidth - 1, currentWidth - 1, 25), new MathHelper.IntRange(currentWidth, currentWidth, 30), new MathHelper.IntRange(currentWidth + 1, currentWidth + 1, 20));
                    currentWidth = Mathf.Clamp(currentWidth, minWidth, maxWidth);
                    clearToChangeWidth = false;
                    passWhenWidthChanged = sanitychecker;
                }

                // Make sure we aren't already at the end of the map! If so, we're done!
                if(currentTilePos.x == -10 || currentTilePos.x == MapSize + 10 || currentTilePos.y == -10 || currentTilePos.y == MapSize + 10)
                {
                    break;
                }
    

            }
            Debug.Log($"River generated. Attempt direction changes: {attemptedDirectionChange}. Succesful attempts: {directionChangeSucceeded}. Total generation passes: {sanitychecker}.");

            return riverTiles;
        }

        /// <summary>
        /// Return direction (N/NE/E/SE/S/ etc) in which to place next river tile segment.
        /// </summary>
        /// <param name="currentTilePos"></param>
        /// <param name="endTilePos"></param>
        /// <returns></returns>
        string GetNextRiverTileDirection(string previousDirection, int passTracker)
        {
            float weightModifier = 1;
            // If still in early stages of river pathing, make sure it doesn't double back on itself.
            if(passTracker < 20)
            {
                weightModifier = 0;
                if(passTracker > 5)
                {
                    weightModifier = 1 / passTracker;
                }
            }

            switch (previousDirection)
            {
                case "N":
                    {
                        return GetWeightedRandomDirection(60, 20 * weightModifier, 0, 0, 0, 0, 0, 20 * weightModifier);                        
                    }
                case "NE":
                    {
                        return GetWeightedRandomDirection(20 * weightModifier, 60, 20 * weightModifier, 0, 0, 0, 0, 0);
                    }
                case "E":
                    {
                        return GetWeightedRandomDirection(0, 20 * weightModifier, 60, 20 * weightModifier, 0, 0, 0, 0);
                    }
                case "SE":
                    {
                        return GetWeightedRandomDirection(0, 0, 20 * weightModifier, 60, 20 * weightModifier, 0, 0, 0);
                    }
                case "S":
                    {
                        return GetWeightedRandomDirection(0, 0, 0, 20 * weightModifier, 60, 20 * weightModifier, 0, 0);
                    }
                case "SW":
                    {
                        return GetWeightedRandomDirection(0, 0, 0, 0, 20 * weightModifier, 60, 20 * weightModifier, 0);
                    }
                case "W":
                    {
                        return GetWeightedRandomDirection(0, 0, 0, 0, 0, 20 * weightModifier, 60, 20 * weightModifier);
                    }
                case "NW":
                    {
                        return GetWeightedRandomDirection(20 * weightModifier, 0, 0, 0, 0, 0, 20 * weightModifier, 60);
                    }
            }
            
            return "lol";
        }

        /// <summary>
        /// Return random direction based on provided weights.
        /// </summary>
        /// <returns></returns>
        string GetWeightedRandomDirection(float weightN, float weightNE, float weightE, float weightSE, float weightS,
            float weightSW, float weightW, float weightNW)
        {
            string direction = "null";

            MathHelper.IntRange[] weightedRanges = new MathHelper.IntRange[] 
            {
                new MathHelper.IntRange(1, 1, weightN), new MathHelper.IntRange(2, 2, weightNE),
                new MathHelper.IntRange(3, 3, weightE), new MathHelper.IntRange(4, 4, weightSE),
                new MathHelper.IntRange(5, 5, weightS), new MathHelper.IntRange(6, 6, weightSW),
                new MathHelper.IntRange(7, 7, weightW), new MathHelper.IntRange(8, 8, weightNW),
            };
            int result = MathHelper.RandomRange.WeightedRange(weightedRanges);
            switch (result)
            {
                case 1:
                    {
                        direction = "N";
                        break;
                    }
                case 2:
                    {
                        direction = "NE";
                        break;
                    }
                case 3:
                    {
                        direction = "E";
                        break;
                    }
                case 4:
                    {
                        direction = "SE";
                        break;
                    }
                case 5:
                    {
                        direction = "S";
                        break;
                    }
                case 6:
                    {
                        direction = "SW";
                        break;
                    }
                case 7:
                    {
                        direction = "W";
                        break;
                    }
                case 8:
                    {
                        direction = "NW";
                        break;
                    }
            }
            //Debug.Log($" Result = {result}, Direction returned is {direction}");
            return direction;
        }



        /// <summary>
        /// Returns a string direction in which to start river pathing.
        /// </summary>
        /// <returns></returns>
        string GetStartDirection(Vector3Int startTilePos)
        {
            string direction = "None";

            // Determine which direction we are going based on endTilePos.
            // If start Y equals mapSize, the start is north.
            if (startTilePos.y == MapSize - 1 + riverStartPositionOffset)
            {
                direction = "S";
            }
            // If start Y equals = 0, the start is south.
            else if (startTilePos.y == 0 - riverStartPositionOffset)
            {
                direction = "N";

            }
            // If start X equals mapSize, the start is east.
            else if (startTilePos.x == MapSize - 1 + riverStartPositionOffset)
            {
                direction = "W";

            }
            // If start X equals 0, the start is west.
            else if (startTilePos.x == 0 - riverStartPositionOffset)
            {
                direction = "E";

            }

            return direction;
        }

        #endregion

        #region Lake Generation
        void GenerateLakes(int numOfLakes)
        {

        }
        #endregion

        #region Terrain Generation

        /// <summary>
        /// Generates underlying map terrain.
        /// </summary>
        void GenerateTerrain()
        {
            // Set up water tile rotations
            //terrainTilemap.RefreshAllTiles();

            int[,] moistureMap = GenerateMoistureMap();
            for (int x = 0; x < MapSize; x++)
            {
                for (int y = 0; y < MapSize; y++)
                {
                    if (CheckIfTileHasWater(x, y))
                    {
                        MapData[x, y] = new LandPlot(false, false, false, true, 1, "None", "Water", 0, 0,
                            GenerateTileLandValue(0), new Vector2Int(x, y), new Vector3Int(x, y, 0));
                        continue;
                    }
                    int distanceFromWater = moistureMap[x, y];
                    string terrainTileKey = GetTerrainTileTypeFromDistanceFromWater(distanceFromWater);
  
                    TerrainTile newTile = ScriptableObject.CreateInstance<TerrainTile>();
                    newTile.SetAssetReference(activeTileDictionary[terrainTileKey]);

                    // Add the tile to mapData
                    MapData[x, y] = new LandPlot(false, false, false, false, -1, "None", terrainTileKey, newTile.GetRandomTileVariant(),
                        newTile.GetRandomTileRotation(), GenerateTileLandValue(distanceFromWater), new Vector2Int(x, y), new Vector3Int(x, y, 0));

                    MapData[x, y].CurrentLandValue = MapData[x, y].BaseLandValue;
                    newTile.SetTileCoordinates(x, y);
                    terrainTilemap.SetTile(new Vector3Int(x, y, 0), newTile);

                }
            }

        }

        float GenerateTileLandValue(int distanceFromWater)
        {
            // Randomise land value from base value
            float landValue = Random.Range(BaseLandValue - 50, BaseLandValue + 50);

            // Adjust value based on distance to water
            landValue += (Random.Range(2800, 3201) - Mathf.Clamp((40 * distanceFromWater), 0, 3200));

            return landValue;
        }

        /// <summary>
        /// Returns array of neighbor terrain tiles, if any, where index 0 = N, 1 = NE, 7 = NW.
        /// </summary>
        /// <param name="centerTile"></param>
        /// <returns></returns>
        Vector3Int[] GetNeighboringTerrainTiles(Vector3Int centerTilePosition)
        {
            Vector3Int[] neighbors = new Vector3Int[8];
            // Set up each tile
            // N
            if (centerTilePosition.x > 0 || centerTilePosition.x < MapSize - 1 || centerTilePosition.y + 1 > 0 || centerTilePosition.y + 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x, centerTilePosition.y + 1, 0);
            }
            // NE
            if (centerTilePosition.x + 1 > 0 || centerTilePosition.x + 1 < MapSize - 1 || centerTilePosition.y + 1 > 0 || centerTilePosition.y + 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x + 1, centerTilePosition.y + 1, 0);
            }
            // E
            if (centerTilePosition.x - 1 > 0 || centerTilePosition.x - 1 < MapSize - 1 || centerTilePosition.y > 0 || centerTilePosition.y < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x - 1, centerTilePosition.y, 0);
            }
            // SE
            if (centerTilePosition.x + 1 > 0 || centerTilePosition.x + 1 < MapSize - 1 || centerTilePosition.y - 1 > 0 || centerTilePosition.y - 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x + 1, centerTilePosition.y - 1, 0);
            }
            // S
            if (centerTilePosition.x > 0 || centerTilePosition.x < MapSize - 1 || centerTilePosition.y - 1 > 0 || centerTilePosition.y - 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x, centerTilePosition.y - 1, 0);
            }
            // SW
            if (centerTilePosition.x - 1 > 0 || centerTilePosition.x - 1 < MapSize - 1 || centerTilePosition.y - 1 > 0 || centerTilePosition.y - 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x - 1, centerTilePosition.y - 1, 0);
            }
            // W
            if (centerTilePosition.x - 1 > 0 || centerTilePosition.x - 1 < MapSize - 1 || centerTilePosition.y > 0 || centerTilePosition.y < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x - 1, centerTilePosition.y, 0);
            }
            // NW
            if (centerTilePosition.x - 1 > 0 || centerTilePosition.x - 1 < MapSize - 1 || centerTilePosition.y - 1 > 0 || centerTilePosition.y - 1 < MapSize - 1)
            {
                neighbors[0] = new Vector3Int(centerTilePosition.x - 1, centerTilePosition.y - 1, 0);
            }

            return neighbors;
        }


        int[,] GenerateMoistureMap()
        {
             moistureMap = new int[MapSize, MapSize];

            // Generate the upper limits of the moisture bands.
            GenerateMoistureBandBreakpoints();
            //Debug.Log(moistureBandBreakpoints[3]);
            // Initialize the map
            for (int x = 0; x < MapSize; x++)
            {
                for (int y = 0; y < MapSize; y++)
                {
                    moistureMap[x, y] = moistureBandBreakpoints[3] + 6;
                }
            }

            #region Bleed From Water Tiles Approach

            // Bleed outward from each water tile
            foreach (Vector3Int pos in allWaterTiles)
            {
                AssignMoistureValuesFromWaterTile(pos);
            }
            #endregion

            //Debug.Log(moistureMap[0, 0]);
            //Debug.Log(moistureMap[148, 148]);
            // Delete off-map river tiles now that moisture map is generated.
            DeleteOffMapRiverTiles();
            return moistureMap;
        }

        /// <summary>
        /// Based on the default Moisture Band breakpoints, generate slightly randomised values.
        /// </summary>
        /// <returns></returns>
        void GenerateMoistureBandBreakpoints()
        {
            int[] breakpoints = new int[4];
            for(int i = 0; i < moistureBandBreakpointDefaults.Length; i++)
            {
                int currentDefaultBreakpoint = moistureBandBreakpointDefaults[i];
                MathHelper.IntRange[] weightedBreakpointRange = new MathHelper.IntRange[5];
                weightedBreakpointRange[0] = new MathHelper.IntRange(currentDefaultBreakpoint - 2 - i, currentDefaultBreakpoint - 2 - i, 15);
                weightedBreakpointRange[1] = new MathHelper.IntRange(currentDefaultBreakpoint - 1 - i, currentDefaultBreakpoint - 1 - i, 20);
                weightedBreakpointRange[2] = new MathHelper.IntRange(currentDefaultBreakpoint, currentDefaultBreakpoint, 30);
                weightedBreakpointRange[3] = new MathHelper.IntRange(currentDefaultBreakpoint + 1 + i, currentDefaultBreakpoint + 1 + i, 20);
                weightedBreakpointRange[4] = new MathHelper.IntRange(currentDefaultBreakpoint + 2 + i, currentDefaultBreakpoint + 2 + i, 15);
                breakpoints[i] = MathHelper.RandomRange.WeightedRange(weightedBreakpointRange);
            }
            moistureBandBreakpoints = breakpoints;
        }

        /// <summary>
        /// Returns string of terrain tile type based on distance from water.
        /// </summary>
        /// <returns></returns>
        string GetTerrainTileTypeFromDistanceFromWater(int distanceFromWater)
        {
            string tileType = "DriestTerrain";
            int[] breakpoints = new int[4];
            // Generate dynamic breakpoints
            for (int i = 0; i < moistureBandBreakpoints.Length; i++)
            {
                int currentBreakpoint = moistureBandBreakpoints[i];
                MathHelper.IntRange[] weightedBreakpointRange = new MathHelper.IntRange[5];
                weightedBreakpointRange[0] = new MathHelper.IntRange(currentBreakpoint - 2 - (currentBreakpoint > 1 ? i - 1 : 0), currentBreakpoint - 2 - (currentBreakpoint > 1 ? i - 1 : 0), 10);
                weightedBreakpointRange[1] = new MathHelper.IntRange(currentBreakpoint - 1 - (currentBreakpoint > 1 ? i - 1 : 0), currentBreakpoint - 1 - (currentBreakpoint > 1 ? i - 1 : 0), 25);
                weightedBreakpointRange[2] = new MathHelper.IntRange(currentBreakpoint, currentBreakpoint, 30);
                weightedBreakpointRange[3] = new MathHelper.IntRange(currentBreakpoint + 1 + (currentBreakpoint > 1 ? i - 1 : 0), currentBreakpoint + 1 + (currentBreakpoint > 1 ? i - 1 : 0), 25);
                weightedBreakpointRange[4] = new MathHelper.IntRange(currentBreakpoint + 2 + (currentBreakpoint > 1 ? i - 1 : 0), currentBreakpoint + 2 + (currentBreakpoint > 1 ? i - 1 : 0), 10);
                breakpoints[i] = MathHelper.RandomRange.WeightedRange(weightedBreakpointRange);
            }

            if (distanceFromWater <= breakpoints[0])
            {
                //Debug.Log($"Returning wettest terrain. Distance from water: {distanceFromWater}.");
                return tileType = "Terrain_Wettest";
            }
            else if (distanceFromWater <= breakpoints[1])
            {
                return tileType = "Terrain_Wet";
            }
            else if (distanceFromWater <= breakpoints[2])
            {
                return tileType = "Terrain_Normal";
            }
            else if (distanceFromWater <= breakpoints[3])
            {
                return tileType = "Terrain_Dry";
            }
            else if (distanceFromWater > breakpoints[3])
            {
                return tileType = "Terrain_Driest";
            }
            else
            {
                Debug.Log($"Default tile type -- distanceFromWater: {distanceFromWater}");
                return tileType;
            }
        }

        /// <summary>
        /// Bleeds outward from given WaterTile in valid directions to assign distanceFromWater values for the moisture map.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <returns></returns>
        void AssignMoistureValuesFromWaterTile(Vector3Int waterTilePos)
        {
            int startX = waterTilePos.x;
            int startY = waterTilePos.y;
            //Debug.Log($"Checking WaterTile at {startX},{startY}.");

            // Tracks validity of directions. Indexes go clock-wise, where 0 = N, 1 = NE, and 7 = NW.
            bool[] validDirections = new bool[8];
            for(int i = 0; i < 7; i++)
            {
                validDirections[i] = false;
            }

            #region Get Valid Directions
            // North
            if (!CheckIfTileHasWater(startX, startY + 1) && !(startX < 0) && !(startX > moistureMap.GetLength(0) - 1))
            {
                validDirections[0] = true;
            }
            // Northeast
            if (!CheckIfTileHasWater(startX + 1, startY + 1) && !(startY > moistureMap.GetLength(0)) && !(startX > moistureMap.GetLength(0) - 1))
            {
                validDirections[1] = true;
            }
            // East
            if (!CheckIfTileHasWater(startX + 1, startY) && !(startY < 0) && !(startY > moistureMap.GetLength(0) - 1))
            {
                validDirections[2] = true;
            }
            // Southeast
            if (!CheckIfTileHasWater(startX + 1, startY - 1) && !(startY < 0) && !(startX > moistureMap.GetLength(0) - 1))
            {
                validDirections[3] = true;
            }
            // South
            if (!CheckIfTileHasWater(startX, startY - 1) && !(startX < 0) && !(startX > moistureMap.GetLength(0) - 1))
            {
                validDirections[4] = true;
            }
            // Southwest
            if (!CheckIfTileHasWater(startX - 1, startY - 1) && !(startY < 0) && !(startX < 0))
            {
                validDirections[5] = true;
            }
            // West
            if (!CheckIfTileHasWater(startX - 1, startY) && !(startY < 0) && !(startY > moistureMap.GetLength(0) - 1))
            {
                validDirections[6] = true;
            }
            // Northwest
            if (!CheckIfTileHasWater(startX - 1, startY + 1) && !(startY > moistureMap.GetLength(0) - 1) && !(startX < 0))
            {
                validDirections[7] = true;
            }
            #endregion

            #region Bleed Outwards & Assign Moisture Values
            for (int i = 1; i <= moistureBandBreakpoints[3] + 6; i++)
            {
                //Debug.Log($"i = {i}, startPos = {startX},{startY}.");
                //if(startX + i < 0 || startY + i < 0 || startX - i > moistureMap.GetLength(0) - 1 || startY - i > moistureMap.GetLength(0) - 1)
                //{
                //    //Debug.Log("Should be stopping.");
                //    continue;
                //}
                // N
                if (validDirections[0])
                {
                    if(startY + i < 0)
                    {
                        // Do nothing!
                    }
                    else if (startY + i > moistureMap.GetLength(0) - 1)
                    {
                        validDirections[0] = false;
                    }
                    else if (moistureMap[startX, startY + i] > i)
                    {
                        //Debug.Log($"Tile {startX},{startY} should have a moisture value of {i}.");
                        moistureMap[startX, startY + i] = i;
                    }
                }
                // NE
                if (validDirections[1])
                {
                    // i / 2 to halve diagonal tile distance.
                    int diagonalOffset = i / 2;
                    //Debug.Log(diagonalOffset);
                    if (startX + diagonalOffset < 0 || startY + diagonalOffset < 0)
                    {
                        // Do nothing!
                    }
                    else if (startX + diagonalOffset > moistureMap.GetLength(0) - 1 || startY + diagonalOffset > moistureMap.GetLength(0) - 1)
                    {
                        validDirections[2] = false;
                    }
                    else if (moistureMap[startX + diagonalOffset, startY + diagonalOffset] > diagonalOffset)
                    {
                        moistureMap[startX + diagonalOffset, startY + diagonalOffset] = i;
                    }
                }
                // E
                if (validDirections[2])
                {
                    if (startX + i < 0)
                    {
                        // Do nothing!                        
                    }
                    else if (startX + i > moistureMap.GetLength(0) - 1)
                    {
                        validDirections[2] = false;
                    }
                    else if (moistureMap[startX + i, startY] > i)
                    {
                        moistureMap[startX + i, startY] = i;
                    }
                }
                // SE
                if (validDirections[3])
                {
                    // i / 2 to halve diagonal tile distance.
                    int diagonalOffset = i / 2;
                    //Debug.Log(diagonalOffset);

                    if (startX + diagonalOffset < 0 || startY - diagonalOffset > moistureMap.GetLength(0) - 1)
                    {
                        // Do nothing!
                    }
                    else if (startX + diagonalOffset > moistureMap.GetLength(0) - 1 || startY - diagonalOffset < 0)
                    {
                        validDirections[3] = false;
                    }
                    else if (moistureMap[startX + diagonalOffset, startY - diagonalOffset] > diagonalOffset)
                    {
                        moistureMap[startX + diagonalOffset, startY - diagonalOffset] = i;
                    }
                }
                // S
                if (validDirections[4])
                {
                    if (startY - i > moistureMap.GetLength(0) - 1)
                    {
                        // Do nothing!
                    }
                    else if (startY - i < 0)
                    {
                        validDirections[4] = false;
                    }
                    else if (moistureMap[startX, startY - i] > i)
                    {
                        moistureMap[startX, startY - i] = i;
                    }
                }
                // SW
                if (validDirections[5])
                {
                    // i / 2 to halve diagonal tile distance.
                    int diagonalOffset = i / 2;
                    //Debug.Log(diagonalOffset);
                    //Debug.Log($"x - diag {startX} - {diagonalOffset} = {startX - diagonalOffset}, y - diag {startY} - {diagonalOffset} = {startY - diagonalOffset}");                    
                    if (startX - diagonalOffset > moistureMap.GetLength(0) - 1 || startY - diagonalOffset > moistureMap.GetLength(0) - 1)
                    {
                        // Do nothing!
                    }
                    else if (startX - diagonalOffset < 0 || startY - diagonalOffset < 0)
                    {
                        validDirections[5] = false;
                    }
                    else if (moistureMap[startX - diagonalOffset, startY - diagonalOffset] > diagonalOffset)
                    {
                        moistureMap[startX - diagonalOffset, startY - diagonalOffset] = i;
                    }
                }
                // W
                if (validDirections[6])
                {
                    if (startX - i > moistureMap.GetLength(0) - 1)
                    {
                        // Do nothing!
                    }
                    else if (startX - i < 0)
                    {
                        validDirections[6] = false;
                    }
                    else if (moistureMap[startX - i, startY] > i)
                    {
                        moistureMap[startX - i, startY] = i;
                    }
                }
                // NW
                if (validDirections[7])
                {
                    // i / 2 to halve diagonal tile distance.
                    int diagonalOffset = i / 2;
                    //Debug.Log(diagonalOffset);

                    if (startX - diagonalOffset > moistureMap.GetLength(0) - 1 || startY + diagonalOffset < 0)
                    {
                        // Do nothing!
                    }
                    else if (startX - diagonalOffset < 0 || startY + diagonalOffset > moistureMap.GetLength(0) - 1)
                    {
                        validDirections[7] = false;
                    }
                    else if (moistureMap[startX - diagonalOffset, startY + diagonalOffset] > diagonalOffset)
                    {
                        moistureMap[startX - diagonalOffset, startY + diagonalOffset] = i;
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Returns true if tile at provided coordinates is water.
        /// </summary>
        /// <param name="tilePosX"></param>
        /// <param name="tilePosY"></param>
        /// <returns></returns>
        public bool CheckIfTileHasWater(int tilePosX, int tilePosY)
        {
            return terrainTilemap.GetTile<WaterTile>(new Vector3Int(tilePosX, tilePosY, 0));
        }

        #endregion

        #region City Generation

        void GenerateCity()
        {
            gameManager.city.InitialiseNewCity();

        }

        /// <summary>
        /// Returns a viable tileSprite key for the given zone type and footprint.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        /// <returns></returns>
        public string FindRandomBuildingOfTypeAndSize(string buildingKey, Vector2Int footprint)
        {
            var building = ((BuildableObject)activeTileDictionary[buildingKey]);
            string tileSpriteKey = $"{footprint.x}x{footprint.y}";
            // If there is a perfect match, return that key.
            if (building.tileSprites.ContainsKey(tileSpriteKey))
            {
                return tileSpriteKey;
            }
            // Otherwise, find a key that will fit.
            int footX = footprint.x, footY = footprint.y;
            // Iterate down to a 1x1 tile. There should ALWAYS be a 1x1 tile if there are no other options.
            for(int x = 0; x < footprint.x; x++)
            {
                for(int y = 0; y < footprint.y; y++)
                {
                    tileSpriteKey = $"{footprint.x - x}x{footprint.y - y}";
                    if (building.tileSprites.ContainsKey(tileSpriteKey))
                    {
                        return tileSpriteKey;
                    }
                    tileSpriteKey = $"{footprint.y - y}x{footprint.x - x}";
                    if (building.tileSprites.ContainsKey(tileSpriteKey))
                    {
                        return tileSpriteKey;
                    }
                }
            }
            /// Default to 1x1 if we've made it this far.
            return "1x1";
        }


        #endregion

        #region DecorationGeneration
        void GenerateFoliageAndRocks()
        {

        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Returns a random tile off of the tile map at which to start a river.
        /// </summary>
        /// <returns></returns>
        Vector3Int GetRandomRiverStartTile()
        {
            List<Vector3Int> edgeTiles = new List<Vector3Int>();
            for (int x = 0; x < MapSize; x++)
            {
                edgeTiles.Add(new Vector3Int(x, -riverStartPositionOffset, 0));
            }
            for (int x = 0; x < MapSize; x++)
            {
                edgeTiles.Add(new Vector3Int(x, MapSize - 1 + riverStartPositionOffset, 0));
            }
            for (int y = 1; y < MapSize - 1; y++)
            {
                edgeTiles.Add(new Vector3Int(-riverStartPositionOffset, y, 0));
            }
            for (int y = 1; y < MapSize - 1; y++)
            {
                edgeTiles.Add(new Vector3Int(MapSize - 1 + riverStartPositionOffset, y, 0));
            }

            return edgeTiles[Random.Range(0, edgeTiles.Count)];
        }
        #endregion

        #endregion

        #region Construction

        /// <summary>
        /// Place a object instantiated from the given BuildableObjectData, where the originTile is the bottom left corner.
        /// </summary>
        /// <typeparam name="ObjectType">Type of BuildableObject being placed.</typeparam>
        /// <typeparam name="TileType">Type of Tile being placed.</typeparam>
        /// <param name="newObjectData">Data used to create the object.</param>
        public void PlaceBuildableObject<ObjectType, DataType, TileType>(DataType newObjectData) where ObjectType: BuildableObject where DataType: BuildableObjectData where TileType : CustomTile
        {
            // Reference the building for instantiation
            ObjectType newObject = ScriptableObject.CreateInstance<ObjectType>();

            newObject.InitializeObject(newObjectData, ref ((BuildableObject)activeTileDictionary[newObjectData.tileDictionaryKey]).tileSprites);

            // Place each tile, associating the tile with the new object ID
            for (int x = 0; x < newObjectData.footprint.x; x++)
            {
                for (int y = 0; y < newObjectData.footprint.y; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(newObjectData.originCoords.x + x, newObjectData.originCoords.y + y, 0);

                    /// Place the tile
                    PlaceTileAtLocation<TileType>(newObject.tileSprites[newObject.TileSpriteKey][newObjectData.variant][x, y], tilePosition, objectTilemap, activeTileDictionary[newObjectData.tileDictionaryKey]);

                    /// Input the object ID on the tile
                    MapData[tilePosition.x, tilePosition.y].ObjectOnTileID = newObjectData.ID;
                }
            }
            // Add object data to buildable object data list if it doesn't already exist.
            if (!BuildableObjectDataList.Exists(entry => entry.ID == newObjectData.ID))
            {
                BuildableObjectDataList.Add(newObjectData);
            }
            // Add object to buildable object list if it doesn't already exist.
            if (!MasterBuildableObjectList.Exists(entry => entry.ID == newObject.ID))
            {
                MasterBuildableObjectList.Add(newObject);
            }
        }


        /// <summary>
        /// Removes the buildable object with the given ID.
        /// </summary>
        /// <param name="objectID"></param>
        public void RemoveBuildableObject(int objectID)
        {
            BuildableObjectData objectToRemove = BuildableObjectDataList.Find(entry => entry.ID == objectID);
            // Remove each tile, associating the tile with the new object ID
            for (int x = 0; x < objectToRemove.footprint.x; x++)
            {
                for (int y = 0; y < objectToRemove.footprint.y; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(objectToRemove.originCoords.x + x, objectToRemove.originCoords.y + y, 0);
                    /// Remove the tile
                    RemoveTileAtLocation(tilePosition, objectTilemap);
                    /// Reset the object ID on the tile to default (0).
                    MapData[tilePosition.x, tilePosition.y].ObjectOnTileID = 0; 
                }
            }
            // Remove the object from the buildable object data list.
            BuildableObjectDataList.Remove(objectToRemove);
            // Remove the object from the master list.
            MasterBuildableObjectList.Remove(GetBuildableObjectWithID<BuildableObject>(objectID));
        }

        /// <summary>
        /// Returns the BuildableObject of the given type with the given ID, if it exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectID"></param>
        /// <returns></returns>
        public T GetBuildableObjectWithID<T>(int objectID) where T: BuildableObject
        {


            ///

            ///              YOOOOOOOOOOO this needs to handle there being no object

            ///

            return MasterBuildableObjectList.Find(entry => entry.ID == objectID) as T;
        }

        /// <summary>
        /// Places a fence at the given location.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="dictionaryKey"></param>
        public void PlaceFence(Vector3Int tilePos, string dictionaryKey)
        {
            PlaceTileAtLocation<FenceTile>(tilePos, fenceTilemap, activeTileDictionary[dictionaryKey]);
            MapData[tilePos.x, tilePos.y].AddFenceData(dictionaryKey);
        }

        /// <summary>
        /// Removes the fence at the given location.
        /// </summary>
        /// <param name="tilePos"></param>
        public void RemoveFence(Vector3Int tilePos)
        {
            RemoveTileAtLocation(tilePos, fenceTilemap);
            MapData[tilePos.x, tilePos.y].RemoveFence();
        }

        /// <summary>
        /// Places a road at the given location.
        /// </summary>
        /// <param name="tilePos"></param>
        /// <param name="dictionaryKey"></param>
        public void PlaceRoad(Vector3Int tilePos, string dictionaryKey)
        {
            PlaceTileAtLocation<RoadTile>(tilePos, objectTilemap, activeTileDictionary[dictionaryKey]);
            MapData[tilePos.x, tilePos.y].AddRoadData(dictionaryKey);
        }

        /// <summary>
        /// Removes the road at the given location.
        /// </summary>
        /// <param name="tilePos"></param>
        public void RemoveRoad(Vector3Int tilePos)
        {
            RemoveTileAtLocation(tilePos, objectTilemap);
            MapData[tilePos.x, tilePos.y].RemoveRoad();
        }

        #endregion
    }
}

