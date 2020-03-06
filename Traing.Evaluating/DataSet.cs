using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Traing.Evaluating
{
    public static class DataSet
    {
        public static int TIMESTAMP = 0 , HIGH = 1, LOW = 2, OPEN = 3, VOLUMEFROM = 4, VOLUMETO = 5, CLOSE = 6;
        public static List<double> GetData(string path)
        {
            var list = new List<double>();
            var line = File.ReadAllLines(path);

            for (int i1 = 1; i1 < line.Length; i1++)
            {
                var parts = line[i1].Split(',');

                for (int i2 = 0; i2 < parts.Length; i2++)
                    list.Add(double.Parse(parts[i2]));
            }
            return list;
        }

    }

}

