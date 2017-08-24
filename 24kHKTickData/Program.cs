﻿using Newtonsoft.Json;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;

namespace HK24kTickData
{
    class Program
    {
        static void Main(string[] args)
        {
            ////Console.WriteLine("Hello World!");
            //string oldTimeMs = "1502806131394";
            //DateTime t = PackTool.ToDateTimeMs(oldTimeMs);

            ////2017-08-15 22:08:51,394
            //Console.WriteLine(t.ToString());
            //string timeMs = PackTool.ToUnixTimeMs(t);
            //Contract.Assert(timeMs == oldTimeMs);
            //Console.Read();


            string jqueryCalllBack = "jQuery16";
            string pcode = "022";//LLC=022 LLG=023
            PageType pageType = PageType.Current;

            string tickFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "test.csv");
            tickFilePath = tickFilePath.Replace("file:\\", "").TrimStart('\\');
            long totalTickCount = 0;
            int tryTimes = 0;
            long callTimes = 0;
            using (FileStream fs = new System.IO.FileStream(tickFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.AutoFlush = true;

                CookieContainer cookie = new CookieContainer();
                using (HttpClient hc = new HttpClient(cookie))
                {
                    hc.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                    hc.Headers[HttpRequestHeader.AcceptLanguage] = "zh-CN,zh;q=0.8";
                    //hc.Headers[HttpRequestHeader.Connection] = "keep-alive";
                    hc.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.101 Safari/537.36";

                    DateTime queryDate = DateTime.Parse("2017-08-18 00:00:00");
                    string startDateStr = HttpUtility.UrlEncode(queryDate.ToString("yyyy-MM-dd hh:mm:ss"));
                    string tickcode = "";
                    bool error = false;
                    Random rnd = new Random();

                    crabData:
                    string url = string.Format("http://oa.24k.hk/special/getHistoryData?callbackFun={0}&pcode={1}&searchdate={2}&pagetype={3}&tickcode={4}",
                        jqueryCalllBack, pcode, startDateStr, pageType.GetHashCode(), tickcode);

                    byte[] jsBytes = null;
                    try
                    {
                        tryTimes++;
                        callTimes++;
                        jsBytes = hc.DownloadData(url);
                    }
                    catch (WebException)
                    {
                        error = true;
                    }

                    if (error == false)
                    {
                        tryTimes = 0;

                        string jsString = System.Text.Encoding.UTF8.GetString(jsBytes);
                        string jsonStr = jsString.Substring(jqueryCalllBack.Length + 1).TrimEnd(')');
                        var pageTickInfo = JsonConvert.DeserializeObject<PageTickDataInfo>(jsonStr);

                        if (pageTickInfo.tickList != null && pageTickInfo.tickList.Any())
                        {
                            totalTickCount += pageTickInfo.tickList.Length;
                            foreach (var tick in pageTickInfo.tickList)
                            {
                                sw.WriteLine(string.Format("{0},{1},{2}", tick.bidtime.ToUnixTime(), tick.bid, tick.ask));
                            }
                            //sw.Flush();
                        }

                        if (tickcode == string.Empty)
                            pageType = PageType.NextPage;

                        tickcode = pageType == PageType.NextPage ? pageTickInfo.lastCode : pageTickInfo.firstCode;

                        if (tickcode != null)
                        {
                            Console.WriteLine("已抓取{0}条.", totalTickCount);
                            Thread.Sleep(1000 * rnd.Next(1, 5));
                            goto crabData;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(pageTickInfo.infoMsg))
                                Console.WriteLine(pageTickInfo.infoMsg);
                            Console.WriteLine("已持续执行{0}次， ok.", callTimes);
                        }
                    }
                    else
                    {
                        if (tryTimes < 5)
                            goto crabData;
                    }

                }
                sw.Close();
            }

            Console.WriteLine("数据文件保存在{0}", tickFilePath);
            Console.Read();
        }
    }
}