using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.Utilities
{
    public static class SpriteHelper
    {

        /// <summary>
        /// Returns a sprite Atlas cut from the given Texture2D. Sprites will be of the given size in pixels.
        /// </summary>
        /// <param name="baseTexture"></param>
        /// <param name="pixelsPerUnit">Size of sprites to be created, in pixels.</param>
        /// <returns></returns>
        public static Sprite[,] GetSpriteAtlas(Texture2D baseTexture, int pixelsPerUnit = 20)
        {
            Vector2Int atlasSize = new Vector2Int(baseTexture.width / pixelsPerUnit, baseTexture.height / pixelsPerUnit);
            Sprite[,] atlas = new Sprite[atlasSize.x, atlasSize.y];
            for (int x = 0; x < atlasSize.x; x++)
            {
                for (int y = 0; y < atlasSize.y; y++)
                {
                    atlas[x, y] = Sprite.Create(baseTexture, new Rect(x * pixelsPerUnit, y * pixelsPerUnit, pixelsPerUnit, pixelsPerUnit), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                }
            }

            return atlas;
        }

    }
}
