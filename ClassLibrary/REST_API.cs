using System;
using System.IO;
using System.Net;
using Newtonsoft;
using Newtonsoft.Json.Linq;

namespace qda
{
    namespace Net
    {
        public class REST_API
        {
            public static JArray GetJson(string url)
            {
                var request = WebRequest.Create(url);
                var responde = request.GetResponse().GetResponseStream();

                using (StreamReader sr = new StreamReader(responde))
                {
                    return JArray.Parse(sr.ReadToEnd());
                }
            }

            public static string GetString (string url)
            {
                var request = WebRequest.Create(url);
                var responde = request.GetResponse().GetResponseStream();

                using (StreamReader sr = new StreamReader(responde))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
