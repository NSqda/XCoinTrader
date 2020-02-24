using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using qda.Net;

namespace XCoinTrader
{
    public class SentiCrypt
    {
        public void GetSentiCrypt()
        {
            JArray list = REST_API.GetJson("https://api.senticrypt.com/v1/history/index.json");

            for (int i = 0; i < list.Count; i++)
            {
                using (StreamWriter file = new StreamWriter(@"../../../History/" + list[i].ToString().Replace('_', '#').Replace(".json", ".csv"), true))

                {
                    var json = REST_API.GetJson("https://api.senticrypt.com/v1/history/" + list[i].ToString());

                    file.WriteLine("timestamp,sum,mean,median,count,rate,btc_price,last");
                    for (int i2 = 0; i2 < json.Count; i2++)
                    {
                        var body = json[i2];
                        file.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7}", body["timestamp"], body["sum"], body["mean"], body["median"], body["count"], body["rate"], body["btc_price"], body["last"]);
                    }
                    Console.WriteLine(list[i]);
                }
            }
        }
    }
}