using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class hitbtc_json
    {
        public class RootObject
        {
            public string ask { get; set; }
            public string bid { get; set; }
            public string last { get; set; }
            public string low { get; set; }
            public string high { get; set; }
            public string volume { get; set; }
            public long timestamp { get; set; }
        }
    }
}
