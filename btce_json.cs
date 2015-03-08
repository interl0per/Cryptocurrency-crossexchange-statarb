using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class btce_json
    {
        public class LtcUsd
        {
            public List<List<double>> asks { get; set; }
            public List<List<double>> bids { get; set; }
        }
        public class BtcUsd
        {
            public List<List<double>> asks { get; set; }
            public List<List<double>> bids { get; set; }
        }
        public class Depth
        {
            public BtcUsd btc_usd { get; set; }
            public LtcUsd ltc_usd { get; set; }
        }
        /// <summary>
        /// account information
        /// </summary>
        public class infoRoot
        {
            public int success { get; set; }
            public information @return { get; set; }
        }

        public class information
        {
            public Funds funds { get; set; }
            public Rights rights { get; set; }
            public int transaction_count { get; set; }
            public int open_orders { get; set; }
            public int server_time { get; set; }
            public int received { get; set; }
            public int remains { get; set; }
            public int order_id { get; set; }
        }
        public class Funds
        {
            public double usd { get; set; }
            public double ltc { get; set; }
            public double btc { get; set; }
            public double nmc { get; set; }
            public double rur { get; set; }
            public double eur { get; set; }
            public double nvc { get; set; }
            public double trc { get; set; }
            public double ppc { get; set; }
            public double ftc { get; set; }
            public double xpm { get; set; }
        }

        public class Rights
        {
            public int info { get; set; }
            public int trade { get; set; }
            public int withdraw { get; set; }
        }
        /// <summary>
        /// current bid/ask information
        /// </summary>
        public class Ticker
        {
            public double high { get; set; }
            public double low { get; set; }
            public double avg { get; set; }
            public double vol { get; set; }
            public double vol_cur { get; set; }
            public double last { get; set; }
            public double buy { get; set; }
            public double sell { get; set; }
            public int updated { get; set; }
            public int server_time { get; set; }
        }
        public class RootObject
        {
            public Ticker ticker { get; set; }
        }
        public class Return2
        {
            public int received { get; set; }
            public double remains { get; set; }
            public int order_id { get; set; }
            public Funds funds { get; set; }
        }

        public class tradeRoot
        {
            public string error { get; set; }
            public int success { get; set; }
            public Return2 @return { get; set; }
        }

    }
}
