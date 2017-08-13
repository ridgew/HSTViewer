using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HstFileReader
{
    public static class Pack
    {
        public static long ToUnixTime(this DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        public static DateTime ToDateTime(this long unixTime)
        {
            return DateTime.FromBinary(unixTime * 10000000 + 621355968000000000).ToLocalTime();
        }

        public static int ReadInt(this Stream fs, ref byte[] buf)
        {
            int total = fs.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        public static long ReadLong(this Stream fs, ref byte[] buf)
        {
            long total = fs.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public static double ReadDouble(this Stream fs, ref byte[] buf)
        {
            int total = fs.Read(buf, 0, 8);
            return BitConverter.ToDouble(buf, 0);
        }

        public static DateTime ReadTime(this Stream fs, ref byte[] buf)
        {
            long iIntVer = ReadLong(fs, ref buf);
            return ToDateTime(iIntVer);
        }

        public static string ReadString(this Stream fs, ref byte[] buf, int length)
        {
            int total = fs.Read(buf, 0, length);
            return Encoding.UTF8.GetString(buf, 0, total).TrimEnd('\0');
        }
    }
}
