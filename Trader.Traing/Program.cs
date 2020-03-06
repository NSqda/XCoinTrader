using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using ConvNetSharp;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Serialization;
using ConvNetSharp.Core.Training;
using ConvNetSharp.Core.Training.Double;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.GPU;
using ConvNetSharp.Volume.GPU.Double;

namespace Trader.Training
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Program p = new Program();
            p.Main();
        }

        private void Main()
        {


        }

        public class State
        {
            public List<double> state;
            public int action;
            public double reward;
            public List<double> next_state;
            public bool done;
        }
    }
}