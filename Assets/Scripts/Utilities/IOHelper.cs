using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;


namespace Crops.Utilities
{

    /// <summary>
    /// Handles all IO functions.
    /// </summary>
    public static class IOHelper
    {

        static string myDocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        static string companyFolderPath = myDocumentsPath + "/Earring Pranks Studios";
        static string gameDataFolderPath = companyFolderPath + "/Crops";
        static string streamingAssetsPath = Application.dataPath + "/StreamingAssets";

        // Game Data Folder Paths

        public static string streamingAssetsSavesFolderPath = streamingAssetsPath + "/Saves";
        public static string streamingAssetsScenariosFolderPath = streamingAssetsPath + "/Scenarios";
        public static string streamingAssetsLocalizationFolderPath = streamingAssetsPath + "/Localization";

        // User Data Folder Paths
        public static string userDataModsFolderPath = gameDataFolderPath + "/Mods";
        public static string userDataSavesFolderPath = gameDataFolderPath + "/Saves";
        public static string userDataScenariosFolderPath = gameDataFolderPath + "/Scenarios";
        public static string userDataSettingsFolderPath = gameDataFolderPath + "/Settings";


        static string[] fileStructureFolders = new string[]
        {
                // Streaming Assets Folders
                streamingAssetsSavesFolderPath,
                streamingAssetsScenariosFolderPath,
                streamingAssetsLocalizationFolderPath,
                // User Data Folders
                userDataModsFolderPath,
                userDataSavesFolderPath,
                userDataScenariosFolderPath,
                userDataSettingsFolderPath,

        };

        /// <summary>
        /// Check for missing folders in file structure, and generate any missing entries. Run on game start.
        /// </summary>
        public static void CheckFileStructure()
        {
            foreach (string folder in fileStructureFolders)
            {
                if (!Directory.Exists(folder))
                {
                    GenerateFileStructure();
                    break;
                }
            }
        }

        /// <summary>
        /// Generate any missing folders in file structure (Ex: Saves, Scenarios).
        /// </summary>
        public static void GenerateFileStructure()
        {
            foreach (string folder in fileStructureFolders)
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

        }
        #region Game Config File

        /// <summary>
        /// Returns false if the game config file is not found.
        /// </summary>
        /// <returns></returns>
        public static bool CheckForConfigFile()
        {
            if(File.Exists(Application.persistentDataPath + $"/config.txt"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SaveConfigFile(GameConfig config)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(Application.persistentDataPath + $"/config.txt"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, config);
            }
        }

        public static GameConfig LoadConfigFile()
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader(Application.persistentDataPath + $"/config.txt"))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<GameConfig>(reader);
            }
        }

        #endregion
        #region SavedGameIO
        public static void SaveGameToDisk(GameData gameToSave, string saveName)
        {
            gameToSave.gameInfo.timeSaved = System.DateTime.UtcNow;
            JsonSerializer serializer = new JsonSerializer();

            using (StreamWriter sw = new StreamWriter(userDataSavesFolderPath + $"/{saveName}.farm"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, gameToSave);
            }
        }

        public static GameData LoadGameFromDisk(string saveName)
        {
            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader((userDataSavesFolderPath + $"/{saveName}.farm")))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<GameData>(reader);
            }
        }
        #endregion

        //#region ScenarioIO
        //public static void SaveScenarioToDisk(Scenario scenarioToSave, string scenarioName, string savePath)
        //{
        //    scenarioToSave = new Scenario();
        //    savePath += $"/{scenarioName}.scen";
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream file = File.Create(savePath);
        //    bf.Serialize(file, scenarioToSave);
        //    file.Close();
        //    Debug.Log($"Saved scenario to {savePath}.");

        //}

        //public static Scenario LoadScenarioFromDisk(string savePath)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream file = File.Open(savePath, FileMode.Open);
        //    Scenario loadedGame = (Scenario)bf.Deserialize(file);
        //    file.Close();
        //    return loadedGame;
        //}
        //#endregion
    }
}