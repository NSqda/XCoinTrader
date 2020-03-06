using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace qda
{
    public class Key
    {
        public static JObject json = JObject.Parse(Net.REST_API.GetString("https://drive.google.com/uc?id=1XkVPLsso7BTsg-QiVM4ZYrykz4f-UNpl&export=download"));
    }
}
