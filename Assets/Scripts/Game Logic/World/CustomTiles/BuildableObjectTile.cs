using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

using Crops.Utilities;

namespace Crops.World
{
    public class BuildableObjectTile : CustomTile
    {  
           
        /// <summary>
        /// ID of the object to which this tile belongs, if any.
        /// </summary>
        public int associatedObjectID = 0;

        /// <summary>
        /// Base textures of this object. Each index is one version.
        /// </summary>
        public Texture2D[] baseTextures;

        /// <summary>
        /// Pixels per inch for this texture. Defaults to 20.
        /// </summary>
        public int pixelsPerUnit = 20;

        /// <summary>
        /// Holds arrays of this tiles' sprites, where the two-dimensional array is the sprite position in the tile.
        /// Make sure to check against footprint size when calling!
        /// </summary>
        public Sprite[][,] tileSprites;

        public bool arrayInitialized = false;

        /// <summary>
        /// Reference to the asset from which this tile was created. Used to pull sprites.
        /// </summary>
        public BuildableObject AssetReference
        {
            get;
            private set;
        }

        public override void SetAssetReference(ScriptableObject asset)
        {
            AssetReference = asset as BuildableObject;
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);

            // Reset rotation to 0 ("up").
            transform = GetNewMatrixWithRotation(Quaternion.Euler(0, 0, 0));
        }


        public void InitTileSpritesArray()
        {
            if (!arrayInitialized)
            {
                tileSprites = new Sprite[baseTextures.Length][,];
                for(int i = 0; i < baseTextures.Length; i++)
                {
                    tileSprites[i] = SpriteHelper.GetSpriteAtlas(baseTextures[i], pixelsPerUnit);
                }
                arrayInitialized = true;
            }
        }

    }
}
