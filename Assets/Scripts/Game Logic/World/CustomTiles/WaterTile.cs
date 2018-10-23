using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.World
{

    public class WaterTile : DynamicTile
    {
        /// <summary>
        /// Full-tile water sprites.
        /// </summary>
        public Sprite[] FullSprites => AssetReference.fullSprites;

        /// <summary>
        /// Inner corner-tile water sprites (Water on all tiles save 1 corner (NE/SE/SW/NW).
        /// </summary>
        public Sprite[] InnerCornerSprites => AssetReference.innerCornerSprites;

        /// <summary>
        /// Water on all tiles save two opposed corners on the same side (NE/NW, NW/SW, SW/SE, SE/NE).
        /// </summary>
        public Sprite[] DoubleInnerCornerSprites => AssetReference.doubleInnerCornerSprites;

        /// <summary>
        /// Outer corner-tile water sprites (Water on all tiles save 2 adjacent sides (N/E, E/S, S/W, W/N).
        /// </summary>
        public Sprite[] OuterCornerSprites => AssetReference.outerCornerSprites;

        /// <summary>
        /// Edge-tile (land on 1 side) water sprites.
        /// </summary>
        public Sprite[] EdgeSprites => AssetReference.edgeSprites;

        /// <summary>
        /// Isthmus-tile (land on 2 opposing sides) water sprites.
        /// </summary>
        public Sprite[] IsthmusSprites => AssetReference.isthmusSprites;

        /// <summary>
        /// Peninsula-tile (land on 3 sides) water sprites.
        /// </summary>
        public Sprite[] PeninsulaSprites => AssetReference.peninsulaSprites;

        /// <summary>
        /// Pond-tile (land on all 4 sides) water sprites.
        /// </summary>
        public Sprite[] PondSprites => AssetReference.pondSprites;

        /// <summary>
        /// Is the tile part of a river?
        /// </summary>
        public bool isRiver = false;

        /// <summary>
        /// Is the tile part of a lake?
        /// </summary>
        public bool isLake = false;

        /// <summary>
        /// Reference to the asset from which this tile was created. Used to pull sprites.
        /// </summary>
        public WaterType AssetReference
        {
            get;
            private set;
        }

        public override void SetAssetReference(ScriptableObject asset)
        {
            AssetReference = asset as WaterType;
        }

        // Pretty sure this was me trying to figure things out and isn't needed.
        //public WaterTile(int test)
        //{

        //}

        // This refreshes itself and other WaterTiles that are orthogonally and diagonally adjacent
        public override void RefreshTile(Vector3Int location, ITilemap tilemap)
        {
            for (int yd = -1; yd <= 1; yd++)
                for (int xd = -1; xd <= 1; xd++)
                {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (HasWaterTile(tilemap, position))
                        tilemap.RefreshTile(position);
                }
        }
        // This determines which sprite is used based on the WaterTiles that are adjacent to it and rotates it to fit the other tiles.
        // As the rotation is determined by the WaterTile, the TileFlags.OverrideTransform is set for the tile.
        public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
        {
            // Add each 8 neighbor tiles clockwise, starting at N.
            int mask = HasWaterTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;   // N  = 1
            mask += HasWaterTile(tilemap, location + new Vector3Int(1, 1, 0)) ? 2 : 0;      // NE = 2
            mask += HasWaterTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 4 : 0;      // E  = 4
            mask += HasWaterTile(tilemap, location + new Vector3Int(1, -1, 0)) ? 8 : 0;     // SE = 8
            mask += HasWaterTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 16 : 0;    // S  = 16
            mask += HasWaterTile(tilemap, location + new Vector3Int(-1, -1, 0)) ? 32 : 0;   // SW = 32
            mask += HasWaterTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 64 : 0;    // W  = 64
            mask += HasWaterTile(tilemap, location + new Vector3Int(-1, 1, 0)) ? 128 : 0;   // NW = 128
            
            Sprite newSprite = GetSprite((byte)mask);
            //tileData.flags = 0;
            tileData.sprite = newSprite;

            tileData.transform = GetNewMatrixWithRotation(GetRotation((byte)mask));
            
            tileData.color = Color.white;
            tileData.flags = TileFlags.LockTransform;


        }
        // This determines if the Tile at the position is the same WaterTile.
        private bool HasWaterTile(ITilemap tilemap, Vector3Int position)
        {
            if(position.x < 0 || position.y < 0 || position.x > Map.StaticMapSize - 1 || position.y > Map.StaticMapSize - 1)
            {
                return true;
            }
            return tilemap.GetTile<WaterTile>(position);
        }
        // The following determines which sprite to use based on the number of adjacent WaterTiles
        protected override Sprite GetSprite(byte mask)
        {
            switch (mask)
            {
                case 0:
                case 170: return PondSprites[Random.Range(0, PondSprites.Length)];
                case 1: 
                case 4:
                case 6:
                case 16:
                case 14:
                case 131:
                case 224:
                case 56:
                case 96:
                case 64: return PeninsulaSprites[Random.Range(0, PeninsulaSprites.Length)];
                case 17: 
                case 68: return IsthmusSprites[Random.Range(0, IsthmusSprites.Length)];
                case 254:
                case 251:
                case 243:
                case 126:
                case 207:
                case 159:
                case 252:
                case 231:
                case 249:
                case 63:
                case 239:
                case 191:
                case 241:
                case 31:
                case 124:
                case 199: return EdgeSprites[Random.Range(0, EdgeSprites.Length)];
                case 225:
                case 30:
                case 15:
                case 240:
                case 143:
                case 227:
                case 62:
                case 248:
                case 5:
                case 20:
                case 80:
                case 135:
                case 195:
                case 60:
                case 120:
                case 193:
                case 28:
                case 65: return OuterCornerSprites[Random.Range(0, OuterCornerSprites.Length)];
                case 223:
                case 127:
                case 253:
                case 247: return InnerCornerSprites[Random.Range(0, InnerCornerSprites.Length)];
                case 95:
                case 125:
                case 215:
                case 245: return DoubleInnerCornerSprites[Random.Range(0, DoubleInnerCornerSprites.Length)];
                case 255: return FullSprites[Random.Range(0, FullSprites.Length)];
                    
            }
            return FullSprites[Random.Range(0, FullSprites.Length)];
        }
        // The following determines which rotation to use based on the positions of adjacent WaterTiles
        protected override Quaternion GetRotation(byte mask)
        {
            switch (mask)
            {
                // WEST
                case 6:
                case 95:
                case 64:
                case 14:
                case 63:
                case 159:
                case 191:
                case 143:
                case 15:
                case 223:
                case 135:
                case 31: return Quaternion.Euler(0f, 0f, 270f); // West

                // NORTH
                case 1:
                case 60:
                case 30:
                case 62:
                case 252:
                case 254:
                case 126:
                case 125:
                case 127:
                case 56:
                case 28:
                case 124: return Quaternion.Euler(0f, 0f, 180f); // North

                // EAST                
                case 4:
                case 96:
                case 120:
                case 224:
                case 243:
                case 249:
                case 248:
                case 240:
                case 245:
                case 251:
                case 253:
                case 241: return Quaternion.Euler(0f, 0f, 90f); // East
            }
            return Quaternion.Euler(0f, 0f, 0f);
        }      

    }
}
