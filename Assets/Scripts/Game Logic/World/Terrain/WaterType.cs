using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [CreateAssetMenu]
    public class WaterType : LocalizedObject
    {

        /// <summary>
        /// Full-tile water sprites.
        /// </summary>
        public Sprite[] fullSprites;

        /// <summary>
        /// Inner corner-tile water sprites (Water on all tiles save 1 corner (NE/SE/SW/NW).
        /// </summary>
        public Sprite[] innerCornerSprites;

        /// <summary>
        /// Water on all tiles save two opposed corners on the same side (NE/NW, NW/SW, SW/SE, SE/NE).
        /// </summary>
        public Sprite[] doubleInnerCornerSprites;

        /// <summary>
        /// Outer corner-tile water sprites (Water on all tiles save 2 adjacent sides (N/E, E/S, S/W, W/N).
        /// </summary>
        public Sprite[] outerCornerSprites;

        /// <summary>
        /// Edge-tile (land on 1 side) water sprites.
        /// </summary>
        public Sprite[] edgeSprites;

        /// <summary>
        /// Isthmus-tile (land on 2 opposing sides) water sprites.
        /// </summary>
        public Sprite[] isthmusSprites;

        /// <summary>
        /// Peninsula-tile (land on 3 sides) water sprites.
        /// </summary>
        public Sprite[] peninsulaSprites;

        /// <summary>
        /// Pond-tile (land on all 4 sides) water sprites.
        /// </summary>
        public Sprite[] pondSprites;

    }
}