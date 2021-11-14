using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Common
{
    public static class DateTimeExtensions
    {
        public static bool HasTheSameDayWith(this DateTime dt1, DateTime dt2)
        {
            return dt1.Day == dt2.Day && dt1.Month == dt2.Month && dt1.Year == dt2.Year;
        }

        public static DateTime GetRandom1stJanuary(this DateTime startDate, DateTime endDate)
        {
            int startYear = startDate.Year;
            int endYear = endDate.Year;
            Random rnd = new Random();
            int randomYear = rnd.Next(startYear, endYear + 1);
            return new DateTime(randomYear, 1, 1);
        }

        public static DateTime GetRandomDate(this DateTime startDate, DateTime endDate)
        {
            int days = (int)(endDate - startDate).TotalDays;
            Random rnd = new Random();
            int daysOffset = rnd.Next(0, days + 1);
            return startDate.AddDays((double)daysOffset);
        }

        public static DateTime SetDate(this DateTime time, DateTime dateToBeSet)
        {
            //08-11-2013: Created.
            return new DateTime(dateToBeSet.Year, dateToBeSet.Month, dateToBeSet.Day,
                time.Hour,time.Minute,time.Second,time.Millisecond);
        }

        public static DateTime SetTime(this DateTime date, DateTime timeToBeSet)
        {
            //08-11-2013: Created.
            return new DateTime(date.Year,date.Month,date.Day,
                timeToBeSet.Hour, timeToBeSet.Minute, timeToBeSet.Second, timeToBeSet.Millisecond);
        }

        public static bool IsInLeapYear(this DateTime date)
        {
            return date.Year % 100 == 0 ? date.Year % 400 == 0 : date.Year % 4 == 0;
        }


    }
}
