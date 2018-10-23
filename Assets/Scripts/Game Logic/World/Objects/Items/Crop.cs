using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Crops.World
{
    /// <summary>
    /// Denotes type of field in which this crop is grown.
    /// </summary>
    public enum FieldType { Field, Paddy };

    /// <summary>
    /// Used to measure tolerance of crops to Pests, Weeds, and Fungi.
    /// </summary>
    public enum Tolerance { None, Low, Medium, High, Max };

    /// <summary>
    /// Used to measure crop field condition requirements (Water, Soil Fertility).
    /// </summary>
    public enum Requirement { None, Low, Medium, High, Max};

    /// <summary>
    /// Base class of all items, such as machinery, chemicals, and crops.
    /// </summary>
    [CreateAssetMenu]
    public class Crop : Item
    {

        /// <summary>
        /// Array of sprites representing growth stages, where index 0 is fallow and the last index is ready to harvest.
        /// </summary>
        public Sprite[] cropSprites;

        /// <summary>
        /// Max growth stage for this crop. Determined by crop sprites length (8 sprites = Max stage of 7).
        /// </summary>
        public int maxGrowthStage;

        /// <summary>
        /// Number of weeks required to grow this crop from planting to harvest.
        /// </summary>
        public int weeksToGrow;

        /// Ideal growth conditions/requirements

        /// <summary>
        /// Ideal water level for this crop.         
        /// </summary>
        public Requirement optimalWaterAmount;

        /// <summary>
        /// Tolerance of crop to suboptimal moisture levels. This determines how far moisture can be from optimal before it is considered suboptimal.
        /// </summary>
        public Tolerance moistureTolerance;

        /// <summary>
        /// Ideal soil fertility level for this crop.
        /// </summary>
        public Requirement optimalSoilFertility;

        /// <summary>
        /// Ideal temperature for this crop.
        /// </summary>
        public float optimalTemperature;

        /// <summary>
        /// Tolerance of crop to suboptimal temperatures. This determines how far temp can be from optimal before it is considered suboptimal.
        /// </summary>
        public Tolerance temperatureTolerance;

        /// <summary>
        /// Minimum weeks crop must be at optimal water levels to hit the lowest level of quality (50).
        /// </summary>
        public int minOptimalWaterWeeks;

        /// <summary>
        /// Minimum weeks crop must be at optimal temp levels to hit the lowest level of quality (50).
        /// </summary>
        public int minOptimalTempWeeks;

        /// <summary>
        /// Level of tolerance for pests.
        /// </summary>
        public Tolerance pestTolerance;

        /// <summary>
        /// Level of tolerance for fungi.
        /// </summary>
        public Tolerance fungiTolerance;

        /// <summary>
        /// Level of tolerance for weeds.
        /// </summary>
        public Tolerance weedTolerance;

        private void Awake()
        {
            maxGrowthStage = cropSprites.Length - 1;
        }

    }
}
