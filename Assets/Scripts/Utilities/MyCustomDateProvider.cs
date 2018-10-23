using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crops.Utilities
{

    public class MyCustomDateProvider : IFormatProvider, ICustomFormatter
    {
        /// <summary>
        /// Allow for custom name in place of "Year".
        /// </summary>
        public string yearName = "Year";

        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
                return this;

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is DateTime)) throw new NotSupportedException();

            var dt = (DateTime)arg;

            string suffix;

            if (dt.Day == 11 || dt.Day == 12 || dt.Day == 13)
            {
                suffix = "th";
            }
            else if (dt.Day % 10 == 1)
            {
                suffix = "st";
            }
            else if (dt.Day % 10 == 2)
            {
                suffix = "nd";
            }
            else if (dt.Day % 10 == 3)
            {
                suffix = "rd";
            }
            else
            {
                suffix = "th";
            }

            return string.Format("{0:MMMM} {1}{2}, {0:'" + yearName + " 'y}", arg, dt.Day, suffix);
        }
    }
}