using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cityLife4
{
    /// <summary>
    /// The class behaves much like DateTime with the property Now. However, we  can set the time to be a different time
    /// for QA purposes.
    /// </summary>
    public static class FakeDateTime
    {
        private static int[] monthLengths = {0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private static TimeSpan deltaTime = new TimeSpan(0);
        /// <summary>
        /// Set fake time. From this point on the system will keep the delta between the "real time" and the "system time"
        /// </summary>
        /// <param name="fakeDT">the fake date and time</param>
        public static void SetFakeTime(DateTime fakeDT)
        {
            deltaTime = DateTime.Now - fakeDT;
        }
        /// <summary>
        /// return to "normal" timing
        /// </summary>
        public static void DisableFakeTime()
        {
            deltaTime = new TimeSpan(0);
        }
        /// <summary>
        /// Returns either the real time now, or the time now minus the delta that was set in "setFakeTime". 
        /// For example, if the "setFakeTime" was called at 1/10/2018, 10:00, and the fake time set there was 30/9/2018 9:00 - 
        /// then the system will keep a 25 hour difference between the real time and the fake time.
        /// </summary>
        public static DateTime Now
        {
            get
            {
                DateTime fakeTime = DateTime.Now.Subtract(deltaTime);
                return fakeTime;
            }
        }

        /// <summary>
        /// returns only the date component of the fake time now (sets time to be all 0). This is important to make a date comparison
        /// without the time component
        /// </summary>
        public static DateTime DateNow
        {
            get
            {
                DateTime dateTimeNow = Now;
                DateTime dateNow = new DateTime(dateTimeNow.Year, dateTimeNow.Month, dateTimeNow.Day);
                return dateNow;
            }
        }

        public static bool isFakeTimeEnabled()
        {
            return deltaTime != new TimeSpan(0);
        }
        /// <summary>
        /// returns the number of days in the month. For example: if the date is 15/2/2019, the result is 28.
        /// //If the date is 15/2/2020 the result is 29.
        /// </summary>
        /// <param name="aDate"></param>
        /// <returns></returns>
        public static int monthLength(DateTime aDate)
        {
            int month = aDate.Month;
            if (month == 2 && aDate.Year % 4 == 0)
            {
                //This is a leap year
                return 29;
            }
            else
            {
                return monthLengths[month];
            }
        }
    }
}