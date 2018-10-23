using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ExtensionMethods
{
    /// <summary>
    /// Resets tile rotation to 0 ("Up").
    /// </summary>
    /// <param name="tile"></param>
    public static void ResetRotation(this Tile tile)
    {
        Matrix4x4 newMatrix = new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(0f, 0f, 0f, 1f));
        newMatrix.SetTRS(Vector3.zero, Quaternion.Euler(0, 0, 0), Vector3.one);
        tile.transform = newMatrix;
        Debug.Log("THIS WORKED");
    }

}
