using System;
using System.IO;

namespace HstFileReader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine(Pack.ToUnixTime(DateTime.Parse("2017/7/25 13:11:16")).ToString());
            Console.WriteLine(Pack.ToDateTime(1500959476).ToString("yyyy-MM-dd HH:mm:ss"));
            ReadFileVersion();
            Console.Read();
        }

        static void ReadFileVersion()
        {
            string filePath = @"D:\Dev\HSTViewer\XAUUSD1.hst";

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                byte[] bufBytes = new byte[64];

                //version int(4) = version
                int iIntVer = fs.ReadInt(ref bufBytes);
                Console.WriteLine(iIntVer.ToString());

                //copyright string(64) = 版权信息
                string strVal = fs.ReadString(ref bufBytes, 64);
                Console.WriteLine(strVal);

                // symbol 货币对名称，如"EURUSD"
                strVal = fs.ReadString(ref bufBytes, 12);
                Console.WriteLine(strVal);

                // period 数据周期：15代表 M15周期
                iIntVer = fs.ReadInt(ref bufBytes);
                Console.WriteLine(iIntVer.ToString());

                // digits 数据格式：小数点位数     //例如5，代表有效值至小数点5位，1.
                iIntVer = fs.ReadInt(ref bufBytes);
                Console.WriteLine(iIntVer.ToString());

                // time_t time sign 文件的创建时间
                // 1500959476 = 2017/7/25 13:11:16
                DateTime createTime = fs.ReadTime(ref bufBytes);
                strVal = createTime.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine(strVal);

                fs.Position += 52;//skip 52 bytes
                                  //96 + 52 = 148

            RateInfo:
                //ctm
                DateTime tickTime = fs.ReadTime(ref bufBytes);
                strVal = tickTime.ToString("yyyy-MM-dd HH:mm:ss");
                Console.WriteLine("ctm = {0}", strVal);

                fs.Position += 4;
                //open
                Double dVal = fs.ReadDouble(ref bufBytes);
                Console.WriteLine("open = {0}", dVal);

                //high
                dVal = fs.ReadDouble(ref bufBytes);
                Console.WriteLine("high = {0}", dVal);

                //low
                dVal = fs.ReadDouble(ref bufBytes);
                Console.WriteLine("low = {0}", dVal);

                //close
                dVal = fs.ReadDouble(ref bufBytes);
                Console.WriteLine("close = {0}", dVal);

                //vol
                iIntVer = fs.ReadInt(ref bufBytes);
                Console.WriteLine("vol = {0}", iIntVer);

                if (fs.Position + 16 < fs.Length-1)
                {
                    fs.Position += 16;
                    Console.WriteLine();
                    goto RateInfo;
                }

                Console.WriteLine("读取数据完成！");

            }
        }
    }
}