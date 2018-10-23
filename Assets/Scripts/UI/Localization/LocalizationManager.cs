using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Crops.Localization
{
    public class LocalizationManager : MonoBehaviour
    {

        public static LocalizationManager instance;

        private Dictionary<string, string> localizedText;
        private bool isReady = false;
        private string missingTextString = "Localized text not found";

        // Use this for initialization
        void Awake()
        {
            instance = this;
        }

        public void LoadLocalizedText(string languageName)
        {
            localizedText = new Dictionary<string, string>();

            // Add main localization data.
            string filePath = Utilities.IOHelper.streamingAssetsLocalizationFolderPath + "/" + languageName + ".txt";            

            if (File.Exists(filePath))
            {
                string dataAsJson = File.ReadAllText(filePath);
                LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

                for (int i = 0; i < loadedData.items.Length; i++)
                {
                    localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
                }

                //Debug.Log("Main localization data loaded, dictionary contains: " + localizedText.Count + " entries");
            }
            else
            {
                Debug.LogError("Cannot find main localization file!");
            }

            // Add any mod localization data
            List<string> modFiles = new List<string>();

            foreach(string path in Directory.GetFiles(Utilities.IOHelper.streamingAssetsLocalizationFolderPath + "/"))
            {
                //Debug.Log($"Checking file {path}");
                if(path.Contains(languageName + ".txt"))
                {
                    continue;
                }
                if (path.Contains(languageName) && path.Contains(".txt") && !path.Contains(".meta"))
                {
                    string dataAsJson = File.ReadAllText(path);
                    LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);

                    for (int i = 0; i < loadedData.items.Length; i++)
                    {
                        // If the key already exists, overwrite it with the new value.
                        if (localizedText.ContainsKey(loadedData.items[i].key))
                        {
                            localizedText[loadedData.items[i].key] = loadedData.items[i].value;
                        }
                        // Otherwise, add the new entry.
                        else
                        localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
                    }

                    //Debug.Log($"{path} localization data loaded, dictionary now contains: " + localizedText.Count + " entries");
                }
            }


            isReady = true;
        }

        public string GetLocalizedValue(string key)
        {
            string result = missingTextString;
            if (localizedText.ContainsKey(key))
            {
                result = localizedText[key];
            }

            return result;

        }

        public bool GetIsReady()
        {
            return isReady;
        }

    }
}