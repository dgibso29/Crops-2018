using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops;

namespace Crops.UI
{
    public class CameraFilter : MonoBehaviour
    {

        public MeshRenderer meshRenderer;

        public Material brightnessMaterial;

        public float brightnessAdjustmentRate = .005f;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Set brightness to new value, where 1 = max and 0 = min.
        /// </summary>
        /// <param name="brighness"></param>
        public void SetNewBrightness(float brightness)
        {
            StartCoroutine(AdjustBrightnessGradually(brightness));
        }

        IEnumerator AdjustBrightnessGradually(float brightness)
        {
            while (brightnessMaterial.color.a != brightness)
            {
               float newAlpha = Mathf.MoveTowards(brightnessMaterial.color.a, brightness, brightnessAdjustmentRate);
                brightnessMaterial.color = new Color(0, 0, 0, newAlpha);
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }

    }
}
