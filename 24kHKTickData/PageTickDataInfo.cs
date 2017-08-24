using System;

namespace HK24kTickData
{
    [Serializable]
    public class PageTickDataInfo
    {
        public long infoNo { get; set; }

        public TickData[] tickList { get; set; }

        public string firstCode { get; set; }

        public string lastCode { get; set; }

        public string infoMsg { get; set; }
    }

    [Serializable]
    public class TickData
    {
        public double ask { get; set; }

        public double bid { get; set; }

        public DateTime bidtime { get; set; }

        public string gmcode { get; set; }
    }

    public enum PageType : int
    {
        PrePage = -1,
        Current = 0,
        NextPage = 1
    }
}
