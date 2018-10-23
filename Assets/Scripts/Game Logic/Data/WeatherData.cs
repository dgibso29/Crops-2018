using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crops.World
{
    public class WeatherData
    {
        /// <summary>
        /// Dictionary key for the current weather type.
        /// </summary>
        public string currentWeatherKey;

        /// <summary>
        /// Dictionary key for the forecast weather type.
        /// </summary>
        public string forecastWeatherKey;

        /// <summary>
        /// Current temperature in Celcius. 
        /// </summary>
        public float currentTempC;

        /// <summary>
        /// Forecast temperature in Celcius. 
        /// </summary>
        public float forecastTempC;

        /// <summary>
        /// Current precipitation in MM.
        /// </summary>
        public float currentPrecipitationMM;

        /// <summary>
        /// Forecast precipitation in MM.
        /// </summary>
        public float forecastPrecipitationMM;

        /// <summary>
        /// Name of the active climate in this game.
        /// </summary>
        public string activeClimateName;

    }
}
