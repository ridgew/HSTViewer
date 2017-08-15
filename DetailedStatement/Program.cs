using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MT4OrderAnalyze
{
    class Program
    {
        static void Main(string[] args)
        {
            //string filePath = @"D:\MT4EA\MT4OrderAnalyze\bin\Debug\DetailedStatement11000.htm";
            string filePath = @"C:\Users\Ridge\AppData\Roaming\MetaQuotes\Terminal\50CA3DFB510CC5A8F28B48D1BF2A5702\DetailedStatement8011303.htm";
            if (args.Length > 0 && File.Exists(args[0]))
            {
                filePath = args[0];
            }

            if (!File.Exists(filePath))
            {
                Environment.Exit(0);
            }

            string fTxt = File.ReadAllText(filePath, Encoding.UTF8);

            MatchRule ItemRule = new MatchRule
            {
                RuleId = "ItemContainer",
                Begin = "<tr",
                End = "</tr>",
                SubRules = new List<MatchRule> {
                    new MatchRule { Begin="<td", End="</td>", RuleId = "TradeItem" }
                }
            };

            int startIdx = 0, iCloseCount = 0;
            string strTradeItem = string.Empty;
            List<TradeItemResult> TradeItemList = new List<TradeItemResult>();
            MatchResult result = StringUtil.MatchRuleResult(ItemRule, fTxt, startIdx);
            while (result.IsMatched)
            {
                #region 分行交易记录
                strTradeItem = result.SourceViewOf(fTxt);
                if (result.SubResults != null && result.SubResults.Any())
                {
                    if (result.SubResults.Count == TradeItemResult.CLOSE_ITEM_LENGTH || result.SubResults.Count == TradeItemResult.CANCELL_ITEM_LENGTH)
                    {
                        if (result.SubResults[0].SourceViewOf(strTradeItem).IndexOf("Ticket") == -1)
                        {
                            //有关闭时间
                            if (result.SubResults[8].SourceViewOf(strTradeItem).IndexOf(":") != -1)
                            {
                                result.GroupId = "TradeItem";
                                TradeItemList.Add(TradeItemResult.FromSource(result, strTradeItem));
                                iCloseCount++;

                            }
                        }
                    }
                    else if (result.SubResults.Count == TradeItemResult.BALANCE_ITEM_LENGTH)
                    {
                        if (result.SubResults[2].SourceViewOf(strTradeItem).IndexOf("balance") != -1)
                        {
                            result.GroupId = "BalanceItem";
                            TradeItemList.Add(TradeItemResult.FromSource(result, strTradeItem));
                        }
                    }
                }

                startIdx = result.SourceIndex + result.MatchLength;
                result = StringUtil.MatchRuleResult(ItemRule, fTxt, startIdx);
                #endregion
            }

            if (TradeItemList.Count >= 2)
            {
                if (TradeItemList[0].GetOpenTime() > TradeItemList[1].GetOpenTime())
                    TradeItemList.Reverse(); //倒序
            }

            int iMatchIdx = 0, validOrderIdx = 0, iConsecutive = 0, orderPoint = 0;
            double nowMoney = 0.00, investorMoney = 0.00, lastMoney = 0.00, orderPF = 0.00, orderSize = 0.00;
            string orderNumber = string.Empty;
            string strNewTradeItem = string.Empty;
            string itemChar = string.Empty, lastChar = string.Empty;
            TimeSpan holdTime = TimeSpan.Zero;

            string strNewOutPut = fTxt;
            StringBuilder sb = new StringBuilder();
            foreach (var tItem in TradeItemList)
            {
                iMatchIdx++;

                strTradeItem = tItem.SourceViewOf(fTxt);
                orderNumber = tItem.GetOrderTicket();
                orderPF = tItem.GetProfit();

                if (tItem.IsBalanceItem())
                {
                    nowMoney += orderPF;
                    investorMoney += orderPF;
                    lastMoney = nowMoney;
                    strNewTradeItem = strTradeItem;
                }
                else
                {
                    bool isCancelledOrder = tItem.IsCancelledOrder();

                    #region 交易订单
                    if (isCancelledOrder)
                    {
                        strNewTradeItem = strTradeItem;
                    }
                    else
                    {
                        validOrderIdx++;
                        nowMoney += orderPF + tItem.GetSwap() + tItem.GetCommission();
                        if (lastMoney == nowMoney)
                        {
                            itemChar = "-";
                        }
                        else
                        {
                            itemChar = lastMoney < nowMoney ? TradeItemResult.WIN_CHAR : TradeItemResult.LOSE_CHAR;
                        }

                        iConsecutive = (itemChar != lastChar) ? 1 : (iConsecutive + 1);

                        #region 交易单变化
                        for (int i = 0, j = tItem.SubResults.Count; i < j; i++)
                        {
                            if (i != TradeItemResult.CANCELL_ITEM_LENGTH) //TAX
                            {
                                sb.Append(tItem.SubResults[i].SourceViewOf(strTradeItem));
                            }
                            else
                            {
                                holdTime = tItem.GetCloseTime().Value - tItem.GetOpenTime();
                                orderPoint = tItem.GetProfitPoint();
                                orderSize = tItem.GetOrderSize();
                                double offsetProfit = Math.Abs(orderPF) / (orderSize * (double)orderPoint);
                                string strOffSet = "";
                                if (offsetProfit != 1.00)
                                    strOffSet = " x " + offsetProfit.ToString("0.00000");

                                sb.AppendFormat("<td bgcolor=\"#F3F3F3\" title=\"{3}\">({1}{2}){0}</td>",
                                    nowMoney.ToString("0.00"), itemChar,
                                    nowMoney.CurrentPercent(investorMoney),
                                     "时长：" + holdTime.ToString() + "&#13;&#10;"
                                     + "点差：" + orderPoint.ToString() + " x " + orderSize.ToString() + strOffSet + "&#13;&#10;"
                                     + "盈亏：" + nowMoney.DiffPercent(lastMoney) + "&#13;&#10;"
                                     + "连续：" + itemChar + " " + iConsecutive.ToString()
                                    );
                            }
                        }
                        strNewTradeItem = string.Format(tItem.ContainerFormat, sb.ToString());
                        #endregion

                        strNewTradeItem = strNewTradeItem.Replace(orderNumber, string.Format("({0}){1}", validOrderIdx.ToString().PadLeft(3, '0'), orderNumber));
                        strNewOutPut = strNewOutPut.Replace(strTradeItem, strNewTradeItem);
                    }

                    lastMoney = nowMoney;
                    lastChar = itemChar;
                    #endregion
                }

                sb.Clear();
            }

            File.WriteAllText(filePath + "l", strNewOutPut);
        }
    }
}
