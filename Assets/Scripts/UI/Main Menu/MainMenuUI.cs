using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.UI {
    public class MainMenuUI : MonoBehaviour
    {

        MasterManager masterManager;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update() {

        }

        /// Hey don't forget you wanted to see if you could do this with localisation in mind by:
        /// Referencing each button/UI element textbox and finding a way to load the proper translation from a text file,
        /// maybe by pain-stakingly enumerating every single text box in the game??
        /// DUNNO
        /// BUT
        /// DO
        /// NOT
        /// FORGET
        /// OKAY?

        private void Awake()
        {
            masterManager = MasterManager.instance;
        }

        public void StartNewGame()
        {
            masterManager.StartCoroutine(masterManager.StartNewGame());
        }

        public void LoadGame()
        {
            masterManager.StartCoroutine(masterManager.LoadGame());
        }
    }
}