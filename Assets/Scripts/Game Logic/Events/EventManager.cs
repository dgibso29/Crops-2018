using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Crops.Utilities;

namespace Crops.Events
{
    /// <summary>
    /// Types of time events, ranging from the start of weeks and months to the changing of seasons.
    /// </summary>
    public enum TimeEventType { NewDay, NewWeek, NewMonth, NewYear, SeasonStart }

    public class EventManager : MonoBehaviour
    {

        TimeManager timeManager;
        public GameManager gameManager;

        private void Start()
        {
            timeManager = gameManager.time;
            InitializeListeners();
        }

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
                        gameManager.CheckOnNewDay();
                        break;
                    }
                case TimeEventType.NewWeek:
                    {
                        gameManager.CheckOnNewWeek();
                        break;
                    }
                case TimeEventType.NewMonth:
                    {
                        gameManager.CheckOnNewMonth();
                        break;
                    }
                case TimeEventType.NewYear:
                    {
                        gameManager.CheckOnNewYear();
                        break;
                    }
                case TimeEventType.SeasonStart:
                    {
                        gameManager.CheckOnNewSeason();
                        break;
                    }
            }
        }
    }
}
