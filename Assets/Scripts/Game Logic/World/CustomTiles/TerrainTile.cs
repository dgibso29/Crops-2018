
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{

    public class TerrainTile : CustomTile
    {

        /// <summary>
        /// Holds all variants of this tile type. 0 index is default.
        /// </summary>
        public Sprite[] SpriteVariants => AssetReference.spriteVariants;

        public int[] SpriteVariantWeights => AssetReference.spriteVariantWeights;

        ///// <summary>
        ///// Reference to the asset from which this tile was created. Used to pull sprites.
        ///// </summary>
        //public new TerrainType AssetReference
        //{
        //    get { return base.AssetReference as TerrainType; }
        //    private set { AssetReference = value as TerrainType; }
        //}

        /// <summary>
        /// Reference to the asset from which this tile was created. Used to pull sprites.
        /// </summary>
        public TerrainType AssetReference
        {
            get;
            private set;
        }

        public override void SetAssetReference(ScriptableObject asset)
        {
            AssetReference = asset as TerrainType;
        }


        /// <summary>
        /// Randomly chooses tile sprite from available variants.
        /// </summary>
        public int GetRandomTileVariant()
        {
            MathHelper.IntRange[] weightedRange = new MathHelper.IntRange[SpriteVariants.Length];
            for(int i = 0; i < SpriteVariants.Length; i++)
            {
                weightedRange[i] = new MathHelper.IntRange(i, i, SpriteVariantWeights[i]);
            }
            int variantIndex = MathHelper.RandomRange.WeightedRange(weightedRange);
            SetTileVariant(variantIndex);
            return variantIndex;
        }

        public float GetRandomTileRotation()
        {
            // Randomly rotate tile
            MathHelper.IntRange[] rotationRange = new MathHelper.IntRange[4];
            rotationRange[0] = new MathHelper.IntRange(0, 0, 25);
            rotationRange[1] = new MathHelper.IntRange(90, 90, 25);
            rotationRange[2] = new MathHelper.IntRange(180, 180, 25);
            rotationRange[3] = new MathHelper.IntRange(270, 270, 25);

            float rotation = MathHelper.RandomRange.WeightedRange(rotationRange);
            SetTileRotation(rotation);
            return rotation;
        }

        public void SetTileVariant(int variantIndex)
        {
            sprite = SpriteVariants[variantIndex];

        }

        public void SetTileRotation(float newRotation)
        {
            transform = GetNewMatrixWithRotation(Quaternion.Euler(0f, 0f, newRotation));
        }

    }

    }