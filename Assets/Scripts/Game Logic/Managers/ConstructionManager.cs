using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.World;
using Crops.Economy;

namespace Crops.Construction
{
    public class ConstructionManager : MonoBehaviour
    {
        public GameManager gameManager;
        Map worldMap;

        /// <summary>
        /// Dictionary key of the object last selected for construction.
        /// </summary>
        public string activeObjectDictionaryKey;

        // Track type of active construction -- object, road, fence, etc


        private void Start()
        {
            worldMap = gameManager.worldMap;
        }

        #region Object Construction

        /// <summary>
        /// Attempts to build the given version of the given object at the given position, if all requirements are met. 
        /// Otherwise, the relevant errors will be thrown.
        /// </summary>
        /// <param name="buildingIDToConstruct"></param>
        public void AttemptObjectConstruction<ObjectType>(string objectDictionaryKey, Vector2Int footprint, Vector3Int originTile, int objectVariant, Player activePlayer) where ObjectType : BuildableObject
        {
            ObjectType buildingRef = Tileset.TilesetDictionary[objectDictionaryKey] as ObjectType;

            float purchasePrice = buildingRef.BuildCost;

            // Confirm construction is valid.

            // Get prospective construction footprint tiles
            Vector3Int[,] constructionFootprint = GetObjectConstructionFootprintTiles(footprint, originTile);

            // Check if the player owns the building site
            if (!ValidateObjectConstructionSiteOwnership(constructionFootprint, activePlayer))
            {
                // If validation fails, return here.
                Debug.Log($"Player {activePlayer.ID} cannot build {buildingRef.NameEnglish}, as they do not own the site!");

                // Throw actual in-game error here

                return;
            }

            // Check if the player can afford the purchase
            if (!EconomyManager.ValidatePurchase(purchasePrice, activePlayer))
            {
                // If validation fails, return here.
                Debug.Log($"Player {activePlayer.ID} cannot afford to build {buildingRef.NameEnglish}!");

                // Throw actual in-game error here

                return;
            }

            // Check if the building site is clear
            if (!ValidateObjectConstructionSiteClearance(constructionFootprint))
            {
                // If validation fails, return here.
                Debug.Log($"Player {activePlayer.ID} cannot build {buildingRef.NameEnglish}, as the site is not clear!");

                // Throw actual in-game error here

                return;
            }

            // Create building data, and then build the building.
            string switchString = buildingRef.GetType().Name;

            switch (switchString)
            {
                case "BuildableObject":
                    {
                        BuildableObjectData newBuildingData = new BuildableObjectData(footprint, originTile, objectVariant, objectDictionaryKey, gameManager.AssignUniqueID(), activePlayer.ID);
                        // Build the building
                        worldMap.PlaceBuildableObject<ObjectType, BuildableObjectData, BuildableObjectTile>(newBuildingData);
                        break;
                    }
                case "Field":
                    {
                        FieldData newBuildingData = new FieldData(footprint, originTile, objectVariant, objectDictionaryKey, gameManager.AssignUniqueID(), activePlayer.ID);
                        // Build the building
                        worldMap.PlaceBuildableObject<ObjectType, FieldData, BuildableObjectTile>(newBuildingData);
                        break;
                    }
            }

            //// Build the building
            //worldMap.PlaceBuildableObject<ObjectType, DataType, BuildableObjectTile>(newBuildingData);

            // Process purchase
            EconomyManager.ProcessPurchase(purchasePrice, activePlayer);
        }

        /// <summary>
        /// Attempts to destroy the given object, if all requirements are met. Otherwise, the relevant errors will be thrown.
        /// </summary>
        /// <param name="objectToDestroyID"></param>
        public void AttemptObjectDeconstruction(int objectToDestroyID, Player activePlayer)
        {
            // Ensure attempt is valid

            // If ID = 0, there is no building to destroy.
            if(objectToDestroyID == 0)
            {
                Debug.Log("There is no building to destroy.");
                return;
            }

            // Get the object
            BuildableObject building = gameManager.GetObject(objectToDestroyID);            

            // Player can destroy this building (owns the land it is on OR owns the object)
            if (!ValidateObjectOwner(building, activePlayer) && !ValidateObjectConstructionSiteOwnership(building.GetObjectFootprintTiles(), activePlayer))
            {
                Debug.Log($"Player {activePlayer.ID} cannot destroy building {building.ID} -- Player owns neither the building nor the land it is built upon!");

                return;
            }

            // Player can afford to destroy this building (can afford destruction cost, if any)
            if (!EconomyManager.ValidatePurchase(building.DestructionCost, activePlayer))
            {
                Debug.Log($"Player {activePlayer.ID} cannot afford to destroy building {building.ID}!");

                return;
            }


            // Destroy building
            worldMap.RemoveBuildableObject(objectToDestroyID);

            // Process removal cost
            EconomyManager.ProcessPurchase(building.DestructionCost, activePlayer);
        }

        /// <summary>
        /// Returns true if given tiles are clear for construction.
        /// </summary>
        /// <returns></returns>
        bool ValidateObjectConstructionSiteClearance(Vector3Int[,] tilesToValidate)
        {
            foreach(Vector3Int tile in tilesToValidate)
            {
                // Check if there is an object on the tile.
                if(worldMap.MapData[tile.x, tile.y].ObjectOnTileID != 0)
                {
                    // If there is, return false.
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if given tiles are owned by the player.
        /// </summary>
        /// <returns></returns>
        bool ValidateObjectConstructionSiteOwnership(Vector3Int[,] tilesToValidate, Player activePlayer)
        {
            foreach (Vector3Int tile in tilesToValidate)
            {
                // Check if the player owns the tile
                if (worldMap.MapData[tile.x, tile.y].OwnerID != activePlayer.ID)
                {
                    // If they don't, return false.
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if given tile is owned by the player.
        /// </summary>
        /// <returns></returns>
        bool ValidateObjectConstructionSiteOwnership(Vector3Int tileToValidate, Player activePlayer)
        {
            if (worldMap.MapData[tileToValidate.x, tileToValidate.y].OwnerID != activePlayer.ID)
            {
                // If they don't, return false.
                return false;
            }
            return true;
        }

        /// <summary>
        /// Return true if the given player owns the given object.
        /// </summary>
        /// <param name="building"></param>
        /// <param name="activePlayer"></param>
        /// <returns></returns>
        bool ValidateObjectOwner(BuildableObject building, Player activePlayer)
        {
            // If the object owner is the active player, return true.
            if(building.OwnerID == activePlayer.ID)
            {
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns an array coordinates for a given construction site footprint.
        /// </summary>
        /// <returns></returns>
        Vector3Int[,] GetObjectConstructionFootprintTiles(Vector2Int footprint, Vector3Int origin)
        {
            Vector3Int[,] footprintTiles = new Vector3Int[footprint.x, footprint.y];

            for (int x = 0; x < footprint.x; x++)
            {
                for (int y = 0; y < footprint.y; y++)
                {
                    footprintTiles[x, y] = new Vector3Int(origin.x + x, origin.y + y, 0);
                }
            }

            return footprintTiles;
        }

        #endregion

        #region Road Construction

        /// <summary>
        /// Attempts to build the given road at the given position.
        /// </summary>
        /// <param name="buildPosition"></param>
        /// <param name="objectDictionaryKey"></param>
        /// <param name="activePlayer"></param>
        public void AttemptRoadConstruction(Vector3Int buildPosition, string objectDictionaryKey, Player activePlayer)
        {
            // Does the player own the tile?
            if (!ValidateObjectConstructionSiteOwnership(buildPosition, activePlayer))
            {
                Debug.Log($"Player {activePlayer.ID} cannot build road with key {objectDictionaryKey} -- player does not own the tile!");

                return;
            }

            // Can the player afford to build the road?

            float purchasePrice = ((RoadType)Tileset.TilesetDictionary[objectDictionaryKey]).BuildCost;

            if (!EconomyManager.ValidatePurchase(purchasePrice, activePlayer))
            {
                Debug.Log($"Player {activePlayer.ID} cannot build road with key {objectDictionaryKey} -- player cannot afford to build the road!");

                return;
            }

            // Is the tile clear to build a road upon?
            if (!ValidateRoadConstructionClearance(buildPosition))
            {
                Debug.Log($"Player {activePlayer.ID} cannot build road with key {objectDictionaryKey} -- tile is not clear to build upon!");

                return;
            }

            // Does the tile already have a road?
            if (CheckIfTileHasRoad(buildPosition))
            {
                // If so, we don't want charge the player, and we don't want to build at all if the road types are identical.

                // Check if identical road type
                if (worldMap.GetTile(buildPosition).RoadDictionaryKey == objectDictionaryKey)
                {
                    Debug.Log($"Aborting road construction -- road of same type already exists on tile.");

                    return;
                }

                // Place road without charging player.
                worldMap.PlaceRoad(buildPosition, objectDictionaryKey);
                return;
            }

            // Process purchase
            EconomyManager.ProcessPurchase(purchasePrice, activePlayer);

            // Place the road.
            worldMap.PlaceRoad(buildPosition, objectDictionaryKey);
        }

        /// <summary>
        /// Attempts to remove the road at the given position, if any.
        /// </summary>
        /// <param name="removalPosition"></param>
        /// <param name="activePlayer"></param>
        public void AttemptRoadRemoval(Vector3Int removalPosition, Player activePlayer)
        {
            LandPlot tile = worldMap.GetTile(removalPosition);
            // Does the given tile have a road?
            if (tile.HasRoad == false)
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot remove road at {removalPosition} -- there is no road there!");
                return;
            }

            // Does the tile own the road?
            if(!ValidateObjectConstructionSiteOwnership(removalPosition, activePlayer))
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot remove road at {removalPosition} -- player does not own road!");
                return;
            }

            float destructionCost = ((RoadType)Tileset.TilesetDictionary[tile.RoadDictionaryKey]).DestructionCost;
            // If so, can the player afford to remove the road?
            if (!EconomyManager.ValidatePurchase(destructionCost, activePlayer))
            {
                // Return if player cannot afford
                Debug.Log($"Player {activePlayer.ID} cannot remove road at {removalPosition} -- player cannot afford to remove it!");
                return;
            }

            // If we made it this far, remove the road
            worldMap.RemoveRoad(removalPosition);

            // And charge the player
            EconomyManager.ProcessSale(destructionCost, activePlayer);
        }

        /// <summary>
        /// Returns true if a road can be built on the given tile.
        /// </summary>
        /// <param name="tileToValidate"></param>
        /// <returns></returns>
        bool ValidateRoadConstructionClearance(Vector3Int tileToValidate)
        {
            // Is there an object on this tile?
            if(worldMap.MapData[tileToValidate.x, tileToValidate.y].ObjectOnTileID != 0)
            {
                // If so, return false.
                return false;
            }
            // Otherwise, return true. If there's already a road here, we just want to build over it.

            return true;
        }

        /// <summary>
        /// Return true if the given tile already has a road.
        /// </summary>
        /// <param name="tileToValidate"></param>
        /// <returns></returns>
        bool CheckIfTileHasRoad(Vector3Int tileToValidate)
        {
            if(worldMap.MapData[tileToValidate.x, tileToValidate.y].HasRoad)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Fence Construction

        /// <summary>
        /// Attempts to build the given fence at the given position.
        /// </summary>
        /// <param name="buildPosition"></param>
        /// <param name="objectDictionaryKey"></param>
        /// <param name="activePlayer"></param>
        public void AttemptFenceConstruction(Vector3Int buildPosition, string objectDictionaryKey, Player activePlayer)
        {

            // Does the player own the tile?
            if(!ValidateObjectConstructionSiteOwnership(buildPosition, activePlayer))
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot build {objectDictionaryKey} at {buildPosition} -- player does not own the land!");
                return;
            }

            float purchasePrice = ((FenceType)Tileset.TilesetDictionary[objectDictionaryKey]).BuildCost;
            
            // Can the player afford to build the fence?
            if (!EconomyManager.ValidatePurchase(purchasePrice, activePlayer))
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot build {objectDictionaryKey} at {buildPosition} -- player cannot afford it!");
                return;
            }


            // Is the build site clear?
            if (!ValidateFenceConstructionClearance(buildPosition))
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot build {objectDictionaryKey} at {buildPosition} -- tile is not clear to build fence!");
                return;
            }

            // Does the tile already have a fence?
            if (CheckIfTileHasFence(buildPosition))
            {
                // If so, we don't want charge the player, and we don't want to build at all if the fence types are identical.

                // Check if identical fence type
                if (worldMap.GetTile(buildPosition).FenceDictionaryKey == objectDictionaryKey)
                {
                    Debug.Log($"Aborting fence construction -- fence of same type already exists on tile.");

                    return;
                }

                // Place fence without charging player.
                worldMap.PlaceFence(buildPosition, objectDictionaryKey);
                return;
            }

            // Charge player for fence
            EconomyManager.ProcessPurchase(purchasePrice, activePlayer);

            // Build the fence
            worldMap.PlaceFence(buildPosition, objectDictionaryKey);
        }

        /// <summary>
        /// Attempts to remove the fence at the given position, if any.
        /// </summary>
        /// <param name="removalPosition"></param>
        /// <param name="activePlayer"></param>
        public void AttemptFenceRemoval(Vector3Int removalPosition, Player activePlayer)
        {
            LandPlot tile = worldMap.GetTile(removalPosition);
            // Does the given tile have a fence?
            if (tile.HasFence == false)
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot remove fence at {removalPosition} -- there is no fence there!");
                return;
            }

            // Does the tile own the fence?
            if (!ValidateObjectConstructionSiteOwnership(removalPosition, activePlayer))
            {
                // If not, return
                Debug.Log($"Player {activePlayer.ID} cannot remove fence at {removalPosition} -- player does not own fence!");
                return;
            }

            float destructionCost = ((FenceType)Tileset.TilesetDictionary[tile.FenceDictionaryKey]).DestructionCost;
            // If so, can the player afford to remove the fence?
            if (!EconomyManager.ValidatePurchase(destructionCost, activePlayer))
            {
                // Return if player cannot afford
                Debug.Log($"Player {activePlayer.ID} cannot remove fence at {removalPosition} -- player cannot afford to remove it!");
                return;
            }

            // If we made it this far, remove the fence
            worldMap.RemoveFence(removalPosition);

            // And charge the player
            EconomyManager.ProcessSale(destructionCost, activePlayer);
        }


        /// <summary>
        /// Returns true if a fence can be built on the given tile.
        /// </summary>
        /// <param name="tileToValidate"></param>
        /// <returns></returns>
        bool ValidateFenceConstructionClearance(Vector3Int tileToValidate)
        {
            // Is there an object on this tile?
            if (worldMap.MapData[tileToValidate.x, tileToValidate.y].ObjectOnTileID != 0)
            {
                // If so, can a fence be built around it?
                if (gameManager.GetObject(worldMap.MapData[tileToValidate.x, tileToValidate.y].ObjectOnTileID).AllowFence == true)
                {
                    // If so, return true
                    return true;
                }
                // Otherwise, return false.
                return false;
            }
            // Otherwise, return true. If there's already a fence here, we just want to build over it.

            return true;
        }

        /// <summary>
        /// Return true if the given tile already has a fence.
        /// </summary>
        /// <param name="tileToValidate"></param>
        /// <returns></returns>
        bool CheckIfTileHasFence(Vector3Int tileToValidate)
        {
            if (worldMap.MapData[tileToValidate.x, tileToValidate.y].HasFence)
            {
                return true;
            }

            return false;
        }


        #endregion
    }
}