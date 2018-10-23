using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Crops.World
{

    public enum WeatherCategory { Sunny, PartlyCloudy, Overcast, LightPrecipitation, HeavyPrecipitation, MinorStorm, MajorStorm,
        LightFreezingPrecipitation, HeavyFreezingPrecipitation, MinorWinterStorm, MajorWinterStorm }

    [CreateAssetMenu]
    public class WeatherType : LocalizedObject
    {
        /// <summary>
        /// Sprites for this weather type.
        /// </summary>
        public Sprite[] sprites;

        public WeatherCategory Category
        {
            get
            {
                return _category;
            }
            set
            {
                _category = value;
            }
        }

        [SerializeField]
        private WeatherCategory _category;

        /// <summary>
        /// 
        /// </summary>
        public float visualIntensity;

        public float brightness;

        /// <summary>
        /// Range of precipitation in MM for this type, where X is min and Y is max.
        /// </summary>
        public Vector2 precipitationRange;

        /// <summary>
        /// Range of wind speed in Kph for this type, where X is min and Y is max.
        /// </summary>
        public Vector2 windSpeedRange;



        /// <summary>
        /// Returns a random value between the min and max precipitation of the given weather type.
        /// </summary>
        /// <returns></returns>
        float GetPrecipitationAmount()
        {
            if (precipitationRange.y == 0)
            {
                return 0;
            }
            return UnityEngine.Random.Range(precipitationRange.x, precipitationRange.y + 1);
        }

        /// <summary>
        /// Returns a random value between the min and max wind speed of the given weather type.
        /// </summary>
        /// <returns></returns>
        float GetWindSpeed()
        {
            if (windSpeedRange.y == 0)
            {
                return 0;
            }
            return UnityEngine.Random.Range(windSpeedRange.x, windSpeedRange.y + 1);
        }

    }
}
