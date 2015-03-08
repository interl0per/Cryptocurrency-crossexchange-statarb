using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class okcoin_json
    {

        public class Ticker
        {
            public string buy { get; set; }
            public string high { get; set; }
            public string last { get; set; }
            public string low { get; set; }
            public string sell { get; set; }
            public string vol { get; set; }
        }

        public class RootObject
        {
            //public string date { get; set; }
            public Ticker ticker { get; set; }
        }
    }
}
