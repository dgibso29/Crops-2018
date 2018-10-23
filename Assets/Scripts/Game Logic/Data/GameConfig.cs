using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops
{
    /// <summary>
    /// Data class holding all game configuration (Graphics, Audio, Etc)
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        #region Camera Settings

        public float cameraPanSpeed;
        public float minCameraZoom;
        public float maxCameraZoom;
        public float zoomSpeed;
        public float mouseDragSensitivity;

        #endregion

        /// <summary>
        /// Currently selected language for localization.
        /// </summary>
        public string currentLanguage = "en-US";

        /// <summary>
        /// If true, display temperature in celcius. If false, display in fahrenheit.
        /// </summary>
        public bool tempInCelcius = true;

        /// <summary>
        /// Creates a new GameConfig with default settings.
        /// </summary>
        public GameConfig()
        {
            cameraPanSpeed = 0.75f;
            minCameraZoom = 10;
            maxCameraZoom = 50;
            zoomSpeed = 1;
            mouseDragSensitivity = 0.1f;

            currentLanguage = "en-US";

            tempInCelcius = true;
        }
    }
}
