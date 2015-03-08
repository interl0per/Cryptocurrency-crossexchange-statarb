using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.IO;

namespace X_Exchange_StatArb
{
    class Query
    {
        public static string read(WebRequest req)
        {
            try
            {
                //recieves and reads response from webpage
                var response = req.GetResponse();
                var stream = response.GetResponseStream();
                var sr = new StreamReader(stream);
                var content = sr.ReadToEnd();
                return (content);
            }
            catch (WebException)
            {
                return ("Error reading web request");
            }
        }

        public static string bitfinexQuery(string urlParams, bool bypass, string requestBody = "")
        {
            string key = "";
            string secret = "";
            HMACSHA384 hashMaker = new HMACSHA384(Encoding.UTF8.GetBytes(secret));
            UInt64 unixTimestamp = (UInt64)DateTime.Now.Ticks;
            //request url parameters
            string data = @"{""request"":""/v1/" + urlParams + @""",""nonce"":""" + unixTimestamp.ToString() + @"""}";
            if (bypass)
            {
                data = requestBody;
            }
            string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(data);
            var request = WebRequest.Create(new Uri("https://api.bitfinex.com/v1/" + urlParams)) as HttpWebRequest;
            request.Method = "POST";
            request.KeepAlive = true;
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            byte[] hashed = hashMaker.ComputeHash(Encoding.UTF8.GetBytes(payload));
            string sign = BitConverter.ToString(hashed).Replace("-", "").ToLower();
            request.Headers.Add("X-BFX-APIKEY", key);
            request.Headers.Add("X-BFX-PAYLOAD", payload);
            request.Headers.Add("X-BFX-SIGNATURE", sign);
            var reqStream = request.GetRequestStream();
            reqStream.Write(byteArray, 0, byteArray.Length);
            return (read(request));
        }

        public static UInt32 lastnonce = 0;

        public static string btceQuery(string urlParams)
        {
            string key = "";
            string secret = "";
            HMACSHA512 hashMaker = new HMACSHA512(Encoding.ASCII.GetBytes(secret));
            UInt32 unixTimestamp = (UInt32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (unixTimestamp <= lastnonce)
            {
                unixTimestamp = lastnonce + 1;
            }
            //request url parameters
            string data = "nonce=" + unixTimestamp.ToString() + urlParams;
            byte[] dataStream = Encoding.ASCII.GetBytes(data);
            //set request properties
            var request = WebRequest.Create(new Uri("https://btc-e.com/tapi")) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = dataStream.Length;
            request.Headers.Add("Key", key);
            //do crypto stuff, append header
            byte[] hashed = hashMaker.ComputeHash(dataStream);
            string sign = BitConverter.ToString(hashed);
            sign = sign.Replace("-", "");
            request.Headers.Add("Sign", sign.ToLower());
            lastnonce = unixTimestamp;
            //execute
            var reqStream = request.GetRequestStream();
            reqStream.Write(dataStream, 0, dataStream.Length);
            reqStream.Close();
            return (read(request));
        }
        public static string krakenQuery(string urlParams, string postData = null)
        {
            string uri = "https://api.kraken.com/0/private/" + urlParams;
            string key = "";
            string secret = "";
            UInt64 nonce = (UInt64)DateTime.Now.Ticks;
            //POST Data
            string data = "nonce=" + nonce.ToString() + postData;
            //set request properties
            var request = WebRequest.Create(new Uri(uri)) as HttpWebRequest;
            request.ContentLength = data.Length;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //API-Sign = Message signature using HMAC-SHA512 of (URI path + SHA256(nonce + POST data)) and base64 decoded secret API key
            byte[] base64DecodedSecret = Convert.FromBase64String(secret);
            HMACSHA512 hashMaker = new HMACSHA512(base64DecodedSecret);
            var uriPathByte = Encoding.UTF8.GetBytes("/0/private/" + urlParams);
            var np = nonce + Convert.ToChar(0) + data;
            var hashed256np = sha256_hash(np);
            var z = new byte[uriPathByte.Count() + hashed256np.Count()];
            uriPathByte.CopyTo(z, 0);
            hashed256np.CopyTo(z, uriPathByte.Count());
            var signature = hashMaker.ComputeHash(z);
            request.Headers.Add("API-Key", key);
            //do crypto stuff, append header
            request.Headers.Add("API-Sign", Convert.ToBase64String(signature));
            //execute
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(data);
            }
            return (read(request));
        }
        public static string hitbtcQuery(string method = "balance", string postData = null)
        {
            string key = "";
            string secret = "";
            long nonce = DateTime.Now.Ticks * 10 / TimeSpan.TicksPerMillisecond;
            string uri = "https://api.hitbtc.com/api/1/trading/" + method + "?nonce=" + nonce + "&apikey=" + key;

            string sig = @"/api/1/trading/" + method + "?nonce=" + nonce + "&apikey=" + key + postData;
            //set request properties
            var request = (WebRequest)HttpWebRequest.Create(uri);
            //request.ContentLength = postData.Length;
            request.Method = "GET";
            //API-Sign = Message signature using HMAC-SHA512 of (URI path + SHA256(nonce + POST data)) and base64 decoded secret API key
            //byte[] base64DecodedSecret = Convert.FromBase64String(secret);
            HMACSHA512 hashMaker = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            hashMaker.ComputeHash(Encoding.UTF8.GetBytes(sig));
            string siggy = string.Concat(hashMaker.Hash.Select(b => b.ToString("x2")).ToArray());
            request.Headers.Add("X-Signature", siggy);
            return (read(request));
        }
        public static string btcchinaQuery(string method = "getAccountInfo", string prms = "")
        {
            string postData = "{\"method\": \"" + method + "\", \"params\": [" + prms + "], \"id\": 1}";
            //Console.WriteLine(postData);
            TimeSpan timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            long milliSeconds = Convert.ToInt64(timeSpan.TotalMilliseconds * 1000);
            string tonce = Convert.ToString(milliSeconds);
            string secret = "";//secret key
            string key = "";//access key
            string sig = "tonce=" + tonce + "&accesskey=" + key + "&requestmethod=post&id=1&method=" + method + "&params=" + prms.Replace("\"", "");
            string paramsHash = GetHMACSHA1Hash(secret, sig);
            string base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(key + ':' + paramsHash));
            var request = WebRequest.Create(new Uri("https://api.btcchina.com/api_trade_v1.php")) as HttpWebRequest;
            request.Headers.Add("Json-Rpc-Tonce", tonce.ToString());
            request.Headers.Add("Authorization", "Basic " + base64String);
            request.Method = "POST";
            request.ContentLength = postData.Length;
            byte[] bytes = Encoding.ASCII.GetBytes(postData);
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(bytes, 0, bytes.Length);
                dataStream.Close();
            }
            return (read(request));
        }
        private static string GetHMACSHA1Hash(string secret_key, string input)
        {
            HMACSHA1 hmacsha1 = new HMACSHA1(Encoding.ASCII.GetBytes(secret_key));
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(input));
            byte[] hashData = hmacsha1.ComputeHash(stream);
            StringBuilder hashBuilder = new StringBuilder();
            foreach (byte data in hashData)
            {
                hashBuilder.Append(data.ToString("x2"));
            }
            return hashBuilder.ToString();
        }
        static byte[] sha256_hash(String value)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));
                return result;
            }
        }
    }
}