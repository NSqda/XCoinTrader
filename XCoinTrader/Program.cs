using System;
using System.Collections.Generic;
using ConvNetSharp;
using ConvNetSharp.Core;
using ConvNetSharp.Core.Layers;
using ConvNetSharp.Core.Layers.Double;
using ConvNetSharp.Core.Training;
using ConvNetSharp.Core.Training.Double;
using ConvNetSharp.Volume;
using ConvNetSharp.Volume.GPU;
using ConvNetSharp.Volume.GPU.Double;
using ConvNetSharp.Core.Serialization;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XCoinTrader;
using System.IO;
using System.Text;
using System.Linq;
using Trader.Training;

namespace XCoinTrader
{
    public class Program
    {

        public int BUY = 0, SELL = 1, HOLD = 2;

        public int screenWidth = 1;
        public int screenHeight = 1;
        public int screenDepth = -1;
        public int dataSize = -1;
        public int batchSize = 32;
        public int range = 24 * 3;
        public int numAction = 3;
        public double gamma = 0.95;
        public double epsilon = 1.0;
        public double epsilonMin = 0.01;
        public double epsilonDecay = 0.995;
        public int episode = 0;
        public int epoch = 0;
        public int age = 0;
        public int episodeMin = 1000;

        public int TIMESTAMP = 0, HIGH = 1, LOW = 2, OPEN = 3, VOLUMEFROM = 4, VOLUMETO = 5, CLOSE = 6;
        public List<List<double>> data;
        private List<State> inventory = new List<State>();
        public List<State> stateMemory = new List<State>();
        public List<double> averageLossMemory = new List<double>();


        private Random random = new Random();

        private string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;

        private string modelPath;

        private string mainJson;
        private string targetJson;

        private StringBuilder lossWriter = new StringBuilder();
        private void Main()
        {

            modelPath = projectPath + "/model/";

            data = CSVToList.Read(projectPath + "/History/BTC_USD_TEST.csv");

            dataSize = data[0].Count;
            screenDepth = dataSize * range;
            var json = File.ReadAllText(modelPath + "main2020_03_06_04_52_44.model");
            var net = SerializationExtensions.FromJson<double>(json);
            double totalProfit = 0.0, totalGain = 0.0, totalLoss = 0.0, totalSpent = 0.0;

            for (int i = 0; i < data.Count; i++)
            {

                for (int fromTime = 0; fromTime < dataSize; fromTime++)
                {
                    var toTime = fromTime + range;
                    var input = GetData(fromTime, toTime);
                    var inputVol = ToVolume(input);
                    var result = net.Forward(inputVol);

                    var state = new State();
                    state.state = input;
                    state.action = GetAction(result, false);
                    state.done = toTime < data.Count ? false : true;
                    state.next_state = state.done ? input : GetData(fromTime + 1, toTime + 1);

                    if (state.action == BUY)
                    {
                        state.price = data[toTime][LOW];
                        inventory.Add(state);

                        totalSpent += state.price;
                    }
                    else if (state.action == SELL && inventory.Count > 0)
                    {
                        state.price = data[toTime][HIGH];

                        var reward = 0.0;
                        inventory.ForEach(i => reward += (i.price - state.price));
                        inventory.Clear();
                        state.reward = reward;

                        totalProfit += reward;

                        if (reward > 0)
                            totalGain += reward;
                        else
                            totalLoss += reward;

                    }
                    else if (state.action == HOLD)
                    {
                        state.price = data[toTime][LOW];
                    }

                    stateMemory.Add(state);
                    Console.WriteLine($"{ActionToString(state.action)}: {state.price}  reward: {state.reward}\n");


                }
            }
            Console.WriteLine($"totalGain: {totalGain}\n" +
                                          $"totalLoss: {totalLoss}\n" +
                                          $"totalSpent: {totalSpent}\n" +
                                          $"totalProfit: {totalProfit}\n" +
                                          $"av.Profit: {totalProfit / episode}\n");
            Console.WriteLine(data.Count);
        }

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Main();
        }

        public List<double> GetData(int fromTime, int toTime)
        {
            var temp = new List<double>();
            for (int i1 = fromTime; i1 < toTime; i1++)
                for (int i2 = 0; i2 < dataSize; i2++)
                    temp.Add(data[i1][i2]);
            return temp;
        }

        public Volume<double> ToVolume(List<double> list)
        {
            var vol = BuilderInstance.Volume.SameAs(new Shape(screenWidth, screenHeight, screenDepth));

            for (int iw = 0; iw < screenWidth; iw++)
                for (int ih = 0; ih < screenHeight; ih++)
                    for (int ic = 0; ic < screenDepth; ic++)
                        vol.Set(iw, ih, ic, list[ic]);
            return vol;
        }

        public int GetAction(Volume<double> input, bool isEvaluating)
        {
            int action;
            if (isEvaluating)
            {
                var arr = input.ToArray();
                var _value = arr.Max();
                var _index = Array.IndexOf(arr, _value);
                action = _index;
            }
            else
            {
                if (random.NextDouble() > epsilon)
                {
                    var arr = input.ToArray();
                    var _value = arr.Max();
                    var _index = Array.IndexOf(arr, _value);
                    action = _index;
                }
                else
                {
                    action = random.Next(0, numAction);
                }
            }
            return action;
        }

        public string ActionToString (int action)
        {
            if (action == SELL)
                return "SELL";
            else if (action == BUY)
                return "BUY";
            else
                return "HOLD";
        }
    }
    public class State
    {
        public List<double> state;
        public List<double> next_state;
        public int action;
        public double price;
        public double reward = 0.0;
        public bool done;
    }


}
