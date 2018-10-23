using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.World;

namespace Crops.UI
{
    /// <summary>
    /// Handles all player input.
    /// </summary>
    public class InputManager : MonoBehaviour
    {

        public GameManager gameManager;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            if(gameManager == null && GameManager.instance != null)
            {
                gameManager = GameManager.instance;
            }

            #region Test Functions
            if (Input.GetMouseButtonDown(0))
            {
                gameManager.construction.AttemptObjectConstruction<Field>("Field", new Vector2Int(8, 8), gameManager.GetCurrentTileCoords(), 0, gameManager.GetPlayer(1));
            }

            if (Input.GetMouseButtonDown(1))
            {
                int objectID = gameManager.GetCurrentLandPlot().ObjectOnTileID;
                if (objectID != 0)
                {
                    Field obj = gameManager.worldMap.GetBuildableObjectWithID<Field>(objectID);
                    if(obj == null)
                    {
                        Debug.Log("Fuck");
                    }
                    obj.AdvanceToNextGrowthStage();
                }
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                gameManager.construction.AttemptObjectConstruction<BuildableObject>("Shed", new Vector2Int(5,3), gameManager.GetCurrentTileCoords(), 0, gameManager.GetPlayer(1));

            }


            if (Input.GetKeyDown(KeyCode.E))
            {
                gameManager.time.AddDay();

            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                Vector3Int currTile = gameManager.worldMap.currentTileCoordinates;
                List<Vector3Int> tiles = new List<Vector3Int>();
                for(int x = -25; x < 25; x++)
                {
                    for(int y = -25; y < 25; y++)
                    {
                        tiles.Add(new Vector3Int(currTile.x + x, currTile.y + y, 0));
                    }
                }
                gameManager.land.AttemptToPurchaseLand(tiles, gameManager.GetPlayer(1));
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                gameManager.weather.AdvanceWeatherOneWeek();
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                gameManager.SaveGame();
            }

            #endregion

            #region Camera Controls

            /// Pan Up
            if (Input.GetButton("Camera Up"))
            {
                gameManager.cameraController.PanUp();
            }
            /// Pan Down
            if (Input.GetButton("Camera Down"))
            {
                gameManager.cameraController.PanDown();
            }            
            /// Pan Right
            if (Input.GetButton("Camera Right"))
            {
                gameManager.cameraController.PanRight();

            }
            /// Pan Left
            if (Input.GetButton("Camera Left"))
            {
                gameManager.cameraController.PanLeft();
            }

            /// Zoom Out
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                gameManager.cameraController.ZoomOut();
            }
            /// Zoom In
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                gameManager.cameraController.ZoomIn();
            }
            /// Camera Panning (Intended to be scrollwheel / middle mouse button.
            if (Input.GetButtonDown("Pan Camera"))
            {
                gameManager.cameraController.StartCameraPan();
            }

            if (Input.GetButton("Pan Camera"))
            {
                gameManager.cameraController.PanCamera();
            }
            #endregion

            #region Time Controls

            ///// Pause/Unpause
            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    gameManager.time.TogglePause();
            //}


            #endregion
        }
    }
}
