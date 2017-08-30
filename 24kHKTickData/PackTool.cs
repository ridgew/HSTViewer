using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HK24kTickData
{
    public static class PackTool
    {
        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static string ToUnixTimeMs(this DateTime time)
        {
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            long t = (time.Ticks - startTime.Ticks) / 10000;      //除10000调整为13位  
            return t.ToString();
        }

        public static DateTime ToDateTimeMs(this string unixMsTime)
        {
            long tickts = long.Parse(unixMsTime) * 10000;
            DateTime startTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            return DateTime.FromBinary(tickts + startTime.Ticks);
        }

        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime ToDateTime(this long unixTime)
        {
            return DateTime.FromBinary(unixTime * 10000000 + 621355968000000000).ToLocalTime();
        }

        /// <summary>
        /// ISO8601标准字符串(yyyy-MM-ddTHH:mm:ss.sssZ±hhmm)
        /// <para>EX： 2016-09-08T11:49:57+00:00 </para>
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <remarks>http://docs.developer.amazonservices.com/en_US/dev_guide/DG_ISO8601.html</remarks>
        public static string ToISO8601String(this DateTime time)
        {
            return time.ToString("yyyy-MM-ddTHH:mm:sszzzz", DateTimeFormatInfo.InvariantInfo);
        }

        public static void CurrentLineReplace(string lineStr)
        {
            ClearCurrentConsoleLine();
            Console.Write(lineStr);
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new String(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}
