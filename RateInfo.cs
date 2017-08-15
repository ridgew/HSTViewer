using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace HSTViewer
{
    [Serializable]
    public class RateInfo : IComparable
    {
        public RateInfo(int digits)
        {
            digitNum = digits;
        }

        int digitNum = 2;

        public long Index { get; set; }

        public DateTime CTM { get; set; }

        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }

        /// <summary>
        /// 波幅总量
        /// </summary>
        public int RangePoints
        {
            get
            {
                string nFormat = (digitNum == 0) ? "0" : string.Concat("N", digitNum);
                int totalPoints = int.Parse((High - Low).ToString(nFormat).Replace(".", ""));
                return (Open <= Close) ? totalPoints : 0 - totalPoints;
            }
        }

        public int CompareTo(object obj)
        {
            if (obj != null && obj is RateInfo)
            {
                return (int)(Index - ((RateInfo)obj).Index);
            }
            return 0;
        }

        public int UnusedEmptySize { get; set; }
    }
}
