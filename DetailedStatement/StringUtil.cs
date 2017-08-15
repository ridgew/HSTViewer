using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MT4OrderAnalyze
{
    public static class StringUtil
    {
        public static MatchResult MatchRuleResult(MatchRule rule, string source, int startIndex = 0)
        {
            MatchResult result = new MatchResult { IsMatched = false, GroupId = rule.RuleId };

            int idxBegin = source.IndexOf(rule.Begin, startIndex);
            if (idxBegin != -1)
            {
                int idxEnd = source.IndexOf(rule.End, idxBegin + rule.Begin.Length);
                if (idxEnd != -1)
                {
                    result.SourceIndex = idxBegin;
                    result.MatchLength = idxEnd + rule.End.Length - idxBegin;
                    result.IsMatched = true;

                    #region 子匹配
                    if (rule.SubRules != null && rule.SubRules.Any())
                    {
                        result.SubResults = new List<MatchResult>();

                        string rangeSource = result.SourceViewOf(source);
                        int subIdx = 0;
                        string sItemView = string.Empty;
                        foreach (var sRule in rule.SubRules)
                        {
                            MatchResult subResult = MatchRuleResult(sRule, rangeSource, subIdx);
                            while (subResult.IsMatched == true)
                            {
                                subResult.GroupId = sRule.RuleId;
                                sItemView = subResult.SourceViewOf(rangeSource);
                                result.SubResults.Add(subResult);

                                subIdx = subResult.SourceIndex + subResult.MatchLength;
                                subResult = MatchRuleResult(sRule, rangeSource, subIdx);
                            }
                        }
                    }
                    #endregion
                }
            }
            return result;
        }

        public static string StripHtml(this string source)
        {
            return Regex.Replace(source, "(<[^>]+)>", "");
        }

        public static double ToDouble(this string strPrice)
        {
            return Double.Parse(strPrice.Replace(" ", ""));
        }

        public static string CurrentPercent(this double nowMoney, double investorTotal)
        {
            if (nowMoney > investorTotal)
                return ((nowMoney - investorTotal) / investorTotal).ToString("P2");
            else if (nowMoney < investorTotal)
                return "-" + ((investorTotal - nowMoney) / investorTotal).ToString("P2");
            else
                return "0.00%";
        }

        public static string DiffPercent(this double nowMoney, double lastMoney)
        {
            return CurrentPercent(nowMoney, lastMoney);
        }

    }

    public class MatchRule
    {
        public string RuleId { get; set; }

        public string Begin { get; set; }

        public string End { get; set; }

        public List<MatchRule> SubRules { get; set; }

    }

    public class MatchResult : ICloneable
    {
        /// <summary>
        /// 对应MatchRule的RuleId
        /// </summary>
        public string GroupId { get; set; }

        public int SourceIndex { get; set; }

        public int MatchLength { get; set; }

        public List<MatchResult> SubResults { get; set; }

        public bool IsMatched { get; set; }

        public object Clone()
        {
            return new MatchResult
            {
                GroupId = GroupId,
                SourceIndex = SourceIndex,
                IsMatched = IsMatched,
                SubResults = SubResults,
                MatchLength = MatchLength
            };
        }

        public string SourceViewOf(string source)
        {
            int totalLen = source.Length;
            if (SourceIndex > totalLen) return string.Empty;
            if (SourceIndex + MatchLength > totalLen)
                return source.Substring(SourceIndex);
            return source.Substring(SourceIndex, MatchLength);
        }
    }

    public class TradeItemResult : MatchResult
    {
        internal const int CLOSE_ITEM_LENGTH = 14;
        internal const int CANCELL_ITEM_LENGTH = 11;
        internal const int BALANCE_ITEM_LENGTH = 5;

        internal const string WIN_CHAR = "↑";
        internal const string LOSE_CHAR = "↓";

        public TradeItemResult(string itemString)
        {
            ItemString = itemString;
        }

        public string ItemString { get; set; }

        public string ContainerFormat
        {
            get
            {
                return ItemString.Substring(0, ItemString.IndexOf(">") + 1) + "{0}</tr>";
            }
        }

        public string GetOrderTicket()
        {
            return SubResults[0].SourceViewOf(ItemString).StripHtml();
        }

        double getSubResultDouble(int idxSub)
        {
            return SubResults[idxSub].SourceViewOf(ItemString)
                .StripHtml().ToDouble();
        }

        public double GetProfit()
        {
            if (SubResults.Count == BALANCE_ITEM_LENGTH)
                return getSubResultDouble(BALANCE_ITEM_LENGTH - 1);
            else if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(CLOSE_ITEM_LENGTH - 1);
            return 0.00;
        }

        public int GetProfitPoint()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
            {
                double diff = GetOrderPriceClose() - GetOrderPriceOpen();
                string fmt = "0.".PadRight(GetDigits() + 2, '0');
                if (diff == 0)
                {
                    return 0;
                }
                else
                {
                    return int.Parse(Math.Abs(diff).ToString(fmt).Replace(".", "").TrimStart('0'));
                }
            }
            return 0;
        }

        public double GetSwap()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(CLOSE_ITEM_LENGTH - 2);
            return 0.00;
        }

        public DateTime GetOpenTime()
        {
            return DateTime.Parse(SubResults[1].SourceViewOf(ItemString).StripHtml());
        }

        public DateTime? GetCloseTime()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH || SubResults.Count == CANCELL_ITEM_LENGTH)
                return DateTime.Parse(SubResults[8].SourceViewOf(ItemString).StripHtml());
            return null;
        }

        public string GetOrderType()
        {
            return SubResults[2].SourceViewOf(ItemString).StripHtml();
        }

        public double GetOrderSize()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(3);
            return 0.00;
        }

        public string GetOrderSymbol()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return SubResults[4].SourceViewOf(ItemString).StripHtml();
            return string.Empty;
        }

        public int GetDigits()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
            {
                string strOpen = SubResults[5].SourceViewOf(ItemString).StripHtml().Replace(" ", "").Trim();
                int idx = strOpen.IndexOf('.');
                return strOpen.Substring(idx + 1).Length;
            }
            return 0;
        }

        public double GetOrderPriceOpen()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(5);
            return 0.00;
        }

        public double GetOrderPriceClose()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(9);
            return 0.00;
        }

        public double GetCommission()
        {
            if (SubResults.Count == CLOSE_ITEM_LENGTH)
                return getSubResultDouble(CLOSE_ITEM_LENGTH - 4);
            return 0.00;
        }

        public bool IsCancelledOrder()
        {
            return SubResults.Count == CANCELL_ITEM_LENGTH;
        }

        public bool IsBalanceItem()
        {
            return GroupId == "BalanceItem";
        }

        public static TradeItemResult FromSource(MatchResult rawResult, string source)
        {
            TradeItemResult ret = new TradeItemResult(source);
            ret.GroupId = rawResult.GroupId;
            ret.SourceIndex = rawResult.SourceIndex;
            ret.IsMatched = rawResult.IsMatched;
            ret.SubResults = rawResult.SubResults;
            ret.MatchLength = rawResult.MatchLength;
            return ret;
        }
    }

}



