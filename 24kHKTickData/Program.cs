using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace HK24kTickData
{
    class Program
    {
        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();

            ////Console.WriteLine("Hello World!");
            //string oldTimeMs = "1502806131394";
            //DateTime t = PackTool.ToDateTimeMs(oldTimeMs);

            ////2017-08-15 22:08:51,394
            //Console.WriteLine(t.ToString());
            //string timeMs = PackTool.ToUnixTimeMs(t);
            //Contract.Assert(timeMs == oldTimeMs);
            //Console.Read();

            string jqueryCalllBack = "jQuery19";
            string pcode = "022";//LLC=022 LLG=023
            PageType pageType = PageType.Current;

            DateTime? useDefineTime = null;
            DateTime startTime = default(DateTime);
            if (args.Length > 0)
            {
                //允许设置开始时间
                if (DateTime.TryParse(args[0], out startTime))
                    useDefineTime = startTime;

                //允许10位UnixTime
                if (Regex.IsMatch(args[0], "^\\d{10}$"))
                    useDefineTime = long.Parse(args[0]).ToDateTime();
            }

            if (args.Length > 2 && (args[2] == "022" || args[2] == "023"))
                pcode = args[2];

            string tickFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), pcode + ".csv");
            tickFilePath = tickFilePath.Replace("file:\\", "").TrimStart('\\');
            //允许设置覆盖的文件
            if (args.Length > 1 && Directory.Exists(Path.GetDirectoryName(args[1])))
                tickFilePath = args[1];

            if (args.Length > 0 && File.Exists(args[0]))
            {
                tickFilePath = args[0];
                using (StreamReader sr = new StreamReader(args[0]))
                {
                    string lineStr = sr.ReadLine();
                    string pattern = "(\\d{10}),([\\d\\.]+),([\\d\\.]+)";
                    if (lineStr != null && Regex.IsMatch(lineStr, pattern))
                    {
                        while ((lineStr = sr.ReadLine()) != null)
                        {
                            Match m = Regex.Match(lineStr, pattern);
                            if (m.Success)
                                useDefineTime = long.Parse(m.Groups[1].Value).ToDateTime();
                        }
                    }
                    sr.Close();
                }
            }

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

                    DateTime queryDate = useDefineTime.HasValue ? useDefineTime.Value : DateTime.Now.Date.AddDays(-7);

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
                        error = false;
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
                        var bidTime = default(DateTime);
                        if (pageTickInfo.tickList != null && pageTickInfo.tickList.Any())
                        {
                            totalTickCount += pageTickInfo.tickList.Length;
                            foreach (var tick in pageTickInfo.tickList)
                            {
                                sw.WriteLine(string.Format("{0},{1},{2}", tick.bidtime.ToUnixTime(), tick.bid, tick.ask));
                                bidTime = tick.bidtime;
                            }
                            sw.Flush();
                        }

                        if (tickcode == string.Empty)
                            pageType = PageType.NextPage;

                        tickcode = pageType == PageType.NextPage ? pageTickInfo.lastCode : pageTickInfo.firstCode;

                        if (tickcode != null)
                        {
                            PackTool.CurrentLineReplace(string.Format("已抓取{0}条, {1:yyyy-MM-dd HH:mm:ss}.", totalTickCount, bidTime));
                            Thread.Sleep(1000 * rnd.Next(1, 5));
                            goto crabData;
                        }
                        else
                        {
                            Console.Write(Environment.NewLine);
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
