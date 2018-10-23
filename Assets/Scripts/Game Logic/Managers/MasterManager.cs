using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using Crops.Utilities;
using Crops.Localization;

namespace Crops
{
    /// <summary>
    /// Manager of all game functions. Intended to exist as part of the Master Scene.
    /// </summary>
    public class MasterManager : MonoBehaviour
    {
        public static MasterManager instance;
        public static GameConfig gameConfig;

        public GameObject worldMapPrefab;

        // Use this for initialization
        void Start()
        {
            InitialiseGame();
            StartMainMenu();

        }

        private void Awake()
        {
            instance = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Run all necessary startup functions.
        /// </summary>
        void InitialiseGame()
        {
            IOHelper.CheckFileStructure();
            InitialiseConfigFile();
            AssetBundleHelper.InitializeAssetBundles();
            LocalizationManager.instance.LoadLocalizedText(gameConfig.currentLanguage);
        }

        /// <summary>
        /// Check for the config file and generate a new one if needed, and then load & reference it.
        /// </summary>
        void InitialiseConfigFile()
        {
            if(IOHelper.CheckForConfigFile() == false)
            {
                IOHelper.SaveConfigFile(new GameConfig());
            }
            gameConfig = IOHelper.LoadConfigFile();
        }

        void StartMainMenu()
        {
            SceneManager.LoadScene("_MainMenu", LoadSceneMode.Additive);
        }

        public IEnumerator StartNewGame()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("_MainGame", LoadSceneMode.Additive);
            yield return new WaitUntil(() => asyncLoad.isDone);
            SceneManager.UnloadSceneAsync("_MainMenu");
            GameManager.instance.StartNewGame(new GameInfo());
            yield return null;

        }

        public IEnumerator LoadGame()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("_MainGame", LoadSceneMode.Additive);
            yield return new WaitUntil(() => asyncLoad.isDone);
            SceneManager.UnloadSceneAsync("_MainMenu");
            GameManager.instance.StartLoadedGame("testSave");
            yield return null;
        }


    }
}