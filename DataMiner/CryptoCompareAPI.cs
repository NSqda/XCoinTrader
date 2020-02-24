using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using qda;

namespace CryptoCompare
{
    public class Header
    {
        public string Response { get; set; }
        public string Message { get; set; }
        public bool HasWarning { get; set; }
        public decimal Type { get; set; }
        public object RateLimit { get; set; }
        public Body Data { get; set; }

    }
    public class Body
    {
        public bool Aggregated { get; set; }
        public decimal TimeFrom { get; set; }
        public decimal TimeTo { get; set; }
        public IList<History> Data { get; set; }

    }

    public class History
    {
        public decimal time { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal open { get; set; }
        public decimal volumefrom { get; set; }
        public decimal volumeto { get; set; }
        public decimal close { get; set; }
        public string conversionType { get; set; }
        public string conversionSymbol { get; set; }
    }
    public class CryptoCompareAPI
    {
        public CryptoCompareAPI()
        {
        }
        private string Query(string method, string function, Dictionary<string, string> param = null)
        {
            string paramData = BuildQueryData(param);
            string url = "https://min-api.cryptocompare.com/data/v2" + function + "?" + paramData;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = method;

            try
            {
                using (WebResponse webResponse = webRequest.GetResponse())
                using (Stream str = webResponse.GetResponseStream())
                using (StreamReader sr = new StreamReader(str))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse response = (HttpWebResponse)wex.Response)
                {
                    if (response == null)
                        throw;

                    using (Stream str = response.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(str))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }

        private string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            StringBuilder sb = new StringBuilder();
            foreach (var item in param)
                sb.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));

            try { return sb.ToString().Substring(1); }
            catch (Exception) { return ""; }
        }
        public string GetHourlyPairOHLCV(string fsym = "", string tsym = "", string limit = "", string toTs = "")
        {

            var param = new Dictionary<string, string>();
            param["fsym"] = fsym;
            param["tsym"] = tsym;
            param["limit"] = limit;
            if (!toTs.Equals(""))
                param["toTs"] = toTs;
            param["api_key"] = Key.json["CryptoCompareAPI_key"].ToString ();
            return Query("GET", "/histohour", param);
        }
    }
}
