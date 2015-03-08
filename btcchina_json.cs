using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class btcchina_json
    {
        public class Ticker
        {
            public double high { get; set; }
            public double low { get; set; }
            public double buy { get; set; }
            public double sell { get; set; }
            public double last { get; set; }
            public double vol { get; set; }
            public int date { get; set; }
        }
        public class Depth
        {
            public List<List<double>> asks { get; set; }
            public List<List<double>> bids { get; set; }
            public int date { get; set; }
        }
        public class RootObject
        {
            public Ticker ticker { get; set; }
        }
    }
}
