using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using CryptoCompare;
using System.Text;
using qda.Net;

namespace DataMiner
{
    class Program
    {
        private static int sleepDelay = 20;
        private static int limit = 2000;
        private static string fysm = "BTC";
        private static string tysm = "USD";

        private static StreamWriter file;
        private static string path;

        private static object locker1 = new object();
        private static object locker2 = new object();

        private static Timer timer;
        private static bool isStop = false;
        private static bool shutdown = false;

        private static CryptoCompareAPI cryptoCompareAPI = new CryptoCompareAPI();

        private static decimal TimeFrom = 0;
        private static decimal TimeTo = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine("Write down {fysm}");
            fysm = Console.ReadLine().Trim().ToUpper();

            path = string.Format("{0}_{1}_{2:s}.csv", fysm, tysm, DateTime.Now).Replace(':', '-');
            file = new StreamWriter(path, true);

            Console.WriteLine("Starting operation...");
            Console.WriteLine("fysm : {0},tysm : {1}", fysm, tysm);

            SendMail.Send(subject:( fysm + '_' + tysm + " Operation started"),body: DateTime.Now.ToString());

            timer = new Timer(MainLoop, false, 0, 1000);

            while (!isStop)
            {
                if (shutdown)
                    break;
            }

            Console.WriteLine("Operation is stoped");
            Console.WriteLine("Time : " + DateTime.Now);

           SendMail.Send(subject:fysm + '_' + tysm + " Operation completed", body:DateTime.Now.ToString());
            file.Close();
            file.Dispose();
            timer.Dispose();
        }
        private static void MainLoop(object sender)
        {
            lock (locker1)
            {
                if (!isStop)
                {
                    var body = JsonConvert.DeserializeObject<CryptoCompare.Header>(cryptoCompareAPI.GetHourlyPairOHLCV(fysm, tysm, limit.ToString(), TimeFrom.Equals(0) ? "" : TimeFrom.ToString()));
                    TimeFrom = body.Data.TimeFrom;
                    TimeTo = body.Data.TimeTo;
                    Console.WriteLine("Operation start..." + string.Format("TimeFrom : {0}, TimeTo : {1}", TimeFrom, TimeTo));

                    if (!body.Response.Equals("Success"))
                    {
                        timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        shutdown = true;
                        Console.WriteLine("shutdown...");
                        Console.WriteLine("Time : " + DateTime.Now);
                    }

                    for (int i = 0; i < body.Data.Data.Count; i++)
                    {
                        lock (locker2)
                        {
                            var temp = body.Data.Data[i];

                            if (temp.high == 0 && temp.low == 0 && temp.open == 0 && temp.close == 0)
                            {
                                timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                                shutdown = true;
                                Console.WriteLine("shutdown...");
                                Console.WriteLine("Time : " + DateTime.Now);
                                break;
                            }

                            file.WriteLine("{0},{1},{2},{3},{4},{5},{6}", temp.time, temp.high, temp.low, temp.open, temp.volumefrom, temp.volumeto, temp.close);

                            //Thread.Sleep(sleepDelay);
                        }
                    }
                    Console.WriteLine("Operation end..." + string.Format("TimeFrom : {0}, TimeTo : {1}", TimeFrom, TimeTo));
                }
                else
                {
                    shutdown = true;
                    timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    Console.WriteLine("shutdown...");
                    Console.WriteLine("Time : " + DateTime.Now);
                }
            }
        }
    }
}

