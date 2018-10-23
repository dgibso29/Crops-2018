using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.World
{
    /// <summary>
    /// Base class for dynamic tiles (ie, roads, fences, water; Any type that changes based on those adjacent to it).
    /// </summary>
    public abstract class DynamicTile : CustomTile
    {
        // The following determines which sprite to use based on the number of adjacent tiles of this type
        protected abstract Sprite GetSprite(byte mask);

        // The following determines which rotation to use based on the positions of adjacent tiles of this type
        protected abstract Quaternion GetRotation(byte mask);

    }
}

