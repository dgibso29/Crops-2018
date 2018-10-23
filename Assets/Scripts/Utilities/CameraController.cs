using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.UI
{
    public class CameraController : MonoBehaviour
    {
        Vector3 lastMousePosition;

        GameObject cameraTarget;

        /// <summary>
        /// Camera filter that is used to adjust brightness.
        /// </summary>
        public CameraFilter brightnessFilter;        

        //public Vector3 cameraStartingPosition;
        float CameraPanSpeed { get { return MasterManager.gameConfig.cameraPanSpeed; } }
        float MinCameraZoom { get { return MasterManager.gameConfig.minCameraZoom; } }
        float MaxCameraZoom { get { return MasterManager.gameConfig.maxCameraZoom; } }
        float ZoomSpeed { get { return MasterManager.gameConfig.zoomSpeed; } }
        float MouseDragSensitivity { get { return MasterManager.gameConfig.mouseDragSensitivity; } }


        // Use this for initialization
        void Start()
        {
            cameraTarget = gameObject;
        }

        private void FixedUpdate()
        {
            LockFiltersToCameraPosition();
        }

        public void PanUp()
        {
            transform.position += /*cameraTarget.*/transform.up.normalized * CameraPanSpeed;
        }

        public void PanDown()
        {
            transform.position -= /*cameraTarget.*/transform.up.normalized * CameraPanSpeed;
        }

        public void PanRight()
        {
            transform.position += /*cameraTarget.*/transform.right.normalized * CameraPanSpeed;
        }

        public void PanLeft()
        {
            transform.position -= /*cameraTarget.*/transform.right.normalized * CameraPanSpeed;
        }

        public void ZoomOut()
        {
            if (Camera.main.orthographicSize < MaxCameraZoom)
                Camera.main.orthographicSize += ZoomSpeed;
        }

        public void ZoomIn()
        {
            if (Camera.main.orthographicSize > MinCameraZoom)
                Camera.main.orthographicSize -= ZoomSpeed;
        }

        /// <summary>
        /// Must be once before initiating PanCamera.
        /// </summary>
        public void StartCameraPan()
        {
            lastMousePosition = Input.mousePosition;
        }

        public void PanCamera()
        {
            Vector3 mousePosChange = Input.mousePosition - lastMousePosition;
            /*cameraTarget.*/
            transform.Translate(-(mousePosChange.x * MouseDragSensitivity), -(mousePosChange.y * MouseDragSensitivity), 0);
            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// Sets all filters to camera position.
        /// </summary>
        void LockFiltersToCameraPosition()
        {
            brightnessFilter.transform.position = transform.position;
        }

    }
}



