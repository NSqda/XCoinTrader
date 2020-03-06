using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace Trader.Traing
{

    namespace SentiCrypt
    {
        public class Utils
        {
            public static List<SentiCrypt> CSVReader(string path)
            {
                var items = new List<SentiCrypt>();
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split(',');

                    if (!parts[0].Contains("time"))

                        items.Add(new SentiCrypt
                        {
                            timestamp = double.Parse(parts[0]),
                            sum = double.Parse(parts[1]),
                            mean = double.Parse(parts[2]),
                            median = double.Parse(parts[3]),
                            count = double.Parse(parts[4]),
                            rate = double.Parse(parts[5]),
                            btc_price = double.Parse(parts[6]),
                            last = double.Parse(parts[7])
                        });
                }

                return items;
            }

            public static List<SentiCrypt> BinReader(string path)
            {
                var items = new List<SentiCrypt>();
                var bytes = File.ReadAllBytes(path);
                var rowSize = sizeof(double) * 8;
                for (var offset = 0; offset < bytes.Length; offset += rowSize)
                {
                    items.Add(new SentiCrypt
                    {
                        timestamp = BitConverter.ToDouble(bytes, offset + 0),
                        sum = BitConverter.ToDouble(bytes, offset + 8),
                        mean = BitConverter.ToDouble(bytes, offset + 16),
                        median = BitConverter.ToDouble(bytes, offset + 24),
                        count = BitConverter.ToDouble(bytes, offset + 32),
                        rate = BitConverter.ToDouble(bytes, offset + 40),
                        btc_price = BitConverter.ToDouble(bytes, offset + 48),
                        last = BitConverter.ToDouble(bytes, offset + 56)
                    });
                }

                return items;
            }

            public static void CsvToBin(string csvPath, string binPath)
            {
                var startTime = DateTime.Now;
                var items = CSVReader(csvPath);
                Console.WriteLine(csvPath + " : " + (DateTime.Now - startTime));
                startTime = DateTime.Now;

                using (var fileStream = new FileStream(binPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var item in items)
                    {
                        writer.Write(item.timestamp);
                        writer.Write(item.sum);
                        writer.Write(item.mean);
                        writer.Write(item.median);
                        writer.Write(item.count);
                        writer.Write(item.rate);
                        writer.Write(item.btc_price);
                        writer.Write(item.last);
                    }
                }

                Console.WriteLine(csvPath + " : " + (DateTime.Now - startTime));
            }
        }

        public struct SentiCrypt
        {
            public double timestamp;
            public double sum;
            public double mean;
            public double median;
            public double count;
            public double rate;
            public double btc_price;
            public double last;
        }
    }

    namespace CryptoCompare
    {
        public class Utils
        {
            public static List<CryptoCompare> CSVReader(string path)
            {
                var items = new List<CryptoCompare>();
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Split(',');

                    if (!parts[0].Contains("time"))
                        items.Add(new CryptoCompare
                        {
                            timestamp = double.Parse(parts[0]),
                            high = double.Parse(parts[1]),
                            low = double.Parse(parts[2]),
                            open = double.Parse(parts[3]),
                            volumefrom = double.Parse(parts[4]),
                            volumeto = double.Parse(parts[5]),
                            close = double.Parse(parts[6])
                        });
                }
                return items;
            }

            public static List<CryptoCompare> BinReader(string path)
            {
                var items = new List<CryptoCompare>();
                var bytes = File.ReadAllBytes(path);
                var rowSize = sizeof(double) * 8;
                for (var offset = 0; offset < bytes.Length; offset += rowSize)
                {
                    items.Add(new CryptoCompare
                    {
                        timestamp = BitConverter.ToDouble(bytes, offset + 0),
                        high = BitConverter.ToDouble(bytes, offset + 8),
                        low = BitConverter.ToDouble(bytes, offset + 16),
                        open = BitConverter.ToDouble(bytes, offset + 24),
                        volumefrom = BitConverter.ToDouble(bytes, offset + 32),
                        volumeto = BitConverter.ToDouble(bytes, offset + 40),
                        close = BitConverter.ToDouble(bytes, offset + 48)
                    });
                }

                return items;
            }

            public static void CsvToBin(string csvPath, string binPath)
            {
                var startTime = DateTime.Now;
                var items = CSVReader(csvPath);
                Console.WriteLine(csvPath + " : " + (DateTime.Now - startTime));
                startTime = DateTime.Now;

                using (var fileStream = new FileStream(binPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new BinaryWriter(fileStream))
                {
                    foreach (var item in items)
                    {
                        writer.Write(item.timestamp);
                        writer.Write(item.high);
                        writer.Write(item.low);
                        writer.Write(item.open);
                        writer.Write(item.volumefrom);
                        writer.Write(item.volumeto);
                        writer.Write(item.close);
                    }
                }

                Console.WriteLine(csvPath + " : " + (DateTime.Now - startTime));
            }
        }
        public class CryptoCompare
        {
            public double timestamp;
            public double high;
            public double low;
            public double open;
            public double volumefrom;
            public double volumeto;
            public double close;
        }
    }

    public class CSVToList
    {
        public static List<List<double>> Read(string path)
        {
            var line = File.ReadAllLines(path);

            var list = new List<List<double>>();

            for (int i1 = 0; i1 < line.Length; i1++)
            {
                var t = new List<double>();
                var parts = line[i1].Split(',');
                if (parts[0].Contains("time"))
                    continue;
                //don't need to use timestamp
                for (int i2 = 1; i2 < parts.Length; i2++)
                    t.Add(double.Parse(parts[i2]));

                list.Add(t);
            }

            return list;
        }
    }
}