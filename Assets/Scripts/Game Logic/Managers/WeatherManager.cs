using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops;

namespace Crops.World
{
    public class WeatherManager : MonoBehaviour
    {

        public WeatherData Data
        {
            get { return gameManager.gameData.weatherData; }
            set { gameManager.gameData.weatherData = value; }
        }

        /// <summary>
        /// Current global pest level based on current weather conditions.
        /// </summary>
        public static ConditionLevel GlobalPestLevel = 0;
        /// <summary>
        /// Current global weed level based on current weather conditions.
        /// </summary>
        public static ConditionLevel GlobalWeedLevel = 0;
        /// <summary>
        /// Current global fungus level based on current weather conditions.
        /// </summary>
        public static ConditionLevel GlobalFungusLevel = 0;

        public GameManager gameManager;

        /// <summary>
        /// Active weather type for the current week.
        /// </summary>
        public WeatherType activeWeatherType;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        void SetActiveWeatherType(string weatherTypeKey)
        {
            activeWeatherType = ((WeatherType)(Tileset.TilesetDictionary[weatherTypeKey]));
        }

        /// <summary>
        /// Initializes weather for new game.
        /// </summary>
        public void Initialize()
        {
            GenerateInitialWeather();
            ReportFullForecast();
        }

        public void ReportFullForecast()
        {
            ReportCurrentWeather();
            ReportForecastWeather();
        }

        void ReportCurrentWeather()
        {
            Debug.Log($"Current Temp: {Data.currentTempC}. Current Precip: {Data.currentPrecipitationMM}. Current Weather: {Data.currentWeatherKey}.");
        }

        void ReportForecastWeather()
        {
            Debug.Log($"Forecast Temp: {Data.forecastTempC}. Forecast Precip: {Data.forecastPrecipitationMM}. Forecast Weather: {Data.forecastWeatherKey}.");
        }

        /// <summary>
        /// Starts progression of weather from save.
        /// </summary>
        public void InitializeFromSave()
        {

        }

        /// <summary>
        /// Generate current weather on initilization.
        /// </summary>
        void GenerateInitialWeather()
        {
            // Get current month index
            int monthIndex = gameManager.time.GetRawDate.Month;

            // Get temp & precipitation
            float temp = GetTempFromMonth(monthIndex);
            float precip = GetPrecipFromMonth(monthIndex);

            // Get weather type key
            string weatherTypeKey = GetWeatherTypeKey(temp, precip, monthIndex);

            // Set all of these
            Data.currentTempC = temp;
            Data.currentPrecipitationMM = precip;
            Data.currentWeatherKey = weatherTypeKey;

            // Generate forecast weather
            GenerateWeatherForecast();

            SetActiveWeatherType(weatherTypeKey);
            // Done
        }

        /// <summary>
        /// Generate forecast for next week based on current weather.
        /// </summary>
        void GenerateWeatherForecast()
        {
            int monthIndex = GetForecastMonthIndex();

            // Generate forecast temp & precip
            float temp = GetTempFromMonth(monthIndex);
            float precip = GetPrecipFromMonth(monthIndex);

            // Get forecast weather type key
            string weatherTypeKey = GetWeatherTypeKey(temp, precip, monthIndex);

            // Set 
            Data.forecastTempC = temp;
            Data.forecastPrecipitationMM = precip;
            Data.forecastWeatherKey = weatherTypeKey;
        }

        /// <summary>
        /// Generates global pest, weed, and fungus levels based on current weather conditions.
        /// </summary>
        void GenerateGlobalConditionLevels()
        {
            ConditionLevel expectedPests = GetExpectedPestLevel();
            ConditionLevel expectedWeeds = GetExpectedWeedLevel();
            ConditionLevel expectedFungi = GetExpectedFungusLevel();

            // Increment/decrement each condition relative to expected.
            if(expectedPests > GlobalPestLevel)
            {
                GlobalPestLevel++;
            }
            else if (expectedPests < GlobalPestLevel)
            {
                GlobalPestLevel--;
            }

            if (expectedWeeds > GlobalWeedLevel)
            {
                GlobalWeedLevel++;
            }
            else if (expectedWeeds < GlobalWeedLevel)
            {
                GlobalWeedLevel--;
            }

            if (expectedFungi > GlobalFungusLevel)
            {
                GlobalFungusLevel++;
            }
            else if (expectedFungi < GlobalFungusLevel)
            {
                GlobalFungusLevel--;
            }
        }

        /// <summary>
        /// Returns expected pest level from current conditions.
        /// </summary>
        /// <returns></returns>
        ConditionLevel GetExpectedPestLevel()
        {
            // If freezing, no pests.
            if (GetCurrentTempInC() <= 5)
            {
                return ConditionLevel.None;
            }
            if (GetCurrentTempInC() <= 10)
            {
                return ConditionLevel.Low;
            }
            if (GetCurrentTempInC() <= 15)
            {
                return ConditionLevel.Medium;
            }
            if (GetCurrentTempInC() <= 20)
            {
                return ConditionLevel.High;
            }
            if (GetCurrentTempInC() <= 30)
            {
                return ConditionLevel.Max;
            }
            if (GetCurrentTempInC() <= 35)
            {
                return ConditionLevel.High;
            }
            if (GetCurrentTempInC() <= 40)
            {
                return ConditionLevel.Medium;
            }
            if (GetCurrentTempInC() <= 45)
            {
                return ConditionLevel.Low;
            }
            // If we get here, it's too damn hot for most bugs.
            return ConditionLevel.None;

        }

        /// <summary>
        /// Returns expected weed level from current conditions.
        /// </summary>
        /// <returns></returns>
        ConditionLevel GetExpectedWeedLevel()
        {
            // If too cold, no weeds.
            if (GetCurrentTempInC() <= 10)
            {
                return ConditionLevel.None;
            }
            // If coldish but rainy, some weeds.
            if (GetCurrentTempInC() <= 20 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.Low;
            }
            // Ignore temp from this point.
            switch (activeWeatherType.Category)
            {
                case WeatherCategory.Overcast:
                    {
                        return ConditionLevel.None;
                    }
                case WeatherCategory.PartlyCloudy:
                    {
                        return ConditionLevel.Low;
                    }
                case WeatherCategory.Sunny:
                    {
                        return ConditionLevel.Medium;
                    }
                case WeatherCategory.LightPrecipitation:
                case WeatherCategory.MinorStorm:
                    {
                        return ConditionLevel.High;
                    }
                case WeatherCategory.HeavyPrecipitation:
                case WeatherCategory.MajorStorm:
                    {
                        return ConditionLevel.Max;
                    }
            }
            // If here, default to low.
            return ConditionLevel.Low;
        }

        /// <summary>
        /// Returns expected fungus level from current conditions.
        /// </summary>
        /// <returns></returns>
        ConditionLevel GetExpectedFungusLevel()
        {
            // If too cold or no rain, no fungus.
            if (GetCurrentTempInC() <= 10 || activeWeatherType.Category <= WeatherCategory.Overcast)
            {
                return ConditionLevel.None;
            }
            if (GetCurrentTempInC() <= 15 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.Medium;
            }
            if (GetCurrentTempInC() <= 20 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.High;
            }
            if (GetCurrentTempInC() <= 30 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.Max;
            }
            if (GetCurrentTempInC() <= 35 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.Medium;
            }
            if (GetCurrentTempInC() <= 40 && activeWeatherType.Category > WeatherCategory.Overcast)
            {
                return ConditionLevel.Low;
            }
            // Default to none
            return ConditionLevel.None;


        }

        /// <summary>
        /// Advances weather by one week, generating new forecast weather.
        /// </summary>
        public void AdvanceWeatherOneWeek()
        {
            SetForecastToCurrent();
            GenerateWeatherForecast();
            GenerateGlobalConditionLevels();
            ReportFullForecast();
        }

        /// <summary>
        /// Sets forecast weather to current
        /// </summary>
        void SetForecastToCurrent()
        {
            Data.currentTempC = Data.forecastTempC;
            Data.currentPrecipitationMM = Data.forecastPrecipitationMM;
            Data.currentWeatherKey = Data.forecastWeatherKey;

            SetActiveWeatherType(Data.forecastWeatherKey);

        }

        /// <summary>
        /// Displays weather effects for the current weather type, if any.
        /// </summary>
        /// <returns></returns>
        IEnumerator DisplayWeatherEffects()
        {

            yield break;
        }


        /// <summary>
        /// Returns index (1 = January) for month of forecast week.
        /// </summary>
        /// <returns></returns>
        int GetForecastMonthIndex()
        {
            // If the next week is still in the current month
            if(gameManager.time.GetRawDate.AddDays(7).Month == gameManager.time.GetRawDate.Month)
            {
                // Return that month
                return gameManager.time.GetRawDate.Month - 1;
            }
            // Otherwise, return the next month.
            return gameManager.time.GetRawDate.AddMonths(1).Month - 1;
        }

        /// <summary>
        /// Get temperature in C from month's climate data.
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        float GetTempFromMonth(int month)
        {
            return Random.Range(Tileset.activeClimateData.AverageLowTempInCelcius[month], Tileset.activeClimateData.AverageHighTempInCelcius[month] + 1);
        }

        /// <summary>
        /// Get precipitation in mm from month's climate data.
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        float GetPrecipFromMonth(int month)
        {
            float averagePrecip = Tileset.activeClimateData.AveragePrecipitationInMM[month];
            return Random.Range(Mathf.Floor(averagePrecip - 15), (averagePrecip + 1) + 15) / 4;
        }

        /// <summary>
        /// Get weather type key from given temp & precipitation, 
        /// for the given month (values are adjusted to match magnitude of average monthly precipitation).
        /// </summary>
        /// <returns></returns>
        string GetWeatherTypeKey(float tempC, float precipitation, int monthIndex)
        {
            string keyToUse;

            WeatherCategory categoryToGet;

            float averagePrecip = Tileset.activeClimateData.AveragePrecipitationInMM[monthIndex];

            /// Multiply breakpoints by this to normalize this function to differing average precipitations.
            int normalizingValue = 1 + ((int)averagePrecip / 25);

            if (precipitation < 2.5f * normalizingValue)
            {
                categoryToGet = WeatherCategory.Sunny;
            }
            else if(precipitation < 5 * normalizingValue)
            {
                categoryToGet = WeatherCategory.PartlyCloudy;
            }
            else if(precipitation < 7.5 * normalizingValue)
            {
                categoryToGet = WeatherCategory.Overcast;
            }
            else if(precipitation < 15 * normalizingValue)
            {
                if(tempC <= 0)
                {
                    MathHelper.IntRange normal = new MathHelper.IntRange(0, 0, 70);
                    MathHelper.IntRange storm = new MathHelper.IntRange(1, 1, 30);
                    int result = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[2] { normal, storm });
                    if(result == 0)
                    {
                        categoryToGet = WeatherCategory.LightFreezingPrecipitation;
                    }
                    else
                    {
                        categoryToGet = WeatherCategory.MinorWinterStorm;
                    }
                }
                else
                {
                    MathHelper.IntRange normal = new MathHelper.IntRange(0, 0, 70);
                    MathHelper.IntRange storm = new MathHelper.IntRange(1, 1, 30);
                    int result = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[2] { normal, storm });
                    if (result == 0)
                    {
                        categoryToGet = WeatherCategory.LightPrecipitation;
                    }
                    else
                    {
                        categoryToGet = WeatherCategory.MinorStorm;
                    }
                }
            }
            else
            {
                if (tempC <= 0)
                {
                    MathHelper.IntRange normal = new MathHelper.IntRange(0, 0, 80);
                    MathHelper.IntRange storm = new MathHelper.IntRange(1, 1, 20);
                    int result = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[2] { normal, storm });
                    if (result == 0)
                    {
                        categoryToGet = WeatherCategory.HeavyFreezingPrecipitation;
                    }
                    else
                    {
                        categoryToGet = WeatherCategory.MajorWinterStorm;
                    }
                }
                else
                {
                    MathHelper.IntRange normal = new MathHelper.IntRange(0, 0, 80);
                    MathHelper.IntRange storm = new MathHelper.IntRange(1, 1, 20);
                    int result = MathHelper.RandomRange.WeightedRange(new MathHelper.IntRange[2] { normal, storm });
                    if (result == 0)
                    {
                        categoryToGet = WeatherCategory.HeavyPrecipitation;
                    }
                    else
                    {
                        categoryToGet = WeatherCategory.MajorStorm;
                    }
                }
            }
            try
            {
                List<string> keysOfCategory = new List<string>();
                foreach (Tileset.TilesetIndex entry in gameManager.worldMap.currentTileset.weatherTypes)
                {
                    if (entry.key.Contains(categoryToGet.ToString()))
                    {
                        keysOfCategory.Add(entry.key);
                    }
                }
                keyToUse = keysOfCategory[Random.Range(0, keysOfCategory.Count)];
            }
            catch
            {
                keyToUse = categoryToGet.ToString();
            }
            return keyToUse;
        }

        #region Helper Functions

        /// <summary>
        /// Returns the current temperature in Celcius.
        /// </summary>
        public float GetCurrentTempInC()
        {
            return Data.currentTempC;
        }

        /// <summary>
        /// Returns the current temperature in the player's preferred unit.
        /// </summary>
        /// <returns></returns>
        public float GetCurrentTempInPreferredUnit()
        {
            if(MasterManager.gameConfig.tempInCelcius == false)
            {
                return ConvertTempToF(Data.currentTempC);
            }
            return Data.currentTempC;
        }

        /// <summary>
        /// Returns the forecast temperature in Celcius.
        /// </summary>
        public float GetForecastTempInC()
        {
            return Data.forecastTempC;
        }

        /// <summary>
        /// Returns the forecast temperature in the player's preferred unit.
        /// </summary>
        /// <returns></returns>
        public float GetForecastTempInPreferredUnit()
        {
            if (MasterManager.gameConfig.tempInCelcius == false)
            {
                return ConvertTempToF(Data.forecastTempC);
            }
            return Data.forecastTempC;
        }

        /// <summary>
        /// Returns the given Celcius temperature in Fahrenheit.
        /// </summary>
        /// <param name="tempInC"></param>
        /// <returns></returns>
        public static float ConvertTempToF(float tempInC)
        {
            return (1.8f * tempInC) + 32;
        }

        /// <summary>
        /// Returns the given Fahrenheit temperature in Celcius.
        /// </summary>
        /// <param name="tempInF"></param>
        /// <returns></returns>
        public static float ConvertTempToC(float tempInF)
        {
            return 1.8f * (tempInF - 32);
        }

        /// <summary>
        /// Returns the current precipitation.
        /// </summary>
        public float GetCurrentPrecipitation()
        {
            return Data.currentPrecipitationMM;
        }

        /// <summary>
        /// Returns the forecast precipitation.
        /// </summary>
        public float GetForecastPrecipitation()
        {
            return Data.forecastPrecipitationMM;
        }

        /// <summary>
        /// Returns the current weather category.
        /// </summary>
        /// <returns></returns>
        public WeatherCategory GetCurrentWeatherCategory()
        {
            return activeWeatherType.Category;
        }

        #endregion

    }
}
