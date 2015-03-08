using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitfinex_pairs
{
    class kraken_json
    {
        public class XXBTZEUR
        {
            public List<string> a { get; set; }
            public List<string> b { get; set; }
            public List<string> c { get; set; }
            public List<string> v { get; set; }
            public List<string> p { get; set; }
            public List<int> t { get; set; }
            public List<string> l { get; set; }
            public List<string> h { get; set; }
            public string o { get; set; }
        }
        public class XLTCZEUR
        {
            public List<string> a { get; set; }
            public List<string> b { get; set; }
            public List<string> c { get; set; }
            public List<string> v { get; set; }
            public List<string> p { get; set; }
            public List<int> t { get; set; }
            public List<string> l { get; set; }
            public List<string> h { get; set; }
            public string o { get; set; }
        }
        public class Result
        {
            public XXBTZEUR XXBTZEUR { get; set; }
            public XLTCZEUR XLTCZEUR { get; set; }

        }

        public class RootObject
        {
            public List<object> error { get; set; }
            public Result result { get; set; }
        }
    }
}
