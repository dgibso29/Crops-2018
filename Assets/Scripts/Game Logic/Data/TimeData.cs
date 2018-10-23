using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Crops.Utilities
{
    [Serializable]
    public class TimeData
    {
        public string currentSeason;

        public DateTime date;
        public string formattedDate;

        public int currentDay = 0;
        public int currentWeek = 1;
        public int currentMonth = 1;
        public int currentYear = 1;

        public bool gameIsRunning = true;
        public bool paused = false;
    }
}