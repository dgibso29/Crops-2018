using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Crops.World;

public class EditorTools
{
#if UNITY_EDITOR
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/Mods";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    // The following is a helper that adds a menu item to create a RoadTile Asset
    [MenuItem("Assets/Create/RoadTile")]
    public static void CreateRoadTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save RoadTile", "New RoadTile", "Asset", "Save RoadTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RoadTile>(), path);
    }

    // The following is a helper that adds a menu item to create a TerrainTile Asset
    [MenuItem("Assets/Create/TerrainTile")]
    public static void CreateTerrainTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save TerrainTile", "New TerrainTile", "Asset", "Save TerrainTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TerrainTile>(), path);
    }

    // The following is a helper that adds a menu item to create a WaterTile Asset
    [MenuItem("Assets/Create/WaterTile")]
    public static void CreateWaterTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save WaterTile", "New WaterTile", "Asset", "Save WaterTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<WaterTile>(), path);
    }

    // The following is a helper that adds a menu item to create a BuildableObjectTile Asset
    [MenuItem("Assets/Create/BuildableObjectTile")]
    public static void CreateBuildableObjectTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save BuildableObjectTile", "New BuildableObjectTile", "Asset", "Save BuildableObjectTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BuildableObjectTile>(), path);
    }

    // The following is a helper that adds a menu item to create a FenceTile Asset
    [MenuItem("Assets/Create/FenceTile")]
    public static void CreateFenceTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save FenceTile", "New FenceTile", "Asset", "Save FenceTile", "Assets");
        if (path == "")
            return;
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FenceTile>(), path);
    }
#endif



}
