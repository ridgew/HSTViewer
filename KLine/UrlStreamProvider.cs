﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HSTViewer.KLine
{
    public class UrlStreamProvider : Quokka.UI.WebBrowsers.IUrlResourceStream
    {
        public static readonly UrlStreamProvider Instance = new UrlStreamProvider();

        public Stream GetByFullUrl(string url)
        {
            //hstv://host/rawData.js
            MemoryStream ms = new MemoryStream();
            StringBuilder js = new StringBuilder();
            js.AppendLine("var kResName='" + ResourceName + "';");
            string testJs = "var rawData = [[\"2015-11-16\",17229.94,17483.01,17210.43,17483.01,137590000],[\"2015-11-17\",17486.99,17489.5,17451.41,17599.33,167190000],[\"2015-11-18\",17485.49,17737.16,17485.49,17752.16,106810000],[\"2015-11-19\",17739.83,17732.75,17681.98,17772.97,114630000],[\"2015-11-20\",17732.75,17823.81,17732.75,17914.34,153140000],[\"2015-11-23\",17823.61,17792.68,17751.53,17868.18,134680000],[\"2015-11-24\",17770.9,17812.19,17683.51,17862.6,127170000],[\"2015-11-25\",17820.81,17813.39,17801.83,17854.92,82540000],[\"2015-11-27\",17806.04,17813.39,17749.32,17830.36,82540000],[\"2015-11-30\",17802.84,17719.92,17719.79,17837.24,155560000],[\"2015-12-01\",17719.72,17888.35,17719.72,17895.5,103880000],[\"2015-12-02\",17883.14,17729.68,17708.2,17901.58,102860000],[\"2015-12-03\",17741.57,17477.67,17425.56,17780.59,126990000],[\"2015-12-04\",17482.68,17847.63,17482.68,17866.47,137650000],[\"2015-12-07\",17845.49,17730.51,17639.25,17845.49,99670000],[\"2015-12-08\",17703.99,17568,17485.39,17703.99,113720000],[\"2015-12-09\",17558.18,17492.3,17403.51,17767.69,122020000],[\"2015-12-10\",17493.17,17574.75,17474.66,17697.74,107310000],[\"2015-12-11\",17574.75,17265.21,17230.5,17574.75,134510000],[\"2015-12-14\",17277.11,17368.5,17138.47,17378.02,142540000],[\"2015-12-15\",17374.78,17524.91,17341.18,17627.63,123430000],[\"2015-12-16\",17530.85,17749.09,17483.68,17784.36,123790000],[\"2015-12-17\",17756.54,17495.84,17493.5,17796.76,115780000],[\"2015-12-18\",17495.04,17128.55,17124.31,17496.58,344560000],[\"2015-12-21\",17154.94,17251.62,17116.73,17272.36,114910000],[\"2015-12-22\",17253.55,17417.27,17242.86,17451.11,91570000],[\"2015-12-23\",17427.63,17602.61,17427.63,17607.92,92820000],[\"2015-12-24\",17593.26,17552.17,17543.95,17606.34,40350000],[\"2015-12-28\",17535.66,17528.27,17437.34,17536.9,59770000],[\"2015-12-29\",17547.37,17720.98,17547.37,17750.02,69860000],[\"2015-12-30\",17711.94,17603.87,17588.87,17714.13,59760000],[\"2015-12-31\",17590.66,17425.03,17421.16,17590.66,93690000],[\"2016-01-04\",17405.48,17148.94,16957.63,17405.48,148060000],[\"2016-01-05\",17147.5,17158.66,17038.61,17195.84,105750000],[\"2016-01-06\",17154.83,16906.51,16817.62,17154.83,120250000],[\"2016-01-07\",16888.36,16514.1,16463.63,16888.36,176240000],[\"2016-01-08\",16519.17,16346.45,16314.57,16651.89,141850000],[\"2016-01-11\",16358.71,16398.57,16232.03,16461.85,127790000],[\"2016-01-12\",16419.11,16516.22,16322.07,16591.35,117480000],[\"2016-01-13\",16526.63,16151.41,16123.2,16593.51,153530000],[\"2016-01-14\",16159.01,16379.05,16075.12,16482.05,158830000],[\"2016-01-15\",16354.33,15988.08,15842.11,16354.33,239210000],[\"2016-01-19\",16009.45,16016.02,15900.25,16171.96,144360000],[\"2016-01-20\",15989.45,15766.74,15450.56,15989.45,191870000],[\"2016-01-21\",15768.87,15882.68,15704.66,16038.59,145140000],[\"2016-01-22\",15921.1,16093.51,15921.1,16136.79,145850000],[\"2016-01-25\",16086.46,15885.22,15880.15,16086.46,123250000],[\"2016-01-26\",15893.16,16167.23,15893.16,16185.79,118210000],[\"2016-01-27\",16168.74,15944.46,15878.3,16235.03,138350000],[\"2016-01-28\",15960.28,16069.64,15863.72,16102.14,130120000],[\"2016-01-29\",16090.26,16466.3,16090.26,16466.3,217940000],[\"2016-02-01\",16453.63,16449.18,16299.47,16510.98,114450000],[\"2016-02-02\",16420.21,16153.54,16108.44,16420.21,126210000],[\"2016-02-03\",16186.2,16336.66,15960.45,16381.69,141870000],[\"2016-02-04\",16329.67,16416.58,16266.16,16485.84,131490000],[\"2016-02-05\",16417.95,16204.97,16129.81,16423.63,139010000],[\"2016-02-08\",16147.51,16027.05,15803.55,16147.51,165880000],[\"2016-02-09\",16005.41,16014.38,15881.11,16136.62,127740000],[\"2016-02-10\",16035.61,15914.74,15899.91,16201.89,122290000],[\"2016-02-11\",15897.82,15660.18,15503.01,15897.82,172070000],[\"2016-02-12\",15691.62,15973.84,15691.62,15974.04,132550000],[\"2016-02-16\",16012.39,16196.41,16012.39,16196.41,142030000],[\"2016-02-17\",16217.98,16453.83,16217.98,16486.12,124080000],[\"2016-02-18\",16483.76,16413.43,16390.43,16511.84,104950000],[\"2016-02-19\",16410.96,16391.99,16278,16410.96,134340000],[\"2016-02-22\",16417.13,16620.66,16417.13,16664.24,102240000],[\"2016-02-23\",16610.39,16431.78,16403.53,16610.39,98170000],[\"2016-02-24\",16418.84,16484.99,16165.86,16507.39,93620000],[\"2016-02-25\",16504.38,16697.29,16458.42,16697.98,94120000],[\"2016-02-26\",16712.7,16639.97,16623.91,16795.98,98480000],[\"2016-02-29\",16634.15,16516.5,16510.4,16726.12,126220000],[\"2016-03-01\",16545.67,16865.08,16545.67,16865.56,105050000],[\"2016-03-02\",16851.17,16899.32,16766.32,16900.17,104470000],[\"2016-03-03\",16896.17,16943.9,16820.73,16944.31,91110000],[\"2016-03-04\",16945,17006.77,16898.84,17062.38,106910000],[\"2016-03-07\",16991.29,17073.95,16940.48,17099.25,100290000],[\"2016-03-08\",17050.67,16964.1,16921.51,17072.79,108380000],[\"2016-03-09\",16969.17,17000.36,16947.94,17048.5,116690000],[\"2016-03-10\",17006.05,16995.13,16821.86,17130.11,117570000],[\"2016-03-11\",17014.99,17213.31,17014.99,17220.09,123420000],[\"2016-03-14\",17207.49,17229.13,17161.16,17275.07,96350000],[\"2016-03-15\",17217.15,17251.53,17120.35,17251.7,92830000],[\"2016-03-16\",17249.34,17325.76,17204.07,17379.18,118710000],[\"2016-03-17\",17321.38,17481.49,17297.65,17529.01,117990000],[\"2016-03-18\",17481.49,17602.3,17481.49,17620.58,321230016],[\"2016-03-21\",17589.7,17623.87,17551.28,17644.97,84410000],[\"2016-03-22\",17602.71,17582.57,17540.42,17648.94,95450000],[\"2016-03-23\",17588.81,17502.59,17486.27,17588.81,84240000],[\"2016-03-24\",17485.33,17515.73,17399.01,17517.14,84100000],[\"2016-03-28\",17526.08,17535.39,17493.03,17583.81,70460000],[\"2016-03-29\",17512.58,17633.11,17434.27,17642.81,86160000],[\"2016-03-30\",17652.36,17716.66,17652.36,17790.11,79330000],[\"2016-03-31\",17716.05,17685.09,17669.72,17755.7,102600000],[\"2016-04-01\",17661.74,17792.75,17568.02,17811.48,104890000],[\"2016-04-04\",17799.39,17737,17710.67,17806.38,85230000],[\"2016-04-05\",17718.03,17603.32,17579.56,17718.03,115230000],[\"2016-04-06\",17605.45,17716.05,17542.54,17723.55,99410000],[\"2016-04-07\",17687.28,17541.96,17484.23,17687.28,90120000],[\"2016-04-08\",17555.39,17576.96,17528.16,17694.51,79990000],[\"2016-04-11\",17586.48,17556.41,17555.9,17731.63,107100000],[\"2016-04-12\",17571.34,17721.25,17553.57,17744.43,81020000],[\"2016-04-13\",17741.66,17908.28,17741.66,17918.35,91710000],[\"2016-04-14\",17912.25,17926.43,17885.44,17962.14,84510000],[\"2016-04-15\",17925.95,17897.46,17867.41,17937.65,118160000],[\"2016-04-18\",17890.2,18004.16,17848.22,18009.53,89390000],[\"2016-04-19\",18012.1,18053.6,17984.43,18103.46,89820000],[\"2016-04-20\",18059.49,18096.27,18031.21,18167.63,100210000],[\"2016-04-21\",18092.84,17982.52,17963.89,18107.29,102720000],[\"2016-04-22\",17985.05,18003.75,17909.89,18026.85,134120000],[\"2016-04-25\",17990.94,17977.24,17855.55,17990.94,83770000],[\"2016-04-26\",17987.38,17990.32,17934.17,18043.77,92570000],[\"2016-04-27\",17996.14,18041.55,17920.26,18084.66,109090000],[\"2016-04-28\",18023.88,17830.76,17796.55,18035.73,100920000],[\"2016-04-29\",17813.09,17773.64,17651.98,17814.83,136670000],[\"2016-05-02\",17783.78,17891.16,17773.71,17912.35,80100000],[\"2016-05-03\",17870.75,17750.91,17670.88,17870.75,97060000],[\"2016-05-04\",17735.02,17651.26,17609.01,17738.06,95020000],[\"2016-05-05\",17664.48,17660.71,17615.82,17736.11,81530000],[\"2016-05-06\",17650.3,17740.63,17580.38,17744.54,80020000],[\"2016-05-09\",17743.85,17705.91,17668.38,17783.16,85590000],[\"2016-05-10\",17726.66,17928.35,17726.66,17934.61,75790000],[\"2016-05-11\",17919.03,17711.12,17711.05,17919.03,87390000],[\"2016-05-12\",17711.12,17720.5,17625.38,17798.19,88560000],[\"2016-05-13\",17711.12,17535.32,17512.48,17734.74,86640000],[\"2016-05-16\",17531.76,17710.71,17531.76,17755.8,88440000],[\"2016-05-17\",17701.46,17529.98,17469.92,17701.46,103260000],[\"2016-05-18\",17501.28,17526.62,17418.21,17636.22,79120000],[\"2016-05-19\",17514.16,17435.4,17331.07,17514.16,95530000],[\"2016-05-20\",17437.32,17500.94,17437.32,17571.75,111990000],[\"2016-05-23\",17507.04,17492.93,17480.05,17550.7,87790000],[\"2016-05-24\",17525.19,17706.05,17525.19,17742.59,86480000],[\"2016-05-25\",17735.09,17851.51,17735.09,17891.71,79180000],[\"2016-05-26\",17859.52,17828.29,17803.82,17888.66,68940000],[\"2016-05-27\",17826.85,17873.22,17824.73,17873.22,73190000],[\"2016-05-31\",17891.5,17787.2,17724.03,17899.24,147390000],[\"2016-06-01\",17754.55,17789.67,17664.79,17809.18,78530000],[\"2016-06-02\",17789.05,17838.56,17703.55,17838.56,75560000],[\"2016-06-03\",17799.8,17807.06,17689.68,17833.17,82270000],[\"2016-06-06\",17825.69,17920.33,17822.81,17949.68,71870000],[\"2016-06-07\",17936.22,17938.28,17936.22,18003.23,78750000],[\"2016-06-08\",17931.91,18005.05,17931.91,18016,71260000],[\"2016-06-09\",17969.98,17985.19,17915.88,18005.22,69690000],[\"2016-06-10\",17938.82,17865.34,17812.34,17938.82,90540000],[\"2016-06-13\",17830.5,17732.48,17731.35,17893.28,101690000],[\"2016-06-14\",17710.77,17674.82,17595.79,17733.92,93740000],[\"2016-06-15\",17703.65,17640.17,17629.01,17762.96,94130000],[\"2016-06-16\",17602.23,17733.1,17471.29,17754.91,91950000],[\"2016-06-17\",17733.44,17675.16,17602.78,17733.44,248680000],[\"2016-06-20\",17736.87,17804.87,17736.87,17946.36,99380000],[\"2016-06-21\",17827.33,17829.73,17799.8,17877.84,85130000],[\"2016-06-22\",17832.67,17780.83,17770.36,17920.16,89440000]];";
            if (KRateInfo != null && KRateInfo.Any())
            {
                testJs = string.Empty;
                js.Append("var rawData = [");
                int idx = 0;

                string timeFormat = GetTimeFormat();
                if (KRateInfo.Count> CandlestickSize)
                {
                    KRateInfo = KRateInfo.Skip(KRateInfo.Count - CandlestickSize).ToList();
                }

                foreach (var rate in KRateInfo)
                {
                    idx++;

                    if (idx > 1)
                        js.Append(",");

                    // 数据意义：开盘(open)，收盘(close)，最低(lowest)，最高(highest)
                    js.AppendFormat("[\"{0}\",{1},{2},{3},{4},{5}]", rate.CTM.ToString(timeFormat),
                        rate.FormatPrice(rate.Open),
                        rate.FormatPrice(rate.Close),
                        rate.FormatPrice(rate.Low),
                        rate.FormatPrice(rate.High),
                        rate.Volume);
                }
                js.Append("];");
            }

            byte[] allBytes = Encoding.UTF8.GetBytes(js.ToString() + testJs);
            ms.Write(allBytes, 0, allBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        public string ResourceName { get; set; }

        public List<RateInfo> KRateInfo { get; set; }

        /// <summary>
        /// 周期
        /// </summary>
        public string RatePeriod { get; set; }

        public string GetTimeFormat()
        {
            string defaultFmt = "yyyy-MM-dd";
            if (RatePeriod == null)
            {
                return defaultFmt;
            }
            else
            {
                if (RatePeriod[0] == 'M')
                    return "dd HH:mm";
                if (RatePeriod[0] == 'H')
                    return "MM-dd HH";
                if (RatePeriod[0] == 'D')
                    return "yyyy-MM-dd";
            }
            //M1, H1, D1, W1 MN1
            return "yyyy-MM-dd";
        }

        int iCandlestickSize = 1688;
        public int CandlestickSize
        {
            get { return iCandlestickSize; }
            set { iCandlestickSize = value; }
        }

    }
}
