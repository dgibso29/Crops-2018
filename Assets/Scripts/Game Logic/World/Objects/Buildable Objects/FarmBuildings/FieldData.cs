using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Crops.World;
using Crops.Utilities;

namespace Crops.World
{

    [System.Serializable]
    public class FieldData : BuildableObjectData
    {

        public Vector2IntJSON fieldFootprint;

        /// <summary>
        /// Dictionary key for this crop type.
        /// </summary>
        public string cropStringKey;

        /// <summary>
        /// Size 53 array of Field Tasks, where index 0 is the current week and the last index is exactly one year ahead.
        /// </summary>
        public List<FieldTask> fieldSchedule = new List<FieldTask>();
        
        public int growthStage = 0;

        public int growthWeeks = 0;

        public float cropQuality = 0;

        public ConditionLevel moistureLevel = ConditionLevel.Medium;

        public int weeksAtOptimalMoisture = 0;

        public ConditionLevel soilFertility = ConditionLevel.High;

        public int weeksAtOptimalFertility = 0;

        public int weeksAtOptimalTemperature = 0;

        public ConditionLevel pestLevel = 0;

        public int weeksOfPestDamage = 0;

        public ConditionLevel fungusLevel = 0;

        public int weeksOfFungusDamage = 0;

        public ConditionLevel weedLevel = 0;

        public int weeksOfWeedDamage = 0;

        /// <summary>
        /// True if field has active irrigation.
        /// </summary>
        public bool irrigated = false;

        /// <summary>
        /// True if field has active drainage.
        /// </summary>
        public bool drained = false;


        public FieldData(Vector2Int footprint, Vector3Int originTile, int objectVersion, string dictionaryKey, int uniqueID, int ownerID, string cropKey = "Fallow") : base(footprint, originTile, objectVersion, dictionaryKey, uniqueID, ownerID)
        {
            cropStringKey = cropKey;
        }


    }
}
