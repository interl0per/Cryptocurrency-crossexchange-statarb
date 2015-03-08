using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using X_Exchange_StatArb;

namespace bitfinex_pairs
{
    class Program
    {
        #region public variable init - main parameters
        public static double exitRatio = 0.1632;
        const double maxInvest = 10;
        const double investAmt = 10;
        const double xCurrRate = 0.163;
        #endregion

        class MarketOverview
        {
            public Prices prices;
            public Entry entry;
            public Exit exit;
            public class Prices
            {
                public double bfx_ask { get; set; }
                public double bfx_bid { get; set; }
                public double btce_ask { get; set; }
                public double btce_bid { get; set; }
                public double bfx_ask_adjusted { get; set; }
                public double bfx_bid_adjusted { get; set; }
                public double btce_ask_adjusted { get; set; }
                public double btce_bid_adjusted { get; set; }
            }
            public class AdjustedPrices
            {
                public double adjusted_bfx_ask { get; set; }
                public double adjusted_bfx_bid { get; set; }
                public double adjusted_btce_ask { get; set; }
                public double adjusted_btce_bid { get; set; }
            }
            public class Exit
            {
                public double bfxLimitExitVolume;
                //public double btceLimitExitVolume;
                public double marketExitVolume;
            }
            public class Entry
            {
                public double btceLimitEntryVolume;
                public double bfxLimitEntryVolume;
                public double marketEntryVolume;
            }
        }
        static Tuple<double, double, double> calculateProfit(double bfx_ask, double btce_ask, double bfx_bid = 0, double btce_bid = 0)
        {
            double bfxMarketFee = 0.001;
            double bfxLimitFee = 0;
            double e2MarketFee = 0;
            double bfxLimitEntry = (((1 - e2MarketFee) * (bfx_ask / exitRatio)) - (1 + e2MarketFee) * btce_ask) * xCurrRate + bfx_ask * (1 - bfxLimitFee) - (1 + bfxMarketFee) * bfx_ask; //closing at bid
            //enter at btce bid rather than bfx ask -                                                            btce_bid                                   market fee     bfx_bid      
            double marketEntry = ((1 - e2MarketFee) * (bfx_ask / exitRatio) - (1 + e2MarketFee) * btce_ask) * xCurrRate + bfx_bid * (1 - bfxMarketFee) - (1 + bfxMarketFee) * bfx_ask;
            double btceLimitEntry = (((1 - e2MarketFee) * (bfx_ask / exitRatio)) - (1 + e2MarketFee) * btce_bid) * xCurrRate + bfx_bid * (1 - bfxMarketFee) - (1 + bfxMarketFee) * bfx_ask;
            Tuple<double, double, double> profitLevels = new Tuple<double, double, double>(bfxLimitEntry, marketEntry, btceLimitEntry);
            return (profitLevels);
        }

        public class md
        {
            public Asks asks;
            public Bids bids;
            public class Asks
            {
                public double price;
                public double volume;
            }
            public class Bids
            {
                public double price;
                public double volume;
            }
        }

        static MarketOverview getMarketData(string e2)
        {
            md[] bfxOrders = new md[10];
            md[] e2Orders = new md[10];
            HttpWebRequest bfxDepthReq = (HttpWebRequest)WebRequest.Create("https://api.bitfinex.com/v1/book/LTCUSD?limit_bids=10&limit_asks=10");
            bitfinex_json.Depth e1Depth = JsonConvert.DeserializeObject<bitfinex_json.Depth>(Query.read(bfxDepthReq));
            if (e2 == "btcchina")
            {
                HttpWebRequest btcchinaDepthReq = (HttpWebRequest)WebRequest.Create("https://data.btcchina.com/data/orderbook?market=LTCCNY&limit=10");
                btcchina_json.Depth e2Depth = JsonConvert.DeserializeObject<btcchina_json.Depth>(Query.read(btcchinaDepthReq));
                for (int i = 0; i < 10; i++)
                {
                    bfxOrders[i] = new md();
                    bfxOrders[i].asks = new md.Asks();
                    bfxOrders[i].bids = new md.Bids();
                    bfxOrders[i].asks.price = e1Depth.asks[i].price;
                    bfxOrders[i].asks.volume = e1Depth.asks[i].amount;
                    bfxOrders[i].bids.price = e1Depth.bids[i].price;
                    bfxOrders[i].bids.volume = e1Depth.bids[i].amount;
                    e2Orders[i] = new md();
                    e2Orders[i].asks = new md.Asks();
                    e2Orders[i].bids = new md.Bids();
                    e2Orders[i].asks.price = e2Depth.asks[i][0];
                    e2Orders[i].asks.volume = e2Depth.asks[i][1];
                    e2Orders[i].bids.price = e2Depth.bids[i][0];
                    e2Orders[i].bids.volume = e2Depth.bids[i][1];
                }
            }
            else if (e2 == "btc-e")
            {
                HttpWebRequest btceDepthReq = (HttpWebRequest)WebRequest.Create("https://btc-e.com/api/3/depth/ltc_usd?limit=10");
                btce_json.Depth e2Depth = JsonConvert.DeserializeObject<btce_json.Depth>(Query.read(btceDepthReq));
                for (int i = 0; i < 10; i++)
                {
                    bfxOrders[i] = new md();
                    bfxOrders[i].asks = new md.Asks();
                    bfxOrders[i].bids = new md.Bids();
                    bfxOrders[i].asks.price = e1Depth.asks[i].price;
                    bfxOrders[i].asks.volume = e1Depth.asks[i].amount;
                    bfxOrders[i].bids.price = e1Depth.bids[i].price;
                    bfxOrders[i].bids.volume = e1Depth.bids[i].amount;
                    e2Orders[i] = new md();
                    e2Orders[i].asks = new md.Asks();
                    e2Orders[i].bids = new md.Bids();
                    e2Orders[i].asks.price = e2Depth.ltc_usd.asks[i][0];
                    e2Orders[i].asks.volume = e2Depth.ltc_usd.asks[i][1];
                    e2Orders[i].bids.price = e2Depth.ltc_usd.bids[i][0];
                    e2Orders[i].bids.volume = e2Depth.ltc_usd.bids[i][1];
                }
            }
            double bfx_ask = bfxOrders[0].asks.price;
            double bfx_bid = bfxOrders[0].bids.price;
            double btce_ask = e2Orders[0].asks.price;
            double btce_bid = e2Orders[0].bids.price;
            #region profit calculation
            //bfx limit entry 
            double bfxLimitEntryVolume = 0; //volume of best bid at other exchange
            double adjustedBtceAsk = btce_ask;
            for (int i = 0; i < 5; i++)
            {
                double profitAtNextDepth = calculateProfit(bfx_ask, btce_ask).Item1;//((1 - e2MarketFee) * (bfx_ask / exitRatio) - (1 + e2MarketFee) * btceDepth.ltc_usd.asks[i][0]) + bfx_ask - (1 + bfxLimitFee) * bfx_ask;
                if (profitAtNextDepth > 0)
                {
                    adjustedBtceAsk = e2Orders[i].asks.price;
                    bfxLimitEntryVolume += e2Orders[i].asks.volume;
                    //we can buy up to this volume, add this volume to our max and go for this price
                }
                else
                {
                    break;
                }
            }
            //btce limit entry
            double btceLimitEntryVolume = 0; //volume of best bid at other exchange
            double adjustedBfxBid = bfx_bid;
            for (int i = 0; i < 5; i++)
            {
                double profitAtNextDepth = calculateProfit(bfx_ask, btce_ask, bfx_bid, btce_bid).Item3;//((1 - e2MarketFee) * (bfx_ask / exitRatio) - (1 + e2MarketFee) * btceDepth.ltc_usd.asks[i][0]) + bfx_ask - (1 + bfxLimitFee) * bfx_ask;
                if (profitAtNextDepth > 0)
                {
                    adjustedBfxBid = bfxOrders[i].bids.price;
                    btceLimitEntryVolume += bfxOrders[i].bids.volume;
                    //we can buy up to this volume, add this volume to our max and go for this price
                }
                else
                {
                    break;
                }
            }
            //market entry volume
            double marketEntryVolume = 0; //volume of best bid at other exchange
            if (bfxLimitEntryVolume > 0 && btceLimitEntryVolume > 0)
            {
                double nextBfxBidVol = bfxOrders[0].bids.volume;
                double nextBtceAskVol = e2Orders[0].asks.volume;
                double nextBfxBidPrice = bfxOrders[0].bids.price;
                double nextBtceAskPrice = e2Orders[0].asks.price;
                int bfxLastIndex = 0;
                int btceLastIndex = 0;
                while (bfxLastIndex < 4 && btceLastIndex < 4)
                {
                    double profitAtNextDepth = calculateProfit(bfx_ask, nextBtceAskPrice, nextBfxBidPrice).Item2;//((1 - e2MarketFee) * (bfx_ask / exitRatio) - (1 + e2MarketFee) * btceDepth.ltc_usd.asks[i][0]) + bfx_ask - (1 + bfxLimitFee) * bfx_ask;
                    if (profitAtNextDepth > 0)
                    {
                        adjustedBtceAsk = nextBtceAskPrice;
                        adjustedBfxBid = nextBfxBidPrice;
                        double minVol = Math.Min(nextBfxBidVol, nextBtceAskVol);
                        marketEntryVolume += minVol;
                        nextBfxBidVol -= minVol;
                        nextBtceAskVol -= minVol;
                        if (nextBfxBidVol == 0)
                        {
                            adjustedBfxBid = nextBfxBidPrice;
                            bfxLastIndex++;
                            nextBfxBidVol = bfxOrders[bfxLastIndex].bids.volume;
                            nextBfxBidPrice = bfxOrders[bfxLastIndex].bids.price;
                        }
                        else
                        {
                            //nextbtceask<0
                            adjustedBtceAsk = nextBtceAskPrice;
                            btceLastIndex++;
                            nextBtceAskVol = e2Orders[btceLastIndex].asks.volume;
                            nextBtceAskPrice = e2Orders[btceLastIndex].asks.price;
                        }
                        //we can buy up to this volume, add this volume to our max and go for this price
                    }
                    else
                    {
                        break;
                    }
                }
            }
            //bfx limit exit volume
            double bfxLimitExitVolume = 0; //volume of best bid at other exchange
            double adjustedBtceBid = btce_bid;
            for (int i = 0; i < 5; i++)
            {
                double exitRatioAtNextDepth = bfx_bid / e2Orders[i].bids.price;
                if (exitRatioAtNextDepth < exitRatio)
                {
                    adjustedBtceBid = e2Orders[i].bids.price;
                    bfxLimitExitVolume += e2Orders[i].bids.volume;
                    //we can buy up to this volume, add this volume to our max and go for this price
                }
                else
                {
                    break;
                }
            }
            //btce limit exit volume
            double btceLimitExitVolume = 0; //volume of best bid at other exchange
            double adjustedBfxAsk = bfx_ask;
            for (int i = 0; i < 5; i++)
            {
                double exitRatioAtNextDepth = bfx_ask / e2Orders[i].bids.price;
                if (exitRatioAtNextDepth < exitRatio)
                {
                    //adjustedBfxAsk = btceDepth.ltc_usd.asks[i][
                    //btce_bid = btceDepth.ltc_usd.asks[i][0];
                    btceLimitExitVolume += bfxOrders[i].asks.volume;
                    //we can buy up to this volume, add this volume to our max and go for this price
                }
                else
                {
                    break;
                }
            }
            //market exit volume
            double marketExitVolume = 0; //volume of best bid at other exchange
            if (bfxLimitExitVolume > 0 && btceLimitExitVolume > 0)
            {
                double nextBfxAskVol = bfxOrders[0].asks.volume;
                double nextBtceBidVol = e2Orders[0].bids.volume;
                double nextBfxAskPrice = bfxOrders[0].asks.price;
                double nextBtceBidPrice = e2Orders[0].bids.price;
                int bfxLastIndex = 0;
                int btceLastIndex = 0;
                while (bfxLastIndex < 4 && btceLastIndex < 4)
                {
                    double ratioAtNextDepth = nextBfxAskPrice / nextBtceBidPrice;
                    if (ratioAtNextDepth < exitRatio)
                    {
                        //btce_ask = btceDepth.ltc_usd.asks[i][0];
                        double minVol = Math.Min(nextBfxAskVol, nextBtceBidVol);
                        marketExitVolume += minVol;
                        nextBfxAskVol -= minVol;
                        nextBtceBidVol -= minVol;
                        if (nextBfxAskVol == 0)
                        {
                            adjustedBfxAsk = nextBfxAskPrice;
                            bfxLastIndex++;
                            nextBfxAskVol = bfxOrders[bfxLastIndex].asks.volume;
                            nextBfxAskPrice = bfxOrders[bfxLastIndex].asks.price;
                        }
                        else
                        {
                            //nextBtceBidVol==0
                            adjustedBtceBid = nextBtceBidPrice;
                            btceLastIndex++;
                            nextBtceBidVol = e2Orders[btceLastIndex].bids.volume;
                            nextBfxAskPrice = e2Orders[btceLastIndex].bids.price;
                        }
                        //we can buy up to this volume, add this volume to our max and go for this price
                    }
                    else
                    {
                        break;
                    }
                }
            }

            #endregion
            MarketOverview mkt = new MarketOverview();
            mkt.prices = new MarketOverview.Prices();
            mkt.entry = new MarketOverview.Entry();
            mkt.exit = new MarketOverview.Exit();

            mkt.prices.bfx_bid = bfxOrders[0].bids.price;
            mkt.prices.bfx_ask = bfxOrders[0].asks.price;
            mkt.prices.btce_bid = e2Orders[0].bids.price;
            mkt.prices.btce_ask = e2Orders[0].asks.price;
            mkt.entry.bfxLimitEntryVolume = bfxLimitEntryVolume;
            mkt.exit.bfxLimitExitVolume = bfxLimitExitVolume;
            mkt.entry.marketEntryVolume = marketEntryVolume;
            mkt.entry.btceLimitEntryVolume = btceLimitEntryVolume;
            mkt.exit.marketExitVolume = marketExitVolume;
            mkt.prices.bfx_ask_adjusted = adjustedBfxAsk;
            mkt.prices.bfx_bid_adjusted = adjustedBfxBid;
            mkt.prices.btce_ask_adjusted = adjustedBtceAsk;
            mkt.prices.btce_bid_adjusted = adjustedBtceBid;
            return (mkt);
        }
        static void Main(string[] args)
        {
            #region local variable init
            bool firstRun = true;
        restart:
            bool fullyInvested = false;
            bool bfxLimitCloseOrderPlaced = false;
            bool btceLimitCloseOrderPlaced = false;
            bool bfxLimitEntryPlaced = false;
            bool btceLimitEntryPlaced = false;
            double netBfx = 0;
            double netBtce = 0;
            double bfxLimitBidPrice = 0;
            double btceLimitAskPrice = 0;
            double btceLimitBidPrice = 0;
            double btceLimitEntryVolume = 0;
            int bfx_entry_orderid = 0;
            int bfx_exit_orderid = 0;
            int btce_entry_orderid = 0;
            double bfxAskLimit = 0;
            double btceBidLimit = 0;
            double btceMarketActionPrice = 0;
            double bfxMarketActionPrice = 0;
            double btceMarketExitActionPrice = 0;
            double LMT_BFX = 0;
            double LMT_BFX2 = 0;
            double lastNetBtce = 0;
            double targetBtceAmtThisOrder = 0;
            const string exchange2 = "btcchina";
            string crossCName;

            if (exchange2 == "btc-e")
                crossCName = "ltc_usd";

            else if (exchange2 == "btcchina")
                crossCName = "LTCCNY";

            #endregion
            Console.WriteLine("Press enter to start");
            Console.ReadLine();
            Console.WriteLine("running live..");
            while (true)
            {
                MarketOverview marketData = new MarketOverview();
            reReq:
                try
                {
                    marketData = getMarketData(exchange2);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(1000);
                    goto reReq;
                }
                if (!firstRun)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        ClearCurrentConsoleLine();
                    }
                }
                else
                {
                    Console.Clear();
                    firstRun = false;
                }
                ///////////////////////////////////////////
                //        Initial order placement        //
                ///////////////////////////////////////////
                //if (false)
                //{
                exitRatio = exitRatio;
                #region Entry Orders
                if (!fullyInvested && !bfxLimitCloseOrderPlaced && !btceLimitCloseOrderPlaced)//check if we should place orders
                {
                    if (marketData.entry.bfxLimitEntryVolume > 0.1 && !bfxLimitEntryPlaced && marketData.entry.marketEntryVolume <= 0.1)//we can place a limit order to sell at bfx
                    {
                        bfxAskLimit = marketData.prices.bfx_ask;
                        bfx_entry_orderid = ApiCall.placeOrder("bitfinex", Math.Min(Math.Min((maxInvest + netBfx), investAmt), marketData.entry.bfxLimitEntryVolume), marketData.prices.bfx_ask, "ltcusd", "sell", "limit");
                        bfxLimitEntryPlaced = true;
                        btceMarketActionPrice = marketData.prices.btce_ask_adjusted;
                        LMT_BFX = 0;
                    }
                    #region check bfx limit entry status
                    else if (bfxLimitEntryPlaced)        //check our orders status
                    {
                        Tuple<double, double> status = ApiCall.orderStatus("bitfinex", bfx_entry_orderid, 0);
                        double amtExec = status.Item2;
                        double amtRem = status.Item1;
                        //continuously update netBfxHolding
                        netBfx -= amtExec - LMT_BFX;
                        LMT_BFX = amtExec;
                        if (netBfx + netBtce < -0.1)       // some of our order executed, match on the other exchange
                        {
                            double matchingOrderVol = -1 * (netBfx + netBtce);
                            if (btceLimitEntryPlaced)
                            {
                                btceLimitEntryPlaced = false;
                                string cancelAttempt = ApiCall.cancelOrder(exchange2, btce_entry_orderid); //cancel our other limit order
                                //cancelAttempt = cancelAttempt.Replace("return", "ret");
                                //dynamic jds = JObject.Parse(cancelAttempt);
                                //if (jds.success == 0)
                                //{//order is fully processed
                                //    matchingOrderVol = 0;
                                //    netLongBtce += btceLimitEntryVolume;
                                //}
                                //else if (jds.ret.funds.ltc > lastBtceAmtExec)
                                //{//partial execution
                                //    matchingOrderVol -= double.Parse(jds.ret.funds.ltc.ToString()) - lastBtceAmtExec;
                                //}
                            }
                            Console.WriteLine("bitfinex limit order processed some more");
                            int success = ApiCall.placeOrder(exchange2, matchingOrderVol, btceMarketActionPrice, crossCName, "buy"); //match our order on the other exchange
                            //need to verify the order!
                            if (success != -1)
                            {
                                netBtce += matchingOrderVol;
                                lastNetBtce += matchingOrderVol;
                            }
                            if (amtRem < 0.01)
                            {
                                Console.WriteLine("Order part executed fully!");
                                bfxLimitEntryPlaced = false; //order executed in full
                                if (maxInvest + netBfx <= 0.1)
                                {
                                    fullyInvested = true;
                                    Console.WriteLine("Entry done");
                                }
                            }
                        }

                        else if ((marketData.entry.marketEntryVolume > 0.1 || marketData.entry.bfxLimitEntryVolume < amtRem || marketData.entry.bfxLimitEntryVolume <= 0.1) && bfxLimitEntryPlaced)
                        {
                            //we can act on market or we are no longer profitable
                            ApiCall.cancelOrder("bitfinex", bfx_entry_orderid);
                            bfxLimitEntryPlaced = false;
                        }
                        else if (bfxAskLimit > marketData.prices.bfx_ask && marketData.entry.bfxLimitEntryVolume > 0.1 && bfxLimitEntryPlaced && amtRem > 0.1)
                        {
                            bfx_entry_orderid = ApiCall.replaceOrder("bitfinex", bfx_entry_orderid, Math.Min(amtRem, marketData.entry.bfxLimitEntryVolume), marketData.prices.bfx_ask, "ltcusd", "sell", "limit");
                            bfxAskLimit = marketData.prices.bfx_ask;
                            btceMarketActionPrice = marketData.prices.btce_ask_adjusted;
                            LMT_BFX = 0;
                            amtExec = 0;
                        }
                    }
                    #endregion
                    if (!fullyInvested && marketData.entry.btceLimitEntryVolume > 0.1 && !btceLimitEntryPlaced && maxInvest - netBtce > 0.1 && marketData.entry.marketEntryVolume <= 0.1)//place a btce limit order. note that we can have simultanious bfx/btce entry orders
                    {
                        btceLimitBidPrice = marketData.prices.btce_bid;
                        btceLimitEntryVolume = Math.Min(Math.Min(Math.Round(maxInvest - netBtce, 4), investAmt), marketData.entry.btceLimitEntryVolume);
                        btce_entry_orderid = ApiCall.placeOrder(exchange2, btceLimitEntryVolume, marketData.prices.btce_bid, crossCName, "buy");
                        btceLimitEntryPlaced = true;
                        btceBidLimit = marketData.prices.btce_bid;
                        bfxMarketActionPrice = marketData.prices.bfx_bid_adjusted;
                        targetBtceAmtThisOrder = netBtce + btceLimitEntryVolume;
                        //control variables
                    }
                    #region check btce limit entry status
                    else if (btceLimitEntryPlaced)
                    {
                        ApiCall.AccountInformation act = ApiCall.getAccountInfo(exchange2);
                        double amtExec = act.ltcAmount;
                        netBtce = amtExec;
                        //double amtRem = (netBtceHolding + btceLimitEntryVolume) - amtExec;
                        if (netBtce + netBfx > 0.1)
                        {
                            double matchingOrderVol = netBtce + netBfx;
                            if (bfxLimitEntryPlaced)
                            {
                                //we have to cancel the other order
                                string cancelAttempt = ApiCall.cancelOrder("bitfinex", bfx_entry_orderid); //cancelorder returns amount executed
                                bfxLimitEntryPlaced = false;
                                //dynamic jds = JObject.Parse(cancelAttempt); this is more complicated than i thought
                                //if (jds.executed_amount > lastBfxAmtExec)
                                //{
                                //    matchingOrderVol -= double.Parse(jds.executed_amount.ToString()) - lastBfxAmtExec;
                                //}
                            }
                            Console.WriteLine("btce limit order processed some more: " + matchingOrderVol);
                            ApiCall.placeOrder("bitfinex", matchingOrderVol, bfxMarketActionPrice, "ltcusd", "sell", "limit");
                            netBfx -= matchingOrderVol;
                            if (targetBtceAmtThisOrder - amtExec <= 0.1)
                            {
                                btceLimitEntryPlaced = false; //order executed in full
                                Console.WriteLine("Order part executed fully!");
                                if (maxInvest - netBtce <= 0.1)
                                {
                                    fullyInvested = true;
                                    Console.WriteLine("Entry done");
                                }
                            }
                        }
                        if (btceLimitEntryPlaced && (marketData.entry.marketEntryVolume > 0.1 || marketData.entry.btceLimitEntryVolume < targetBtceAmtThisOrder - amtExec || marketData.entry.btceLimitEntryVolume <= 0.1))
                        {
                            //cancel the limit to act on market or because it is not profitable
                            ApiCall.cancelOrder(exchange2, btce_entry_orderid);
                            btceLimitEntryPlaced = false;
                        }
                        else if (btceBidLimit < marketData.prices.btce_bid && marketData.entry.btceLimitEntryVolume > 0.1 && btceLimitEntryPlaced)
                        {
                            double vol = Math.Min(Math.Min(maxInvest - amtExec, marketData.entry.btceLimitEntryVolume), investAmt);
                            btce_entry_orderid = ApiCall.replaceOrder(exchange2, btce_entry_orderid, vol, marketData.prices.btce_bid, crossCName, "buy");
                            btceBidLimit = marketData.prices.btce_bid;
                            bfxMarketActionPrice = marketData.prices.bfx_bid_adjusted;
                            targetBtceAmtThisOrder = amtExec + vol;
                        }
                    }
                    #endregion
                    if (!fullyInvested && marketData.entry.marketEntryVolume > 0.1 && !bfxLimitEntryPlaced && !btceLimitEntryPlaced && netBtce < maxInvest)//see if we should place a market order to enter
                    {
                        //other orders should already be canceled
                        bfx_entry_orderid = ApiCall.placeOrder("bitfinex", Math.Min(Math.Min((maxInvest + netBfx), investAmt), marketData.entry.bfxLimitEntryVolume), marketData.prices.bfx_bid_adjusted, "ltcusd", "sell", "limit");
                        btceMarketActionPrice = marketData.prices.btce_ask_adjusted;
                        bfxLimitEntryPlaced = true;
                        LMT_BFX = 0;
                    }
                }
                #endregion
                ///////////////////////////////////////////
                //        Initial orders Executed        //
                ///////////////////////////////////////////
                #region Exit orders
                if (netBtce > 0 && !bfxLimitEntryPlaced && !btceLimitEntryPlaced)
                {
                    if (marketData.exit.bfxLimitExitVolume > 0.1 && marketData.exit.marketExitVolume < 0.1 && !bfxLimitCloseOrderPlaced && netBfx < -0.1)
                    {
                        //we should enter into a bfx limit exit position
                        bfxLimitCloseOrderPlaced = true;
                        bfx_exit_orderid = ApiCall.placeOrder("bitfinex", Math.Min(Math.Min(-1 * netBfx, investAmt), marketData.exit.bfxLimitExitVolume), marketData.prices.bfx_bid, "ltcusd", "buy", "limit");
                        bfxLimitBidPrice = marketData.prices.bfx_bid;
                        btceLimitAskPrice = marketData.prices.btce_bid;
                        btceMarketExitActionPrice = marketData.prices.btce_bid_adjusted;
                        LMT_BFX2 = 0;
                    }
                    #region check bfx exit order
                    else if (bfxLimitCloseOrderPlaced)
                    {
                        //check progress of our secondary limit orders
                        Tuple<double, double> status = ApiCall.orderStatus("bitfinex", bfx_exit_orderid);
                        double amtExec = status.Item2;
                        double bfxRemAmt = status.Item1;

                        netBfx += amtExec - LMT_BFX2;//continuously update
                        LMT_BFX2 = amtExec;
                        if (netBfx + netBtce > 0.1)  //our bfx order is filled some more, place a matching buy order on btce
                        {
                            ApiCall.AccountInformation btce = ApiCall.getAccountInfo(exchange2);
                            double orderVol = Math.Min(Math.Round((netBtce + netBfx), 3), btce.ltcAmount);
                            Console.WriteLine(ApiCall.placeOrder(exchange2, orderVol, btceMarketExitActionPrice, crossCName, "sell"));
                            Console.WriteLine("Bitfinex limit close order filled some more!");
                            netBtce -= orderVol;
                            if (bfxRemAmt == 0)
                            {            //this order has completed
                                bfxLimitCloseOrderPlaced = false;
                            }
                            if (netBfx >= -0.01)
                            {
                                //our entire position was closed, restart.
                                bfxLimitCloseOrderPlaced = false;
                                //have to double check that bfx position is 100% closed
                                goto restart;
                            }
                        }
                        else if (marketData.exit.bfxLimitExitVolume <= 0.1 || marketData.exit.marketExitVolume > 0.1)
                        {
                            //the best bid is no longer profitable OR we can act on the market
                            ApiCall.cancelOrder("bitfinex", bfx_exit_orderid);
                            bfxLimitCloseOrderPlaced = false;
                        }
                        else if (marketData.prices.bfx_bid > bfxLimitBidPrice && marketData.exit.bfxLimitExitVolume > 0.1)
                        {
                            bfx_exit_orderid = ApiCall.replaceOrder("bitfinex", bfx_exit_orderid, Math.Min(bfxRemAmt, marketData.exit.bfxLimitExitVolume), marketData.prices.bfx_bid, "ltcusd", "buy", "limit");
                            bfxLimitBidPrice = marketData.prices.bfx_bid;
                            btceMarketExitActionPrice = marketData.prices.btce_bid_adjusted;
                            amtExec = 0;
                            LMT_BFX2 = 0;
                        }
                        btceLimitAskPrice = marketData.prices.btce_bid;
                    }
                    #endregion
                    #region check btce close order
                    //if (btceLimitCloseOrderPlaced)
                    //{
                    //    //check progress of our secondary limit orders
                    //    string a = Query.btceQuery("&method=ActiveOrders&pair=ltc_usd");
                    //    a = a.Replace("return", "ret").Replace(btce_exit_orderid.ToString(), "order");
                    //    dynamic dynamos = JObject.Parse(a);
                    //    double amtRem = double.Parse(dynamos.ret.order.amount.ToString());
                    //    double amtExec = btceLimitExitVolume - amtRem;
                    //    if (amtExec > lastAmtSoldBtce)
                    //    {
                    //        //our bfx order is filled some more, place a matching buy order on bfx
                    //        Console.WriteLine(ApiCall.placeOrder("bitfinex", Math.Round((amtExec - lastAmtSoldBtce), 3), marketData.prices.bfx_ask, "ltcusd", "buy"));
                    //        netLongBfx = amtExec;
                    //        Console.WriteLine("Bitfinex limit close order filled some more!");
                    //        netLongBfx += amtExec - lastAmtBoughtBfx;
                    //    }
                    //    if (marketData.prices.btce_ask / marketData.prices.btce_ask > exitRatio)
                    //    {
                    //        //cancel our bfx limit buy order.
                    //        ApiCall.cancelOrder("btc-e", btce_exit_orderid);
                    //        Console.WriteLine("Canceled bfx limit order");
                    //        bfxLimitCloseOrderPlaced = false;
                    //    }
                    //    //else if (marketData.prices.bfx_bid > bfxLimitBidPrice && marketData.prices.bfx_bid / marketData.prices.btce_bid < exitRatio && !waitingOnBtceSell)
                    //    //{
                    //    //    Console.WriteLine("lowering our btce ask");
                    //    //    cancelOrder("bitfinex", bfx_exit_orderid);
                    //    //    //bfxnparam++;
                    //    //    string b = placeOrder(bfxRemAmt, marketData.prices.bfx_bid, "ltcusd", "bitfinex", "buy", "limit");
                    //    //    bfx_exit_orderid = JsonConvert.DeserializeObject<bitfinex_json.orderInfo>(b).order_id;
                    //    //    Console.WriteLine("placed limit buy order to close bfx position");
                    //    //    bfxLimitBidPrice = marketData.prices.bfx_bid;
                    //    //    btceLimitAskPrice = marketData.prices.btce_bid;
                    //    //}
                    //    lastAmtSoldBtce = amtExec;
                    //}
                    #endregion
                    #region market exit
                    if (marketData.exit.marketExitVolume > 0.1 && !bfxLimitCloseOrderPlaced && !btceLimitCloseOrderPlaced && netBfx < 0)//we can close our secondary positions at market
                    {
                        //cancel outstanding close orders - should already be done
                        ApiCall.AccountInformation btce = ApiCall.getAccountInfo(exchange2);
                        if (btce.ltcAmount > 0)
                        {
                            double bfxExitedVol = Math.Min(-1 * netBfx, marketData.exit.marketExitVolume);
                            double btceExitedVol = Math.Min(btce.ltcAmount, marketData.exit.marketExitVolume);
                            double exitVol = Math.Min(bfxExitedVol, btceExitedVol);
                            ApiCall.placeOrder("bitfinex", bfxExitedVol, marketData.prices.bfx_ask_adjusted, "ltcusd", "buy", "limit");
                            ApiCall.placeOrder(exchange2, btceExitedVol, marketData.prices.btce_bid_adjusted, crossCName, "sell");
                            Console.WriteLine("Exited " + bfxExitedVol + " at BFX, " + btceExitedVol + " at BTCE");
                            netBtce -= exitVol;
                            netBfx += exitVol;
                            if (netBfx >= -0.05)
                            {
                                //double check to make sure we have NO bfx position!
                                //closeAllOrders();//claim anything remaining at market
                                goto restart;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No ltc in btc-e wallet yet.");
                        }
                    }
                    #endregion
                    //else if (marketData.prices.bfx_ask / marketData.prices.btce_ask < exitRatio && btceLimitCloseOrderPlaced == false)//if a sell order on btce executes at the best ask and we can immedlatly buy on bfx
                    //{
                    //    Console.WriteLine("placing btce sell limit order");
                    //    btceLimitCloseOrderPlaced = true;
                    //    accountInformation acctInfo = getAccountInfo("btc-e");
                    //    btceLimitExitVolume = Math.Min(Math.Min(netShortBfx - netLongBfx, investAmt), acctInfo.ltcAmount);
                    //    string a = placeOrder(btceLimitExitVolume, marketData.prices.btce_ask, "ltc_usd", "btc-e", "sell");
                    //    a = a.Replace("return", "ret");
                    //    dynamic jOb = JObject.Parse(a);
                    //    btce_exit_orderid = Int32.Parse(jOb.ret.order_id.ToString());
                    //    Console.WriteLine("exit order placed at btce limit");
                    //    btceLimitAskPrice = marketData.prices.btce_bid;
                    //}
                #endregion
                }
                //}
                Console.WriteLine("#####################################################");
                Console.WriteLine("Volume to enter BFX limit at: " + marketData.entry.bfxLimitEntryVolume + " BTCE: " + marketData.entry.btceLimitEntryVolume);
                Console.WriteLine("Volume to enter market at: " + marketData.entry.marketEntryVolume);
                Console.WriteLine("Volume to exit BFX limit at: " + marketData.exit.bfxLimitExitVolume + " BTCE: " + marketData.exit.bfxLimitExitVolume);
                Console.WriteLine("Volume to exit market at: " + marketData.exit.marketExitVolume);
                Console.WriteLine("cross exchange ratio: " + marketData.prices.bfx_ask / marketData.prices.btce_bid);//+ (profitAtThisLevel+exitRatio) +"  " +  (profitAtThisLevelMkt+exitRatio));
                Console.WriteLine("###### " + netBfx + "   ######   " + netBtce + " ######");
                System.Threading.Thread.Sleep(1000);
            }
        }
        static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }
}