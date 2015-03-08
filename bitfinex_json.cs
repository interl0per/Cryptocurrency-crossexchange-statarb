using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class bitfinex_json
    {

        public class Bid
        {
            public double price { get; set; }
            public double amount { get; set; }
            public string timestamp { get; set; }
        }

        public class Ask
        {
            public double price { get; set; }
            public double amount { get; set; }
            public string timestamp { get; set; }
        }

        public class Depth
        {
            public List<Bid> bids { get; set; }
            public List<Ask> asks { get; set; }
        }
        public class RootObject
        {
            public double mid { get; set; }
            public double bid { get; set; }
            public double ask { get; set; }
            public double last_price { get; set; }
            public double low { get; set; }
            public double high { get; set; }
            public double volume { get; set; }
            public string timestamp { get; set; }
        }

        public class infoRoot
        {
            public string type { get; set; }
            public string currency { get; set; }
            public string amount { get; set; }
            public string available { get; set; }
        }
        public class orderInfo
        {
            public int id { get; set; }
            public string symbol { get; set; }
            public string exchange { get; set; }
            public string price { get; set; }
            public string avg_execution_price { get; set; }
            public string side { get; set; }
            public string type { get; set; }
            public string timestamp { get; set; }
            public bool is_live { get; set; }
            public bool is_cancelled { get; set; }
            public bool was_forced { get; set; }
            public string original_amount { get; set; }
            public string remaining_amount { get; set; }
            public string executed_amount { get; set; }
            public int order_id { get; set; }
        }
        public class orderStatus
        {
            public int id { get; set; }
            public string symbol { get; set; }
            public string exchange { get; set; }
            public string price { get; set; }
            public string avg_execution_price { get; set; }
            public string side { get; set; }
            public string type { get; set; }
            public string timestamp { get; set; }
            public bool is_live { get; set; }
            public bool is_cancelled { get; set; }
            public bool was_forced { get; set; }
            public string original_amount { get; set; }
            public string remaining_amount { get; set; }
            public string executed_amount { get; set; }
        }
    }
}
