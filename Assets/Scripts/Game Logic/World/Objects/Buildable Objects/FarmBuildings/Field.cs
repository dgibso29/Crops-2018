using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops;
using Crops.Utilities;
using Crops.Events;

namespace Crops.World
{

    /// <summary>
    /// Defines weekly task for a field.
    /// </summary>
    public enum FieldTaskType { None, Plant, Harvest, Fertilize, Pesticide, Herbicide, Fungicide, Cut };

    /// <summary>
    /// Measures the level of a field condition.
    /// </summary>
    public enum ConditionLevel { None, Low, Medium, High, Max };

    #region Field Task

    /// <summary>
    /// Represents a field's weekly task, holding all necessary info for it.
    /// </summary>
    [System.Serializable]
    public struct FieldTask
    {
        /// <summary>
        /// Type of Field Task
        /// </summary>
        public FieldTaskType Type;
        /// <summary>
        /// Crop Dictionary key for the crop active during this task.
        /// </summary>
        public string ActiveCropKey;

        public FieldTask(FieldTaskType type, string cropKey)
        {
            Type = type;
            ActiveCropKey = cropKey;
        }

    }
    #endregion

    [CreateAssetMenu]
    public class Field : BuildableObject
    {

        FieldData data;

        /// <summary>
        /// Holds the sprites for each field type.
        /// </summary>
        [System.Serializable]
        public struct FieldSpriteArray
        {
            public FieldType Type;
            public Sprite Fallow;
            public Sprite Plowed;
        }

        /// <summary>
        /// Holds arrays of sprites for each type of field.
        /// </summary>
        [SerializeField]
        public FieldSpriteArray[] fieldSprites;

        #region Properties

        /// <summary>
        /// Footprint of the field.
        /// </summary>
        public Vector2Int FieldFootprint
        {
            get { return data.fieldFootprint.ToVector2Int(); }
            set { data.fieldFootprint = new Vector2IntJSON(value); }
        }

        /// <summary>
        /// Dictionary key of the crop being grown. When "Fallow", field will be fallow.
        /// </summary>
        public string CropStringKey
        {
            get { return data.cropStringKey; }
            set { data.cropStringKey = value; }
        }

        /// <summary>
        /// List of Field Tasks, where index 0 is the current week and the last index is exactly one year ahead.
        /// </summary>
        public List<FieldTask> FieldSchedule
        {
            get { return data.fieldSchedule; }
            set { data.fieldSchedule = value; }
        }

        /// <summary>
        /// Growth stage of the current crop. Always 0 when fallow.
        /// </summary>
        public int GrowthStage
        {
            get { return data.growthStage; }
            set { data.growthStage = value; }
        }

        /// <summary>
        /// Weeks crop has been growing. Week of planting = week 1.
        /// </summary>
        public int GrowthWeeks
        {
            get { return data.growthWeeks; }
            set { data.growthWeeks = value; }
        }

        /// <summary>
        /// Quality of the current crop. Always 0 when fallow.
        /// </summary>
        public float CropQuality
        {
            get { return data.cropQuality; }
            set { data.cropQuality = value; }
        }

        /// <summary>
        /// Moisture level of the field, out of 100%.
        /// </summary>
        public ConditionLevel MoistureLevel
        {
            get { return data.moistureLevel; }
            set { data.moistureLevel = value; }
        }

        /// <summary>
        /// Soil fertility level of the field, out of 100%.
        /// </summary>
        public ConditionLevel SoilFertility
        {
            get { return data.soilFertility; }
            set { data.soilFertility = value; }
        }

        /// <summary>
        /// Pest level of the field, out of 100%.
        /// </summary>
        public ConditionLevel PestLevel
        {
            get { return data.pestLevel; }
            set { data.pestLevel = value; }
        }

        /// <summary>
        /// Fungus level of the field, out of 100%.
        /// </summary>
        public ConditionLevel FungusLevel
        {
            get { return data.fungusLevel; }
            set { data.fungusLevel = value; }
        }

        /// <summary>
        /// Weed level of the field, out of 100%.
        /// </summary>
        public ConditionLevel WeedLevel
        {
            get { return data.weedLevel; }
            set { data.weedLevel = value; }
        }

        /// <summary>
        /// Weeks current crop has spent at its optimal moisture level.
        /// </summary>
        public int WeeksAtOptimalMoisture
        {
            get { return data.weeksAtOptimalMoisture; }
            set { data.weeksAtOptimalMoisture = value; }
        }

        /// <summary>
        /// Weeks current crop has spent at or above its optimal fertility level.
        /// </summary>
        public int WeeksAtOptimalFertility
        {
            get { return data.weeksAtOptimalFertility; }
            set { data.weeksAtOptimalFertility = value; }
        }

        /// <summary>
        /// Weeks current crop has spent at its optimal temperature level.
        /// </summary>
        public int WeeksAtOptimalTemperature
        {
            get { return data.weeksAtOptimalTemperature; }
            set { data.weeksAtOptimalTemperature = value; }
        }

        /// <summary>
        /// Weeks current crop has spent above its pest endurance.
        /// </summary>
        public int WeeksOfPestDamage
        {
            get { return data.weeksOfPestDamage; }
            set { data.weeksOfPestDamage = value; }
        }

        /// <summary>
        /// Weeks current crop has spent above its fungus endurance.
        /// </summary>
        public int WeeksOfFungusDamage
        {
            get { return data.weeksOfFungusDamage; }
            set { data.weeksOfFungusDamage = value; }
        }

        /// <summary>
        /// Weeks current crop has spent above its weed endurance.
        /// </summary>
        public int WeeksOfWeedDamage
        {
            get { return data.weeksOfWeedDamage; }
            set { data.weeksOfWeedDamage = value; }
        }


        /// <summary>
        /// True if field has active irrigation.
        /// </summary>
        public bool Irrigated
        {
            get { return data.irrigated; }
            set { data.irrigated = value; }
        }

        /// <summary>
        /// True if field has active drainage.
        /// </summary>
        public bool Drained
        {
            get { return data.drained; }
            set { data.drained = value; }
        }



        #endregion

        /// <summary>
        /// Currently active crop.
        /// </summary>
        Crop CurrentCrop;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        #region Field Management Functions

        // Check if crop is ready to advance to next growth stage.
        void EvaluateGrowthStage()
        {
            float currentGrowthPercentage = (float)GrowthWeeks / (float)CurrentCrop.weeksToGrow;
            // Used to cap growth stage if crop is low quality. Defaults to max stage.
            int maxLowQualityStage = CurrentCrop.maxGrowthStage;
            Debug.Log(currentGrowthPercentage);
            // Determine percentage breakpoint between initial crop stage (after fallow/plow) and harvest stage.
            float growthStageBreakpoint = ((float)CurrentCrop.weeksToGrow / (CurrentCrop.maxGrowthStage - 1)) / (float)CurrentCrop.weeksToGrow;

            // If Crop Quality is below 50, cap growth stage based on quality.
            if(CropQuality < 50)
            {
                // Find halfway point stage.
                maxLowQualityStage = CurrentCrop.maxGrowthStage / 2;
                // Find quality percentage of 50
                float qualityPercentage = CropQuality / 50;
                // Find that percent of low quality stage
                maxLowQualityStage = (int)(maxLowQualityStage * qualityPercentage);
                Debug.Log($"Max low quality stage: {maxLowQualityStage}");
            }

            Debug.Log(growthStageBreakpoint);
            Debug.Log(GrowthStage);
            Debug.Log(currentGrowthPercentage / growthStageBreakpoint);
            // Advance growth stage if the current stage is below the max low quality stage,
            // and if the current stage is below the stage indicated by the growth percentage.
            if (GrowthStage < maxLowQualityStage && GrowthStage < currentGrowthPercentage / growthStageBreakpoint)
            {
                AdvanceToNextGrowthStage();
            }

        }

        #region Field Conditions

        void ReportFieldConditions()
        {
            Debug.Log($"Field {ID}. Growth Stage: {GrowthStage}. Growth Weeks: {GrowthWeeks}. Quality: {CropQuality}. Mois: {MoistureLevel}. Fert: {SoilFertility}. Pest: {PestLevel}. Weed: {WeedLevel}. Fungus: {FungusLevel}. Water Weeks: {WeeksAtOptimalMoisture}. Temp Weeks: {WeeksAtOptimalTemperature}. Damage Weeks P/W/F: {WeeksOfPestDamage},{WeeksOfWeedDamage},{WeeksOfFungusDamage}. Soil Weeks: {WeeksAtOptimalFertility}.");
        }

        /// <summary>
        /// Sets field conditions for the current week
        /// </summary>
        void SetFieldConditions()
        {
            // Moisture
            EvaluateFieldMoisture();

            // Pests
            EvaluateFieldPests();

            // Weeds
            EvaluateFieldWeeds();

            // Fungi
            EvaluateFieldFungi();
        }

        /// <summary>
        /// Sets field moisture according to previous moisture & current precipitation.
        /// </summary>
        void EvaluateFieldMoisture()
        {
            // Get expected moisture level based on current conditions
            ConditionLevel expectedLevel = GetMoistureLevelFromPrecipitation(GameManager.instance.weather.GetCurrentWeatherCategory(), GameManager.instance.weather.GetCurrentTempInC());

            // Adjust moisture level based on relation of current level to expected level.
            // Adjustment will always be in increments of 1.

            // If expected is greater than current, increase level by 1.
            if(expectedLevel > MoistureLevel)
            {
                MoistureLevel++;
            }
            // If expected is less than current, decrease level by 1.
            else if(expectedLevel < MoistureLevel)
            {
                MoistureLevel--;
            }
            // Otherwise, levels are the same, and we do nothing.

        }

        /// <summary>
        /// Returns moisture level that the given weather category & temperature would cause, all other factors removed.
        /// </summary>
        /// <param name="precipitation"></param>
        /// <returns></returns>
        ConditionLevel GetMoistureLevelFromPrecipitation(WeatherCategory weatherCategory, float temperature)
        {
            // Declare current level & default to medium.
            ConditionLevel currentLevel = ConditionLevel.Medium;
            // Find level based on weatherCategory
            switch (weatherCategory)
            {
                case WeatherCategory.Sunny:
                case WeatherCategory.PartlyCloudy:
                case WeatherCategory.Overcast:
                    {
                        currentLevel = ConditionLevel.None;
                        break;
                    }
                case WeatherCategory.LightFreezingPrecipitation:
                case WeatherCategory.LightPrecipitation:
                    {
                        currentLevel = ConditionLevel.Low;
                        break;
                    }
                case WeatherCategory.MinorStorm:
                case WeatherCategory.MinorWinterStorm:
                    {
                        currentLevel = ConditionLevel.Medium;
                        break;
                    }
                case WeatherCategory.HeavyFreezingPrecipitation:
                case WeatherCategory.HeavyPrecipitation:
                    {
                        currentLevel = ConditionLevel.High;
                        break;
                    }
                case WeatherCategory.MajorStorm:
                case WeatherCategory.MajorWinterStorm:
                    {
                        currentLevel = ConditionLevel.Max;
                        break;
                    }
            }

            // Modify this if temperatures are excessively high (40C or above), to simulate drought effects in a basic sense.
            if(temperature >= 40)
            {
                // Reduce whatever the level is by 1.
                if(currentLevel > 0)
                {
                    currentLevel--;
                }
            }
            return currentLevel;
        }

        /// <summary>
        /// Sets field moisture level to given level.
        /// </summary>
        /// <param name="newLevel"></param>
        void SetMoistureLevel(ConditionLevel newLevel)
        {
            MoistureLevel = newLevel;
        }

        /// <summary>
        /// Sets field fertility to given level.
        /// </summary>
        /// <param name="newLevel"></param>
        void SetFieldFertility(ConditionLevel newLevel)
        {
            SoilFertility = newLevel;
        }

        /// <summary>
        /// Sets field pest level to given level.
        /// </summary>
        /// <param name="newLevel"></param>
        void SetPestLevel(ConditionLevel newLevel)
        {
            PestLevel = newLevel;
        }

        /// <summary>
        /// Sets field weed level to given level.
        /// </summary>
        /// <param name="newLevel"></param>
        void SetWeedLevel(ConditionLevel newLevel)
        {
            WeedLevel = newLevel;
        }

        /// <summary>
        /// Sets field fungus level to given level.
        /// </summary>
        /// <param name="newLevel"></param>
        void SetFungusLevel(ConditionLevel newLevel)
        {
            FungusLevel = newLevel;
        }


        /// <summary>
        /// Sets field pest level according to previous level and current conditions.
        /// </summary>
        void EvaluateFieldPests()
        {
            if (WeatherManager.GlobalPestLevel > PestLevel)
            {
                PestLevel++;
            }
            else if (WeatherManager.GlobalPestLevel < PestLevel)
            {
                PestLevel++;
            }
        }

        /// <summary>
        /// Sets field weed level according to previous level and current conditions.
        /// </summary>
        void EvaluateFieldWeeds()
        {
            if (WeatherManager.GlobalWeedLevel > WeedLevel)
            {
                WeedLevel++;
            }
            else if (WeatherManager.GlobalWeedLevel < WeedLevel)
            {
                WeedLevel++;
            }
        }

        /// <summary>
        /// Sets field fungus level according to previous level and current conditions.
        /// </summary>
        void EvaluateFieldFungi()
        {
            if (WeatherManager.GlobalFungusLevel > FungusLevel)
            {
                FungusLevel++;
            }
            else if (WeatherManager.GlobalFungusLevel < FungusLevel)
            {
                FungusLevel++;
            }
        }

        /// <summary>
        /// Evaluates impact of field conditions on crop quality.
        /// </summary>
        void EvaluateFieldConditionImpact()
        {
            // Temperature
            if (CheckIfOptimalTemperature())
            {
                WeeksAtOptimalTemperature++;
            }
            // Moisture
            if (CheckIfOptimalMoisture())
            {
                WeeksAtOptimalMoisture++;
            }
            // Fertility
            if (CheckIfOptimalSoilFertility())
            {
                WeeksAtOptimalFertility++;
            }
            // Pests
            if (CheckForPestDamage())
            {
                WeeksOfPestDamage++;
            }
            // Weeds
            if (CheckForWeedDamage())
            {
                WeeksOfWeedDamage++;
            }            // Fungi
            if (CheckForFungusDamage())
            {
                WeeksOfFungusDamage++;
            }
        }

        /// <summary>
        /// Returns true if current temperature is within tolerance for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckIfOptimalTemperature()
        {
            float currentTemp = GameManager.instance.weather.GetCurrentTempInC();
            float optimalTemp = CurrentCrop.optimalTemperature;
            // For each tolerance level, check if the current temp is within a certain range of optimal, and return false if outside of that range.
            switch (CurrentCrop.temperatureTolerance)
            {
                case Tolerance.None:
                    {
                        if(Mathf.Abs(optimalTemp - currentTemp) > 2)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.Low:
                    {
                        if (Mathf.Abs(optimalTemp - currentTemp) > 4)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.Medium:
                    {
                        if (Mathf.Abs(optimalTemp - currentTemp) > 8)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.High:
                    {
                        if (Mathf.Abs(optimalTemp - currentTemp) > 10)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.Max:
                    {
                        if (Mathf.Abs(optimalTemp - currentTemp) > 15)
                        {
                            return false;
                        }
                        return true;
                    }
                default:
                    {
                        return true;
                    }
            }
        }

        /// <summary>
        /// Returns true if current moisture is within tolerance for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckIfOptimalMoisture()
        {

            switch (CurrentCrop.moistureTolerance)
            {
                case Tolerance.None:
                    {
                        if((int)MoistureLevel == (int)CurrentCrop.optimalWaterAmount)
                        {
                            return true;
                        }
                        return false;
                    }
                case Tolerance.Low:
                    {
                        if (Mathf.Abs((int)MoistureLevel - (int)CurrentCrop.optimalWaterAmount) > 1)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.Medium:
                    {
                        if (Mathf.Abs((int)MoistureLevel - (int)CurrentCrop.optimalWaterAmount) > 2)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.High:
                    {
                        if (Mathf.Abs((int)MoistureLevel - (int)CurrentCrop.optimalWaterAmount) > 3)
                        {
                            return false;
                        }
                        return true;
                    }
                case Tolerance.Max:
                    {
                        if (Mathf.Abs((int)MoistureLevel - (int)CurrentCrop.optimalWaterAmount) > 4)
                        {
                            return false;
                        }
                        return true;
                    }
                default:
                    {
                        return true;
                    }
            }
        }
        /// <summary>
        /// Returns true if current fertility is within tolerance for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckIfOptimalSoilFertility()
        {
            if ((int)SoilFertility >= (int)CurrentCrop.optimalSoilFertility)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if pest level will cause damage for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckForPestDamage()
        {
            if ((int)PestLevel > (int)CurrentCrop.pestTolerance)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if weed level will cause damage for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckForWeedDamage()
        {
            if ((int)WeedLevel > (int)CurrentCrop.weedTolerance)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if fungus level will cause damage for crop.
        /// </summary>
        /// <returns></returns>
        bool CheckForFungusDamage()
        {
            if ((int)FungusLevel > (int)CurrentCrop.fungiTolerance)
            {
                return true;
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Advances field to next growth stage, if applicable. Will trigger harvest task if relevant.
        /// </summary>
        public void AdvanceToNextGrowthStage()
        {
            SetCurrentCrop("Corn");

            // If the growth stage is less than final, advance to the next stage.
            if(GrowthStage < CurrentCrop.maxGrowthStage)
            {
                SetGrowthStage(GrowthStage + 1);
            }
            // If the stage is now final, we are ready to harvest.
            else if(GrowthStage == CurrentCrop.maxGrowthStage)
            {
                // Handle harvest -- Create harvest task if it does not already exist.

                // Temp -- set to fallow
                SetGrowthStage(0);
            }
        }

        /// <summary>
        /// Sets field to given growth stage for current crop. 0 is fallow.
        /// </summary>
        /// <param name="growthStageIndex"></param>
        public void SetGrowthStage(int growthStageIndex)
        {
            GrowthStage = growthStageIndex;
            SetFieldSprite(CurrentCrop.cropSprites[growthStageIndex]);
        }

        public void SetCurrentCrop(string cropDictionaryKey)
        {
            CropStringKey = cropDictionaryKey;
            CurrentCrop = Tileset.CropDictionary[CropStringKey] as Crop;

            // This should happen when built & when a new crop task is scheduled.
        }

        /// <summary>
        /// Updates crop quality based on field conditions to date.
        /// </summary>
        void EvaluateCropQuality()
        {
            /// Crop quality is evaluated on a weekly basis using to-date data, and is not an aggregate value.           
            /// Crop quality is measured on a scale of 0 - 100, and represented in-game by a qualitative scale of:
            /// Awful (0-49) - Bad (50-59) - Poor (60-69) - Average (70-79) - Good (80-89) - Excellent (90-95) - Superb (96-100)
            /// A quality 100 crop will have 95% optimal temp & moisture weeks, and no more than 10% of total growth weeks of combined damage.
            /// Base crop quality is determined equally by optimal moisture and optimal temperature weeks (50/50 split).
            /// Quality is evaluated in three stages:
            /// Minimum Optimal Weeks: Crop will meet 50 quality if it has the minimum optimal temperature and moisture weeks.        
            /// Quality cannot reach 50 if either minimum is not met. If quality is not 50, evaluation completes here.
            /// If Quality is 50, evaluation proceeds to the second stage:
            /// Quality is added based on number of optimal weeks relative to 95% of total growth weeks, with minimum weeks subtracted.
            /// Thus, Quality added from optimal water weeks = ((TotalOptimalWeeks - MinWaterWeeks) / ((TotalGrowthWeeks *95) - MinWaterWeeks)) * 25.
            /// Once these values are added, percentage modifiers are applied in the third stage based on the following rules:
            /// Each week of below-optimal soil fertility over 10% of total growth weeks will reduce crop quality by 2.5%.
            /// Each week of damage over 10% of total growth weeks will reduce crop quality by 2.5%. Multiple damage types stack (1 week of each damage type = 7.5% reduction).
            /// Each week of optimal soil fertility over 90% of total weeks will increase quality by 2.5%.
            /// All percentage modifiers will be added together before being applied to final quality.

            // Declare quality & set to 0.
            float quality = 0;

            // Find total growth weeks minus 5%.
            int AdjustedTotalGrowthWeeks = Mathf.RoundToInt(CurrentCrop.weeksToGrow * .95f);

            // Solve for percentage of to-date temp weeks of required temp weeks.
            float optimalToDateTempPercentage = ((float)WeeksAtOptimalTemperature / CurrentCrop.minOptimalTempWeeks);
            // Solve for percentage of to-date moisture weeks of required moisture weeks.
            float optimalToDateMoisturePercentage = ((float)WeeksAtOptimalMoisture / CurrentCrop.minOptimalWaterWeeks);

            // Set quality to equal the sum of each percentage to date out of 25.
            quality = (optimalToDateTempPercentage * 25) + (optimalToDateMoisturePercentage * 25);

            // If quality is less than 50, we're done -- the crop has not yet reached minimum growth.
            if(quality < 50)
            {
                CropQuality = quality;
            }
            // Otherwise, the crop has reached minimum growth, and we can consider other factors. 
            else
            {
                // Solve for additional quality from optimal water weeks.
                int additionalWaterQuality = ((WeeksAtOptimalMoisture - CurrentCrop.minOptimalWaterWeeks) / (AdjustedTotalGrowthWeeks - CurrentCrop.minOptimalWaterWeeks)) * 25;
                // Solve for additional quality from optimal temp weeks.
                int additionalTempQuality = ((WeeksAtOptimalTemperature - CurrentCrop.minOptimalTempWeeks) / (AdjustedTotalGrowthWeeks - CurrentCrop.minOptimalTempWeeks)) * 25;
                // Add additional quality scores to quality.
                quality += additionalTempQuality + additionalWaterQuality;

                // Solve for quality percentage modifiers
                // Declare quality percentage modifier & set to 0.
                float qualityPercentageModifier = 0;

                // Solve for 10% of growth weeks
                int tenPercentOfGrowthWeeks = Mathf.RoundToInt(CurrentCrop.weeksToGrow * .10f);

                /// Solve for # of weeks at sub-optimal soil fertility under 90% of total growth weeks.
                float poorFertilityModifier = 0;
                // If the number of weeks at optimal fertility is less than 90% of total growth weeks, continue.
                if (CurrentCrop.weeksToGrow - WeeksAtOptimalFertility > tenPercentOfGrowthWeeks)
                {
                    poorFertilityModifier = -2.5f * (CurrentCrop.weeksToGrow - WeeksAtOptimalFertility - tenPercentOfGrowthWeeks);
                }

                /// Solve for # of damage weeks over 10% of total growth weeks.
                float damageModifier = 0;
                // Find total weeks of damage.
                int combinedWeeksOfDamage = WeeksOfFungusDamage + WeeksOfPestDamage + WeeksOfWeedDamage;
                if (combinedWeeksOfDamage > tenPercentOfGrowthWeeks)
                {
                    damageModifier = -2.5f * (combinedWeeksOfDamage - tenPercentOfGrowthWeeks);
                }

                /// Solve for # of weeks at optimal soil fertility over 90% of total growth weeks.
                float greatFertilityModifier = 0;
                // If the number of weeks at optimal fertility is less than 90% of total growth weeks, continue.
                if (CurrentCrop.weeksToGrow - WeeksAtOptimalFertility < tenPercentOfGrowthWeeks)
                {
                    greatFertilityModifier = 2.5f * (CurrentCrop.weeksToGrow - WeeksAtOptimalFertility);
                }

                /// Solve for # of weeks crop is past required growth weeks, minus 1 grace week.
                float unharvestedModifier = 0;
                if(GrowthWeeks > CurrentCrop.weeksToGrow + 1)
                {
                    unharvestedModifier = -2.5f * ((GrowthWeeks - 1) - CurrentCrop.weeksToGrow);
                }

                // Sum of all percentage modifiers
                qualityPercentageModifier = poorFertilityModifier + damageModifier + greatFertilityModifier + unharvestedModifier;
                // Reduce modifiers to percentage
                qualityPercentageModifier /= 100;
                // Apply find quality differential
                float qualityDifferential = quality * qualityPercentageModifier;
                // Apply differential to quality.
                quality += qualityDifferential;
                // Clamp quality between 50 and 100.
                quality = Mathf.Clamp(quality, 50, 100);
                // Set quality & return.
                CropQuality = quality;

            }
            Debug.Log($"Crop quality is {quality}.");

        }

        #endregion

        #region Field Schedule Functions

        /// <summary>
        /// Creates default, empty schedule.
        /// </summary>
        void CreateBlankSchedule()
        {
            for(int i = 0; i < 53; i++)
            {
                FieldSchedule.Add(new FieldTask(FieldTaskType.None, "Fallow"));
            }

            // If crop type is not fallow, set plant task for next week
            if (CropStringKey != "Fallow")
            {
                SetFieldTask(new FieldTask(FieldTaskType.Plant, CropStringKey), 25); 
                CropStringKey = "Fallow";
            }

        }
         
        /// <summary>
        /// Set field task to given task at given week. Will fill out schedule as necessary.
        /// </summary>
        /// <param name="taskToAdd"></param>
        /// <param name="weekToSetTask"></param>
        public void SetFieldTask(FieldTask taskToAdd, int weekToSetTask)
        {
            switch (taskToAdd.Type)
            {
                // If none, do nothing! Wooh!
                case FieldTaskType.None:
                    {
                        SetScheduledTaskForWeek(taskToAdd, weekToSetTask);
                        break;
                    }
                case FieldTaskType.Plant:
                    {
                        // Set plant task for next week
                        SetScheduledTaskForWeek(taskToAdd, weekToSetTask);
                        // Set harvest task for proper future week.
                        SetScheduledTaskForWeek(new FieldTask(FieldTaskType.Harvest, taskToAdd.ActiveCropKey), (Tileset.CropDictionary[taskToAdd.ActiveCropKey] as Crop).weeksToGrow + weekToSetTask);

                        break;
                    }
                case FieldTaskType.Harvest:
                    {
                        break;
                    }
                case FieldTaskType.Fertilize:
                    {
                        break;
                    }
                case FieldTaskType.Pesticide:
                    {
                        break;
                    }
                case FieldTaskType.Herbicide:
                    {
                        break;
                    }
                case FieldTaskType.Fungicide:
                    {
                        break;
                    } 
                case FieldTaskType.Cut:
                    {
                        break;
                    }
            }
        }


        /// <summary>
        /// Sets the task for the given week.
        /// </summary>
        /// <param name="taskToAdd"></param>
        /// <param name="weekToSetTask"></param>
        void SetScheduledTaskForWeek(FieldTask taskToAdd, int weekToSetTask)
        {
            FieldSchedule[weekToSetTask] = taskToAdd;
        }

        /// <summary>
        /// Advances field schedule by one week.
        /// </summary>
        public void AdvanceScheduleOneWeek()
        {
            // Add the current task to the end of the schedule
            FieldSchedule.Add(FieldSchedule[0]);
            // Remove the current task from the schedule.
            FieldSchedule.RemoveAt(0);

            // Advance crop growth if field is not fallow.
            if (CropStringKey != "Fallow")
            {
                GrowthWeeks++;
                SetFieldConditions();
                EvaluateFieldConditionImpact();
                EvaluateCropQuality();
                EvaluateGrowthStage();
                ReportFieldConditions();
            }


            // Handle new current task.
            HandleCurrentTask();
        }
        /// <summary>
        /// Handles the current week's task as needed. Will create Agent jobs as needed, etc.
        /// </summary>
        void HandleCurrentTask()
        {
            /// TEMP
            /// AUTO-HANDLE PLANTING        
            if(FieldSchedule[0].Type == FieldTaskType.Plant)
            {
                SetCurrentCrop(FieldSchedule[0].ActiveCropKey);
                SetGrowthStage(1);
            }
        }
        #endregion

        #region Field Events

        void InitializeListeners()
        {
            InitializeTimeManagerListeners();
        }

        void InitializeTimeManagerListeners()
        {
            TimeManager.TimeEventOccurred += new TimeManager.TimeEventHandler(HandleTimeEvent);
        }


        void HandleTimeEvent(object sender, TimeEventArgs args)
        {
            // Handle event based on TimeEventType
            switch (args.type)
            {
                case TimeEventType.NewDay:
                    {
                        break;
                    }
                case TimeEventType.NewWeek:
                    {
                        AdvanceScheduleOneWeek();
                        break;
                    }
                case TimeEventType.NewMonth:
                    {
                        break;
                    }
                case TimeEventType.NewYear:
                    {
                        break;
                    }
                case TimeEventType.SeasonStart:
                    {
                        break;
                    }
            }
        }
        #endregion

        #region Sprite Functions

        /// <summary>
        /// Sets field sprites to given sprite.
        /// </summary>
        /// <param name="sprite"></param>
        void SetFieldSprite(Sprite sprite)
        {
            for (int x = 0; x < Footprint.x; x++)
            {
                for (int y = 0; y < Footprint.y; y++)
                {
                    GameManager.instance.worldMap.PlaceTileAtLocation<BuildableObjectTile>(sprite, new Vector3Int(OriginTileCoordinates.x + x, OriginTileCoordinates.y + y, 0), GameManager.instance.worldMap.objectTilemap);
                }
            }
        }

        public void InitializeField()
        {
            InitializeListeners();
            CreateBlankSchedule();
        }

        public void InitializeObject(Vector2Int footprint, Vector3Int originTile, int objectVersion,
        string dictionaryKey, int uniqueID, BuildableObjectType type, string cropTypeKey, int ownerID = 0)
        {
            // Call base initialization
            InitializeObject(footprint, originTile, objectVersion,
            dictionaryKey, uniqueID, ownerID);
            CropStringKey = cropTypeKey;
            SetCurrentCrop(cropTypeKey);

            InitializeField();
        }

        public override void InitializeObject<DataType>(DataType objectData, ref Dictionary<string, Sprite[][,]> tileSprites)
        {
            data = objectData as FieldData;

            // Call base initialization to create BuildableObjectData
            InitializeObject(objectData.footprint.ToVector2Int(), objectData.originCoords.ToVector3Int(), objectData.variant,
            objectData.tileDictionaryKey, objectData.ID, objectData.ownerID);

            this.tileSprites = tileSprites;
            arrayInitialized = true;

            CropStringKey = "Corn";
            SetCurrentCrop(CropStringKey);

            InitializeField();


        }

        /// <summary>
        /// Creates tile sprite array of field size using provided 1x1 texture.
        /// </summary>
        public override void InitTileSpritesArray()
        {
            if (!arrayInitialized)
            {

                foreach (TilesetTextureIndex entry in tileTextures)
                {
                    string[] footprintString = entry.Key.Split('x');

                    int footprintX = System.Convert.ToInt32(footprintString[0]);
                    int footprintY = System.Convert.ToInt32(footprintString[1]);
                    Sprite[][,] tileSpriteIndex = new Sprite[entry.Value.Length][,];
                    for (int i = 0; i < entry.Value.Length; i++)
                    {
                        Sprite[,] spriteArray = new Sprite[footprintX, footprintY];
                        spriteArray[0, 0] = SpriteHelper.GetSpriteAtlas(entry.Value[i], pixelsPerUnit)[0, 0];
                        for (int x = 0; x < footprintX; x++)
                        {
                            for (int y = 0; y < footprintY; y++)
                            {
                                spriteArray[x, y] = spriteArray[0, 0];
                            }
                        }
                        tileSpriteIndex[i] = spriteArray;
                    }
                    tileSprites.Add(entry.Key, tileSpriteIndex);
                }
                arrayInitialized = true;
            }
            else
            {
            }
        }

        #endregion
    }
}
