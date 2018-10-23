using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Crops.World
{
    public abstract class CustomTile : Tile
    {

        /// <summary>
        /// Two-dimensional tile coordinates on the tilemap.
        /// </summary>
        public Vector3Int tileCoordinates3D;
        /// <summary>
        /// Two-dimensional tile coordinates on the tilemap.
        /// </summary>
        public Vector2Int tileCoordinates2D;

        /// <summary>
        /// Reference to the logical object this tile represents, if any. This reference should be used for instantiation only.
        /// </summary>
        public Object tileObject;

        public abstract void SetAssetReference(ScriptableObject asset);


        public void SetTileColor(Color color)
        {
            this.color = color;
        }

        /// <summary>
        /// Set tile coordinates.
        /// </summary>
        /// <param name="coordX"></param>
        /// <param name="coordY"></param>
        public void SetTileCoordinates(int coordX, int coordY)
        {
            tileCoordinates3D = new Vector3Int(coordX, coordY, 0);
            tileCoordinates2D = new Vector2Int(coordX, coordY);
        }

        /// <summary>
        /// Return a Matrix4X4 with the provided rotation. Use when setting tile rotation.
        /// </summary>
        /// <param name="newRotation"></param>
        /// <returns></returns>
        public Matrix4x4 GetNewMatrixWithRotation(Quaternion newRotation)
        {
            Matrix4x4 newMatrix = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 1f));
            newMatrix.SetTRS(Vector3.zero, newRotation, Vector3.one);

            return newMatrix;
        }
    }
}
