using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops
{
    /// <summary>
    /// JSON-Safe Vector2Int Data struct.
    /// </summary>
    public struct Vector2IntJSON
    {
        public int x;
        public int y;

        public Vector2 ToVector2()
        {
            return new Vector2(x,y);
        }
        
        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x,y);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, 0);
        }

        public Vector3Int ToVector3Int()
        {
            return new Vector3Int(x, y, 0);
        }

        public Vector2IntJSON(Vector3 vector3)
        {
            x = (int)vector3.x;
            y = (int)vector3.y;
        }
        
        public Vector2IntJSON(Vector2 vector2)
        {
            x = (int)vector2.x;
            y = (int)vector2.y;
        }

        public Vector2IntJSON(Vector2Int vector2Int)
        {
            x = vector2Int.x;
            y = vector2Int.y;
        }

        public Vector2IntJSON(Vector3Int vector3Int)
        {
            x = vector3Int.x;
            y = vector3Int.y;
        }

        public Vector2IntJSON(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2IntJSON(float x, float y)
        {
            this.x = (int)x;
            this.y = (int)y;
        }
    }
}
