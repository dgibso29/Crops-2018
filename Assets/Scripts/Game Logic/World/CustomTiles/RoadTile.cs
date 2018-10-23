using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.World
{
    public class RoadTile : DynamicTile
    {

        /// <summary>
        /// Single tile (unconnected) road sprites.
        /// </summary>
        /// 

        public Sprite[] SingleSprites => AssetReference.singleSprites;

        /// <summary>
        /// Single connection road sprites.
        /// </summary>
        public Sprite[] DeadEndSprites => AssetReference.deadEndSprites;

        /// <summary>
        /// Straight road (2 opposed connections) sprites.
        /// </summary>
        public Sprite[] StraightSprites => AssetReference.straightSprites;

        /// <summary>
        /// Corner road (2 adjacent connections) sprites.
        /// </summary>
        public Sprite[] CornerSprites => AssetReference.cornerSprites;

        /// <summary>
        /// T-Intersection (3 adjacent connections) sprites.
        /// </summary>
        public Sprite[] TJunctionSprites => AssetReference.tJunctionSprites;

        /// <summary>
        /// 4-way intersection (4 connections) sprites.
        /// </summary>
        public Sprite[] FourWaySprites => AssetReference.fourWaySprites;

        /// <summary>
        /// Straight road (2 adjacent connections) gate sprites. Used when crossing fences/walls.
        /// </summary>
        public Sprite[] GateSprites => AssetReference.gateSprites;

        public Sprite[] StraightCrosswalkSprites => AssetReference.straightCrosswalkSprites;

        public bool isCrosswalk = false;

        public bool isGate = false;

        /// <summary>
        /// Reference to the asset from which this tile was created. Used to pull sprites.
        /// </summary>
        public RoadType AssetReference
        {
            get;
            private set;
        }

        public override void SetAssetReference(ScriptableObject asset)
        {
            AssetReference = asset as RoadType;
        }


        // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
        public override void RefreshTile(Vector3Int location, ITilemap tilemap)
        {
            for (int yd = -1; yd <= 1; yd++)
                for (int xd = -1; xd <= 1; xd++)
                {
                    Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                    if (HasRoadTile(tilemap, position))
                        tilemap.RefreshTile(position);
                }
        }
        // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
        // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
        public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
        {
            int mask = HasRoadTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
            mask += HasRoadTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;
            mask += HasRoadTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;
            mask += HasRoadTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;

            Sprite newSprite = GetSprite((byte)mask);

            tileData.sprite = newSprite;
            tileData.color = Color.white;
            tileData.transform = GetNewMatrixWithRotation(GetRotation((byte)mask));
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = ColliderType.None;

        }

        // This determines if the Tile at the position is the same RoadTile.
        private bool HasRoadTile(ITilemap tilemap, Vector3Int position)
        {
            return tilemap.GetTile<RoadTile>(position);
        }
        // The following determines which sprite to use based on the number of adjacent RoadTiles
        protected override Sprite GetSprite(byte mask)
        {
            // Handle crosswalks
            if (isCrosswalk)
            {
                if (mask == 5 || mask == 10)
                {
                    return StraightCrosswalkSprites[Random.Range(0, StraightCrosswalkSprites.Length)];
                }
            }
            // Handle gates
            if (isGate)
            {
                if (mask == 5 || mask == 10)
                {
                    return GateSprites[Random.Range(0, GateSprites.Length)];
                }
            }
            switch (mask)
            {
                case 0: return SingleSprites[Random.Range(0, SingleSprites.Length)];
                case 3:
                case 6:
                case 9:
                case 12: return CornerSprites[Random.Range(0, CornerSprites.Length)];
                case 1:
                case 2:
                case 4:
                case 8: return DeadEndSprites[Random.Range(0, DeadEndSprites.Length)];
                case 5:
                case 10: return StraightSprites[Random.Range(0, StraightSprites.Length)];
                case 7:
                case 11:
                case 13:
                case 14: return TJunctionSprites[Random.Range(0, TJunctionSprites.Length)];
                case 15: return FourWaySprites[Random.Range(0, FourWaySprites.Length)];
            }
            return SingleSprites[Random.Range(0, SingleSprites.Length)];
        }
        // The following determines which rotation to use based on the positions of adjacent RoadTiles
        protected override Quaternion GetRotation(byte mask)
        {
            switch (mask)
            {
                case 9:
                case 10:
                case 7:
                case 8:
                    return Quaternion.Euler(0f, 0f, -90f);
                case 1:
                case 3:
                case 14:
                    return Quaternion.Euler(0f, 0f, -180f);
                case 6:
                case 13:
                case 2:
                    return Quaternion.Euler(0f, 0f, -270f);
            }
            return Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
