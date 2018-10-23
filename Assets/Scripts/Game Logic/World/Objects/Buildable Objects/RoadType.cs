using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    [CreateAssetMenu]
    public class RoadType : LocalizedObject, IBuildable
    {


        /// <summary>
        /// Cost to build this road.
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
        /// Cost to destroy this road.
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

        /// <summary>
        /// Single tile (unconnected) road sprites.
        /// </summary>
        public Sprite[] singleSprites;

        /// <summary>
        /// Single connection road sprites.
        /// </summary>
        public Sprite[] deadEndSprites;

        /// <summary>
        /// Straight road (2 opposed connections) sprites.
        /// </summary>
        public Sprite[] straightSprites;

        /// <summary>
        /// Corner road (2 adjacent connections) sprites.
        /// </summary>
        public Sprite[] cornerSprites;

        /// <summary>
        /// T-Intersection (3 adjacent connections) sprites.
        /// </summary>
        public Sprite[] tJunctionSprites;

        /// <summary>
        /// 4-way intersection (4 connections) sprites.
        /// </summary>
        public Sprite[] fourWaySprites;

        /// <summary>
        /// Straight road (2 adjacent connections) gate sprites. Used when crossing fences/walls.
        /// </summary>
        public Sprite[] gateSprites;

        public Sprite[] straightCrosswalkSprites;

    }
}