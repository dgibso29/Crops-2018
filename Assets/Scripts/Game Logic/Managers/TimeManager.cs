using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Crops.Events;

namespace Crops.Utilities
{

    public class TimeManager : MonoBehaviour
    {
        public GameManager gameManager;

        public TimeData Data
        {
            get { return gameManager.gameData.timeData; }
            set { gameManager.gameData.timeData = value; }
        }

        public float dayLength = 4; // Length of each In-Game day in seconds

        /// <summary>
        /// The current in-game season.
        /// </summary>
        public string CurrentSeason
        {
            get { return Data.currentSeason; }
            set { Data.currentSeason = value; }
        }

        private float currentDayLength; // Used to modify time per day in seconds
        private DateTime Date
        {
            get { return Data.date; }
            set { Data.date = value; }
        }       
        private string FormattedDate
        {
            get { return Data.formattedDate; }
            set { Data.formattedDate = value; }
        }

        public int CurrentDay
        {
            get { return Data.currentDay; }
            set { Data.currentDay = value; }
        }
        public int CurrentWeek
        {
            get { return Data.currentWeek; }
            set { Data.currentWeek = value; }
        }
        public int CurrentMonth
        {
            get { return Data.currentMonth; }   
            set { Data.currentMonth = value; }
        }
        public int CurrentYear
        {
            get { return Data.currentYear; }
            set { Data.currentYear = value; }
        }
        private bool GameIsRunning
        {
            get { return Data.gameIsRunning; } 
            set { Data.gameIsRunning = value; }
        }
        private bool Paused
        {
            get { return Data.paused; }
            set { Data.paused = value; }
        }

        private bool twoTimesSpeed = false;
        private bool threeTimesSpeed = false;
        private bool fourTimesSpeed = false;

        private World.Tileset.Season[] seasons;
        private DateTime currentSeasonEnd;


        // Use this for initialization
        void Start()
        {
            Data = gameManager.gameData.timeData;
        }

        /// <summary>
        /// Starts progression of in-game time.
        /// </summary>
        public void Initialize()
        {
            GameManager.CurrentTileset.SetActiveClimate();
            StartCoroutine(InGameDate());
            currentDayLength = dayLength;
            seasons = World.Tileset.activeClimateData.Seasons;
            SetSeason();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateTimeScale();

        }

        void UpdateIndividualTimeValues()
        {
            CurrentDay = Date.DayOfYear;
            CurrentWeek = ((CurrentDay - 1) / 7) + 1;
            CurrentMonth = Date.Month;
            CurrentYear = Date.Year;
        }

        /// <summary>
        /// Manages the progression of time in the game.
        /// </summary>
        /// <returns></returns>
        IEnumerator InGameDate()
        {
            yield return new WaitForSecondsRealtime(currentDayLength);
            while (GameIsRunning)
            {
                while (!Paused)
                {
                    int prevDay = CurrentDay, prevWeek = CurrentWeek, prevMonth = CurrentMonth, prevYear = CurrentYear;
                    FormattedDate = string.Format(new MyCustomDateProvider(), "{0}", Date);
                    Debug.Log("Today is " + FormattedDate);
                    yield return new WaitForSecondsRealtime(currentDayLength);
                    /// If paused after waiting for the day to end, continue. Otherwise, the day will advance as soon as the game is unpaused.
                    /// The trade-off is restarting the current day, but that is much less impactful.
                    if (Paused)
                    {
                        continue;
                    }
                    AddDay();
                    UpdateIndividualTimeValues();
                    CheckForTimeEvents(prevDay, prevWeek, prevMonth, prevYear);
                }
                while (Paused)
                {
                    yield return null;
                }
            }
            Debug.Log("Time stopped!");
        }

        /// <summary>
        /// Check if any time events should be triggered.
        /// </summary>
        void CheckForTimeEvents(int prevDay, int prevWeek, int prevMonth, int prevYear)
        {
            if(CurrentDay != prevDay)
            {
                OnTimeEventOccurred(this, new TimeEventArgs(TimeEventType.NewDay));
            }
            if (CurrentWeek != prevWeek)
            {
                OnTimeEventOccurred(this, new TimeEventArgs(TimeEventType.NewWeek));
            }
            if (CurrentMonth != prevMonth)
            {
                OnTimeEventOccurred(this, new TimeEventArgs(TimeEventType.NewMonth));
            }
            if (CurrentYear != prevYear)
            {
                OnTimeEventOccurred(this, new TimeEventArgs(TimeEventType.NewYear));
            }
        }

        /// <summary>
        /// Sets the season based on the current date.
        /// </summary>
        void SetSeason()
        {

            if (seasons.Length > 1)
            {
                for (int i = 0; i < seasons.Length; i++)
                {
                    World.Tileset.Season season = seasons[i];
                    World.Tileset.Season prevSeason;
                    if (i == 0)
                    {
                        prevSeason = seasons[seasons.Length - 1];
                    }
                    else
                    {
                        prevSeason = seasons[i - 1];

                    }
                    World.Tileset.Season nextSeason;
                    if (i == seasons.Length - 1)
                    {
                        nextSeason = seasons[0];
                    }
                    else
                    {
                        nextSeason = seasons[i + 1];
                    }

                    // If Season starts and ends in different years
                    if (season.StartMonth > nextSeason.StartMonth)
                    {
                        // And the current month is the season start month
                        if (season.StartMonth == Date.Month)
                        {
                            // And the current day is on or after the start day
                            if (season.StartDay <= Date.Day)
                            {
                                CurrentSeason = season.Name;
                                currentSeasonEnd = new DateTime(Date.Year + 1, nextSeason.StartMonth, nextSeason.StartDay);
                            }
                            // And the current day is before the start day
                            else if (season.StartDay > Date.Day)
                            {
                                CurrentSeason = prevSeason.Name;
                                currentSeasonEnd = new DateTime(Date.Year, season.StartMonth, season.StartDay);
                            }
                        }
                        // And the current month is before the next season start month
                        else if (Date.Month < nextSeason.StartMonth)
                        {
                            CurrentSeason = season.Name;
                            currentSeasonEnd = new DateTime(Date.Year, nextSeason.StartMonth, nextSeason.StartDay);
                        }
                        // And the current month is the next season start month
                        else if (Date.Month == nextSeason.StartMonth)
                        {
                            // And the current day is before the next season start day
                            if (Date.Day < nextSeason.StartDay)
                            {
                                CurrentSeason = season.Name;
                                currentSeasonEnd = new DateTime(Date.Year, nextSeason.StartMonth, nextSeason.StartDay);
                            }
                        }
                    }
                    // Otherwise, things are easier.
                    else
                    {
                        // Set up start dates for the relevant seasons
                        DateTime prevStartDate;
                        DateTime currentStartDate = new DateTime(Date.Year, season.StartMonth, season.StartDay);
                        DateTime nextStartDate;
                        // Check if next or prev season start and end in different years
                        //Debug.Log(date);
                        bool isYearOne = Date.Year - 1 < 1;
                        // Previous
                        if (prevSeason.StartMonth > season.StartMonth && !isYearOne)
                        {
                            prevStartDate = new DateTime(Date.Year - 1, prevSeason.StartMonth, prevSeason.StartDay);
                        }
                        else
                        {
                            prevStartDate = new DateTime(Date.Year, prevSeason.StartMonth, prevSeason.StartDay);
                        }
                        // Next
                        if (nextSeason.StartMonth < season.StartMonth)
                        {
                            nextStartDate = new DateTime(Date.Year + 1, nextSeason.StartMonth, nextSeason.StartDay);
                        }
                        else
                        {
                            nextStartDate = new DateTime(Date.Year, nextSeason.StartMonth, nextSeason.StartDay);
                        }
                        // Compare current date against start dates.
                        // If current date is less than current start date
                        if (Date < currentStartDate && !isYearOne)
                        {
                            // And it's greater than the previous start date
                            if (Date > prevStartDate)
                            {
                                CurrentSeason = prevSeason.Name;
                                currentSeasonEnd = currentStartDate;
                            }
                        }
                        // If the current date is greater than the current start date
                        else if (Date > currentStartDate)
                        {
                            // And it's less than the next start date
                            if (Date < nextStartDate)
                            {
                                CurrentSeason = season.Name;
                                currentSeasonEnd = nextStartDate;
                            }
                        }
                    }
                }
            }
            else
            {
                CurrentSeason = seasons[0].Name;
            }
            OnTimeEventOccurred(this, new TimeEventArgs(TimeEventType.SeasonStart));
        }

        /// <summary>
        /// Returns the current formatted date.
        /// </summary>
        public string GetCurrentDate
        {
            get { return FormattedDate; }
        }
        /// <summary>
        /// Returns the current DateTime date.
        /// </summary>
        public DateTime GetRawDate
        {
            get { return Date; }
        }

        public void SetTimeFromSave(DateTime newDate)
        {
            Date = newDate;
            FormattedDate = string.Format(new MyCustomDateProvider(), "{0}", Date);
        }

        public void AddDay()
        {
            Date = Date.AddDays(1);

            if(Date > currentSeasonEnd && seasons.Length > 1)
            {
                SetSeason();
            }
        }

        public override string ToString()
        {
            FormattedDate = string.Format(new MyCustomDateProvider(), "{0}", Date);
            return FormattedDate;
        }

        void UpdateTimeScale()
        {
            if (twoTimesSpeed)
            {
                currentDayLength = dayLength * 2;
                Time.timeScale = 2;
            }
            else if (threeTimesSpeed)
            {
                currentDayLength = dayLength * 3;
                Time.timeScale = 3;
            }
            else if (fourTimesSpeed)
            {
                currentDayLength = dayLength * 4;
                Time.timeScale = 4;
            }
            else
            {
                currentDayLength = dayLength;
                Time.timeScale = 1;
            }
        }

        public bool TwoTimesSpeed
        {
            get { return twoTimesSpeed; }
            set { TwoTimesSpeed = value; }
        }

        public bool ThreeTimesSpeed
        {
            get { return threeTimesSpeed; }
            set { threeTimesSpeed = value; }
        }

        public bool FourTimesSpeed
        {
            get { return fourTimesSpeed; }
            set { fourTimesSpeed = value; }
        }
        /// <summary>
        /// Pauses or unpauses game based on current state.
        /// </summary>
        public void TogglePause()
        {
            if (!Paused)
            {
                Paused = true;
                Time.timeScale = 0;
            }
            else if (Paused)
            {
                Paused = false;
                Time.timeScale = 1;
            }
        }

        public delegate void TimeEventHandler(object sender, TimeEventArgs e);        

        public static event TimeEventHandler TimeEventOccurred;       

        public static void OnTimeEventOccurred(object sender, TimeEventArgs e)
        {
            TimeEventOccurred?.Invoke(sender, e);
        }
    }

}
