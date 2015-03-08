using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using bitfinex_pairs;

namespace X_Exchange_StatArb
{
    class ApiCall
    {
        public class AccountInformation
        {
            public int openOrders { get; set; }
            public double btcAmount { get; set; }
            public double ltcAmount { get; set; }
            public double usdAmount { get; set; }
        }
        public static bool lengthGood(double test, int precision)
        {
            double x = test * Math.Pow(10, precision);
            return (Math.Truncate(x) == x);
        }
        public static int placeOrder(string exchange, double volume, double price, string pair, string orderType, string marketLimit = "limit")
        {
            int orderId;
            //check length
            try
            {
                if (!lengthGood(volume, 5))
                {
                    double x = Math.Pow(10, 5);
                    volume = Math.Truncate(x * volume) / x;
                }
                if (volume < 0.1 && exchange != "btcchina")
                {
                    Console.WriteLine("Attempting to pass an order of volume < 0.1!");
                    Console.WriteLine(marketLimit + " " + orderType + " FAILED at " + exchange);
                    return (-1);
                }
                if (exchange == "btc-e")
                {
                    string resp = Query.btceQuery("&method=Trade&pair=" + pair + "&type=" + orderType + "&rate=" + price + "&amount=" + volume);
                    resp = resp.Replace("return", "ret");
                    dynamic jOb = JObject.Parse(resp);
                    orderId = Int32.Parse(jOb.ret.order_id.ToString());//0 indicates order was immediatly executed
                }
                else if (exchange == "bitfinex")
                {
                    UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
                    string resp = Query.bitfinexQuery("order/new", true, @"{""request"":""/v1/order/new"", ""nonce"":""" + unixTimestamp + @""", ""symbol"":""" + pair + @""", ""amount"":""" + volume + @""", ""price"":""" + price + @""", ""side"":""" + orderType + @""", ""exchange"":""bitfinex"", ""type"":""" + marketLimit + @"""}");
                    orderId = JsonConvert.DeserializeObject<bitfinex_json.orderInfo>(resp).order_id;
                }
                else if (exchange == "btcchina")
                {
                    if (orderType == "buy")
                    {
                        string resp = Query.btcchinaQuery("buyOrder2", (price + "," + volume + "," + "\"" + pair + "\""));
                        dynamic jOb = JObject.Parse(resp);
                        orderId = Int32.Parse(jOb.result.ToString());
                    }
                    else
                    {
                        string resp = Query.btcchinaQuery("sellOrder2", (price + "," + volume + "," + "\"" + pair + "\""));
                        dynamic jOb = JObject.Parse(resp);
                        orderId = Int32.Parse(jOb.result.ToString());
                    }
                }
                else if (exchange == "kraken")
                {
                    orderId = 1;
                }
                else if (exchange == "hitbtc")
                {
                    orderId = 1;
                }
                else if (exchange == "okcoin")
                {
                    orderId = 1;
                }
                else
                {
                    orderId = -1;
                }
            }
            catch (JsonReaderException)
            {
                return(0);
            }
            Console.WriteLine(marketLimit + " " + orderType + " order #" + orderId + " placed at " + exchange);
            return (orderId);
        }
        public static string cancelOrder(string exchange, double orderID)
        {
            //returns json response from server
            string ret;
            if (exchange == "bitfinex")
            {
                UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
                ret = Query.bitfinexQuery("order/cancel", true, @"{""request"":""/v1/order/cancel"", ""nonce"":""" + (unixTimestamp) + @""", ""order_id"":" + orderID + @"}");
            }
            else if (exchange == "btc-e")
            {
                ret = Query.btceQuery("&method=CancelOrder&order_id=" + orderID);
            }
            else if (exchange == "btcchina")
            {
                ret = Query.btcchinaQuery("cancelOrder", (orderID + "," + @"""LTCCNY"""));
            }
            else if (exchange == "kraken")
            {
                ret = "";
            }
            else if (exchange == "hitbtc")
            {
                ret = "";
            }
            else
            {
                ret = "Error: Invalid exchange " + exchange;
            }
            Console.WriteLine(exchange + " # " + orderID + " order canceled");
            return (ret);
        }
        public static Tuple<double, double> orderStatus(string exchange, int orderID, double initialAmount = 0, double btceNetLong = 0)
        {
        //returns the amount executed of the order. returns -2 if the exchange is invalid, -1 if the order is not found
        a:
            try
            {
                double executedAmount;
                double remainingAmount;
                if (exchange == "bitfinex")
                {
                    UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
                    string a = Query.bitfinexQuery("order/status", true, @"{""request"":""/v1/order/status"", ""nonce"":""" + unixTimestamp + @""", ""order_id"":" + orderID + "}");
                    executedAmount = Double.Parse(JsonConvert.DeserializeObject<bitfinex_json.orderStatus>(a).executed_amount);
                    remainingAmount = Double.Parse(JsonConvert.DeserializeObject<bitfinex_json.orderStatus>(a).remaining_amount);
                }
                else if (exchange == "btc-e")
                {
                    //netlong is the last amt. of ltc we were holding on btce.
                    AccountInformation act = getAccountInfo("btc-e");
                    remainingAmount = initialAmount - (act.ltcAmount - btceNetLong);
                    executedAmount = act.ltcAmount - btceNetLong;
                }
                else if (exchange == "btcchina")
                {
                    dynamic j0b = JObject.Parse(Query.btcchinaQuery("getAccountInfo", @"""balance"""));
                    Console.WriteLine(j0b.result.balance.ltc.amount);
                    double x = j0b.result.balance.ltc.amount;
                    remainingAmount = initialAmount - (x - btceNetLong);
                    executedAmount = x - btceNetLong;
                }
                else if (exchange == "kraken")
                {
                    remainingAmount = -2;
                    executedAmount = -2;
                }
                else if (exchange == "hitbtc")
                {
                    remainingAmount = -2;
                    executedAmount = -2;
                }
                else    //the exchange is invalid
                {
                    remainingAmount = -2;
                    executedAmount = -2;
                }


                return (new Tuple<double, double>(remainingAmount, executedAmount));
            }
            catch (JsonReaderException)
            {
                goto a;
            }
        }

        public static AccountInformation getAccountInfo(string exchange)
        {
            //returns our current ltc, btc, yen/eur/usd inventory
            AccountInformation getInfo = new AccountInformation();
            if (exchange == "btc-e")
            {
                string accountInfoResp = Query.btceQuery("&method=getInfo");
                btce_json.infoRoot actInfoParsed = JsonConvert.DeserializeObject<btce_json.infoRoot>(accountInfoResp);
                getInfo.btcAmount = actInfoParsed.@return.funds.btc;
                getInfo.ltcAmount = actInfoParsed.@return.funds.ltc;
                getInfo.usdAmount = actInfoParsed.@return.funds.usd;
                getInfo.openOrders = actInfoParsed.@return.open_orders;
            }
            else if (exchange == "bitfinex")
            {
                string accountInfoResp = Query.bitfinexQuery("balances", false);
                List<bitfinex_json.infoRoot> actInfoParsed = JsonConvert.DeserializeObject<List<bitfinex_json.infoRoot>>(accountInfoResp);
                getInfo.btcAmount = double.Parse(actInfoParsed[0].available);
                getInfo.btcAmount = double.Parse(actInfoParsed[1].available);
                getInfo.usdAmount = double.Parse(actInfoParsed[2].available);
            }
            else if (exchange == "btcchina")
            {
                string a = Query.btcchinaQuery("getAccountInfo");
                dynamic j0b = JObject.Parse(a);
                getInfo.btcAmount = j0b.result.balance.btc.amount;
                getInfo.ltcAmount = j0b.result.balance.ltc.amount;
                getInfo.usdAmount = j0b.result.balance.cny.amount;
            }
            else
            {
                //string accountInfoResp = "Invalid exchange";
            }
            return (getInfo);
        }
        public static void closeAllOrders()
        {
            AccountInformation btceCheck = getAccountInfo("btc-e");
            if (btceCheck.btcAmount > 0)
            {

            }
            //Ensure that our orders are actually canceled
            string bfxCheck = Query.bitfinexQuery("positions", false);

            //  Console.WriteLine(resp);
            dynamic activePositions = JObject.Parse(@"{""id"":209523,""symbol"":""btcusd"",""status"":""ACTIVE"",""base"":""4.2472"",""amount"":""-0.1"",""timestamp"":""1411398420.0"",""swap"":""0.0"",""pl"":""-0.0025328""}");
            UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
            string resp2 = Query.bitfinexQuery("position/claim", true, @"{""request"":""/v1/position/claim"", ""nonce"":""" + unixTimestamp + @""", ""position_id"":""" + activePositions[0].id + "}");
            Console.WriteLine(resp2);
        }
        public static int replaceOrder(string exchange, int orderid, double volume, double price, string pair, string orderType, string marketLimit = "limit")
        {
            int newOrderId;
            if (exchange == "bitfinex")
            {
                Console.WriteLine("Replacing bitfinex order #" + orderid);
                UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
                string resp = Query.bitfinexQuery("order/cancel/replace", true, @"{""request"":""/v1/order/cancel/replace"", ""nonce"":""" + unixTimestamp + @""", ""order_id"":" + orderid + @", ""symbol"":""" + pair + @""", ""amount"":""" + volume + @""", ""price"":""" + price + @""", ""side"":""" + orderType + @""", ""exchange"":""bitfinex"", ""type"":""" + marketLimit + @"""}");
                newOrderId = JsonConvert.DeserializeObject<bitfinex_json.orderInfo>(resp).order_id;
            }
            else if (exchange == "btc-e")
            {
                Console.WriteLine("Replacing btc-e order #" + orderid);
                cancelOrder(exchange, orderid);
                newOrderId = placeOrder(exchange, volume, price, pair, orderType, marketLimit);
            }
            else if (exchange == "btcchina")
            {
                Console.WriteLine("Replacing btcchina order #" + orderid);
                cancelOrder(exchange, orderid);
                newOrderId = placeOrder(exchange, volume, price, pair, orderType, marketLimit);
            }
            else
            {
                Console.WriteLine("Invalid exchange");
                newOrderId = 0;//what the hell are you doing
            }
            return (newOrderId);
        }
        public static double getPositions()
        {
            //returns vol. we are holding
            string bfxCheck = Query.bitfinexQuery("positions", false);
            bfxCheck = bfxCheck.TrimStart('[').TrimEnd(']');
            try
            {
                dynamic parser = JObject.Parse(bfxCheck);
                return (parser.amount);
            }
            catch (Exception)
            {//there are no positions - everything is executed
                return (0);
            }
        }
    }
}

