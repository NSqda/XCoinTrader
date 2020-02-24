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
        private static string path = "../../../../Key/key.json";

        public static JObject json = JObject.Parse(System.IO.File.ReadAllText(path));
    }
}
