using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HSTViewer
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
            int total = fs.Read(buf, 0, 8);
            return BitConverter.ToInt64(buf, 0);
        }

        public static double ReadDouble(this Stream fs, ref byte[] buf)
        {
            int total = fs.Read(buf, 0, 8);
            return BitConverter.ToDouble(buf, 0);
        }

        /// <summary>
        /// 反转二进制字节序列
        /// </summary>
        public static byte[] ReverseBytes(this byte[] bytes)
        {
            int num = bytes.Length / 2;
            byte by;
            int idx;
            for (int i = 0; i < num; i++)
            {
                by = bytes[i];
                idx = bytes.Length - i - 1;
                bytes[i] = bytes[idx];
                bytes[idx] = by;
            }
            return bytes;
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

        public static string FormatSize(this long fileSize)
        {
            if (fileSize < 1024)
                return string.Format("{0} bytes", fileSize);

            if (fileSize >= 1024 && fileSize < 1024 * 1024)
                return string.Concat(((double)fileSize / 1024d).ToString("0.00"), "K");

            if (fileSize >= 1024 * 1024 && fileSize < 1024 * 1024 * 1024)
                return string.Concat(((double)fileSize / (1024d * 1024d)).ToString("0.00"), "M");

            if (fileSize >= 1024 * 1024 * 1024)
                return string.Concat(((double)fileSize / (1024d * 1024d * 1024d)).ToString("0.00"), "G");

            return string.Format("{0} 字节", fileSize);
        }

        public static string FormatPeriod(this int totalMinutes)
        {
            if (totalMinutes < 60)
                return string.Format("M{0}", totalMinutes);

            if (totalMinutes >= 60 && totalMinutes < 60 * 24)
                return string.Format("H{0}", totalMinutes / 60);

            if (totalMinutes >= 60 * 24 && totalMinutes < 60 * 24 * 7)
                return string.Format("D{0}", totalMinutes / (60 * 24)); // 1440

            if (totalMinutes >= 60 * 24 * 7 && totalMinutes < 60 * 24 * 30)
                return string.Format("W{0}", totalMinutes / (60 * 24 * 7)); //10080

            if (totalMinutes >= 60 * 24 * 30 && totalMinutes < 60 * 24 * 365)
                return string.Format("MN{0}", totalMinutes / (60 * 24 * 30)); //43200

            return string.Format("M{0}", totalMinutes);
        }
    }
}
