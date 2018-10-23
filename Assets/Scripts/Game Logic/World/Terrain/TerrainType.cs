using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [CreateAssetMenu]
    public class TerrainType : LocalizedObject
    {

        /// <summary>
        /// Holds all variants of this tile type. 0 index is default.
        /// </summary>
        public Sprite[] spriteVariants;

        public int[] spriteVariantWeights;

    }
}