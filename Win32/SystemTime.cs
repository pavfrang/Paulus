using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paulus.Win32
{

    //system time is a 16-byte value that is used in the registry (eg. DateCreated fields)
    public struct SystemTime : IComparable<SystemTime>, IComparable<DateTime>, IComparable
    {
        public enum DayOfWeek : short { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday }
        public enum MonthOfYear : short { January = 1, February, March, April, May, June, July, August, September, October, November, December }

        public short wYear;
        public MonthOfYear wMonth;
        public DayOfWeek wDayOfWeek;
        public short wDay;
        public short wHour;
        public short wMinute;
        public short wSecond;
        public short wMilliseconds;

        public static explicit operator SystemTime(byte[] bytes)
        {
            SystemTime tm;
            tm.wYear = BitConverter.ToInt16(bytes, 0);
            tm.wMonth = (MonthOfYear)BitConverter.ToInt16(bytes, 2);
            //works too!
            tm.wDayOfWeek = (DayOfWeek)BitConverter.ToInt16(bytes, 4); //0 is sunday
            tm.wDay = BitConverter.ToInt16(bytes, 6);
            tm.wHour = BitConverter.ToInt16(bytes, 8);
            tm.wMinute = BitConverter.ToInt16(bytes, 10);
            tm.wSecond = BitConverter.ToInt16(bytes, 12);
            tm.wMilliseconds = BitConverter.ToInt16(bytes, 14);
            return tm;
        }

        public static explicit operator DateTime(SystemTime tm)
        {
            return new DateTime(tm.wYear, (int)tm.wMonth, tm.wDay, tm.wHour, tm.wMinute, tm.wSecond, tm.wMilliseconds);
        }

        public DateTime ToDateTime() { return (DateTime)this; }

        
        #region SystemTime ToString
        public override string ToString()
        {
            return ToDateTime().ToString();
        }

        public string ToString(IFormatProvider provider)
        {
            return ToDateTime().ToString(provider);
        }

        public string ToString(string format)
        {
            return ToDateTime().ToString(format);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            return ToDateTime().ToString(format, provider);
        }
        #endregion

        #region Equality
        public override bool Equals(object obj)
        {
            if (obj is DateTime)
            {
                return ToDateTime() == (DateTime)obj;
            }
            else if (obj is SystemTime)
            {
                return ToDateTime() == ((SystemTime)obj).ToDateTime();
            }
            else return false;
        }

        public static bool operator ==(SystemTime left, SystemTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SystemTime left, SystemTime right)
        {
            return !left.Equals(right);
        }

        public override int GetHashCode()
        {
            return wYear ^ (short)wMonth ^ wDay ^ wHour ^ wMinute ^ wSecond ^ wMilliseconds;
        }

        public static bool operator ==(SystemTime left, DateTime right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SystemTime left, DateTime right)
        {
            return !left.Equals(right);
        }
        #endregion

        public int CompareTo(SystemTime other)
        {
            return ToDateTime().CompareTo(other.ToDateTime());
        }

        public int CompareTo(DateTime other)
        {
            return ToDateTime().CompareTo(other);
        }

        public int CompareTo(object obj)
        {
            if (obj is SystemTime) return CompareTo((SystemTime)obj);
            else if (obj is DateTime) return CompareTo((DateTime)obj);
            else throw
 new ArgumentException("obj is not the same type as this instance.", "obj"); //always greater than another type
        }
    }


    public static class TimeExt
    {
        public static SystemTime ToSystemTime(this DateTime tm)
        {
            SystemTime systm;
            systm.wDay = (short)tm.Day;
            systm.wDayOfWeek = (SystemTime.DayOfWeek)tm.DayOfWeek;
            systm.wHour = (short)tm.Hour;
            systm.wMilliseconds = (short)tm.Millisecond;
            systm.wMinute = (short)tm.Minute;
            systm.wMonth = (SystemTime.MonthOfYear)tm.Month;
            systm.wSecond = (short)tm.Second;
            systm.wYear = (short)tm.Year;
            return systm;
        }
    }
}
