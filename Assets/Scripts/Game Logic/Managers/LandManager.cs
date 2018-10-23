using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Crops.World;
using Crops;


public class LandManager : MonoBehaviour
{

    Map worldMap;
    public GameManager gameManager;


    private void Start()
    {
        worldMap = gameManager.worldMap;
    }

    /// <summary>
    /// Attempt to purchase tiles given if player can afford and land is not otherwise invalid.
    /// </summary>
    /// <param name="tilesToPurchase"></param>
    public void AttemptToPurchaseLand(List<Vector3Int> tilesToPurchase, Player activePlayer)
    {
        List<LandPlot> clearToBuy = new List<LandPlot>();
        float purchasePrice = 0;

        // Check if each tile is valid, and, if it is, add its value to the purchase price.
        foreach (Vector3Int tile in tilesToPurchase)
        {
            // Check if the tile is valid for purchase, and add it to clearToBuy if so.
            if (CheckIfTileCanBePurchased(tile))
            {
                clearToBuy.Add(worldMap.MapData[tile.x, tile.y]);
            }
        }
        // Get total purchase price
        purchasePrice = GetLandPurchasePrice(clearToBuy);
        // Check if player can afford to buy the land -- Adjust by economyManager.LandTransactionAdjustment
        // purchasePrice *= economyManager.LandTransactionAdjustment

        // If we make it this far, purchase the land!
        BuyTiles(clearToBuy, activePlayer);
        // Charge the player for the land
    }
   
    /// <summary>
    /// Attempt to sell tiles if valid.
    /// </summary>
    /// <param name="tilesToPurchase"></param>
    public void AttemptToSellLand(List<Vector3Int> tilesToSell, Player activePlayer)
    {
        List<LandPlot> clearToSell = new List<LandPlot>();
        float salePrice = 0;

        // Check if each tile is valid, and, if it is, add its value to the sell price.
        foreach (Vector3Int tile in tilesToSell)
        {
            // Check if the tile is valid for sale, and add it to clearToSell if so.
            if (CheckIfTileCanBeSold(tile))
            {
                clearToSell.Add(worldMap.MapData[tile.x, tile.y]);
            }
        }
        // Get total sale price
        salePrice = GetLandSalePrice(clearToSell);
        // Check if player can afford to sell the land -- Adjust by economyManager.LandTransactionAdjustment
        // sellPrice *= economyManager.LandTransactionAdjustment

        // If we make it this far, sell the land!
        SellTiles(clearToSell);
        // Credit the player for the land
    }

    public float GetLandPurchasePrice(List<Vector3Int> tilesToPurchase)
    {
        float price = 0;
        foreach(Vector3Int tilePos in tilesToPurchase)
        {
            price += Crops.Economy.EconomyManager.GetPurchasePrice(worldMap.MapData[tilePos.x, tilePos.y].CurrentLandValue);
        }
        return price;
    }

    public float GetLandPurchasePrice(List<LandPlot> tilesToPurchase)
    {
        float price = 0;
        foreach (LandPlot tile in tilesToPurchase)
        {
            price += Crops.Economy.EconomyManager.GetPurchasePrice(tile.CurrentLandValue);
        }
        return price;
    }

    public float GetLandSalePrice(List<Vector3Int> tilesToSell)
    {
        float price = 0;
        foreach (Vector3Int tilePos in tilesToSell)
        {
            price += Crops.Economy.EconomyManager.GetSalePrice(worldMap.MapData[tilePos.x, tilePos.y].CurrentLandValue);
        }
        return price;
    }

    public float GetLandSalePrice(List<LandPlot> tilesToSell)
    {
        float price = 0;
        foreach (LandPlot tile in tilesToSell)
        {
            price += Crops.Economy.EconomyManager.GetSalePrice(tile.CurrentLandValue);
        }
        return price;
    }
    void BuyTiles(List<LandPlot> tilesToPurchase, Player activePlayer)
    {
        foreach(LandPlot tile in tilesToPurchase)
        {
            BuyTile(tile, activePlayer);            
        }
    }

    void BuyTile(LandPlot tile, Player activePlayer)
    {
        tile.IsOwnedByPlayer = true;
        tile.OwnerID = activePlayer.ID;
        //worldMap.PlaceFence(tile.PlotCoordinates3D, "Fence_Main");
    }

    void SellTiles(List<LandPlot> tilesToSell)
    {
        foreach (LandPlot tile in tilesToSell)
        {
            SellTile(tile);
        }
    }

    void SellTile(LandPlot tile)
    {
        tile.IsOwnedByPlayer = false;
        tile.OwnerID = -1;
        worldMap.RemoveFence(tile.PlotCoordinates3D);
    }

    /// <summary>
    /// Returns true if tile can be purchased.
    /// </summary>
    /// <param name="tilePos"></param>
    /// <returns></returns>
    bool CheckIfTileCanBePurchased(Vector3Int tilePos)
    {
        if (worldMap.terrainTilemap.GetTile<TerrainTile>(tilePos))
        {
            LandPlot tileToCheck = worldMap.MapData[tilePos.x, tilePos.y];
            if(!tileToCheck.IsOwnedByPlayer && !gameManager.city.CheckIfOwnedByCity(tilePos))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if tile can be sold.
    /// </summary>
    /// <param name="tilePos"></param>
    /// <returns></returns>
    bool CheckIfTileCanBeSold(Vector3Int tilePos)
    {
        if (worldMap.terrainTilemap.GetTile<TerrainTile>(tilePos))
        {
            LandPlot tileToCheck = worldMap.MapData[tilePos.x, tilePos.y];
            if (tileToCheck.IsOwnedByPlayer)
            {
                return true;
            }
        }
        return false;
    }
}
