using System;
using System.Collections.Generic;
using ConvNetSharp;
using ConvNetSharp.Core;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.GPU;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XCoinTrader
{
    class Program
    {
        static void Main(string[] args)
        {
            Program program = new Program();
            program.main();
            Console.ReadKey();
        }
        public void main()
        {
            qda.Net.SendMail.Send(subject: "Starting Operation", body: "dd");
            SentiCrypt sc = new SentiCrypt();
            sc.GetSentiCrypt();
            qda.Net.SendMail.Send(subject: "End", body: "dd");
        }

        
    }


}
