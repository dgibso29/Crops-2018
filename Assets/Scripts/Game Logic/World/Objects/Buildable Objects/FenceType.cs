using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [CreateAssetMenu]
    public class FenceType : LocalizedObject, IBuildable
    {
        /// <summary>
        /// Cost to build this fence.
        /// </summary>
        public float BuildCost
        {
            get
            {
                return _buildCost;
            }
            set
            {
                _buildCost = value;
            }
        }

        [SerializeField]
        private float _buildCost;

        /// <summary>
        /// Cost to destroy this fence.
        /// </summary>
        public float DestructionCost
        {
            get
            {
                return _destructionCost;
            }
            set
            {
                _destructionCost = value;
            }
        }

        [SerializeField]
        private float _destructionCost;

        public Sprite NoAssignment;

        /// <summary>
        /// Full-tile fence sprites (land owned on all sides).
        /// </summary>
        public Sprite[] fullSprites;

        /// <summary>
        /// Inner corner-tile fence sprites (land owned on all tiles save 1 corner (NE/SE/SW/NW).
        /// </summary>
        public Sprite[] singleInnerCornerSprites;

        /// <summary>
        /// Land owned on all tiles save two corners on the same side (NE/NW, NW/SW, SW/SE, SE/NE).
        /// </summary>
        public Sprite[] doubleInnerCornerSprites;

        /// <summary>
        /// Land owned on all tiles save two corners on opposite sides.
        /// </summary>
        public Sprite[] doubleInnerCornerDiagonalSprites;

        /// <summary>
        /// Land owned on only one corner.
        /// </summary>
        public Sprite[] tripleInnerCornerSprites;

        /// <summary>
        /// No corner tiles owned.
        /// </summary>
        public Sprite[] quadrupleInnerCornerSprites;

        /// <summary>
        /// Outer corner-tile fence sprites (land owned on all tiles save 2 adjacent sides (N/E, E/S, S/W, W/N).
        /// </summary>
        public Sprite[] outerCornerSprites;

        /// <summary>
        /// Outer corner-tile fence sprites with the opposing corner unowned.
        /// </summary>
        public Sprite[] outerCornerWithInnerSprites;

        /// <summary>
        /// Edge-tile (land owned on 1 side) fence sprites.
        /// </summary>
        public Sprite[] edgeSprites;

        public Sprite[] edgeSingleInnerLeftSprites;

        public Sprite[] edgeSingleInnerRightSprites;

        public Sprite[] edgeDoubleInnerSprites;

        /// <summary>
        /// Isthmus-tile (land owned on 2 opposing sides) fence sprites.
        /// </summary>
        public Sprite[] isthmusSprites;

        /// <summary>
        /// Peninsula-tile (land owned on 3 sides) fence sprites.
        /// </summary>
        public Sprite[] peninsulaSprites;

        /// <summary>
        /// Pond-tile (land owned on all 4 sides) fence sprites.
        /// </summary>
        public Sprite[] pondSprites;

    }
}