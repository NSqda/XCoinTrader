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
using ConvNetSharp.Volume.GPU.Double;
using Trader.Training;
using System.Diagnostics;

namespace Trader.Evaluating
{
    public class Program
    {
        private bool isLearning = false;

        private Net<double> mainNet;
        private Net<double> targetNet;
        private SgdTrainer trainer;

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
        private string dataPath;

        private string mainJson;
        private string targetJson;

        private string parms;

        private StringBuilder lossWriter = new StringBuilder();

        private void Main()
        {
            BuilderInstance<double>.Volume = new VolumeBuilder();

            modelPath = projectPath + "/model/";
            dataPath = projectPath + "/History/";

            while (true)
            {
                Console.WriteLine("학습할 데이터를 입력.");
                var line = Console.ReadLine();

                if (File.Exists(dataPath + line))
                {
                    try
                    {
                        data = CSVToList.Read(dataPath + line);
                        parms = line.Replace(".csv", "_");
                        break;
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine("올바른 파일을 입력.");
                    }

                }
                else Console.WriteLine("파일이 존재하지 않습니다.");
            }

            dataSize = data[0].Count;
            screenDepth = dataSize * range + 1;

            mainNet = new Net<double>();
            targetNet = new Net<double>();

            BuildNetwork(mainNet);
            BuildNetwork(targetNet);


            trainer = new SgdTrainer(mainNet) { LearningRate = 0.01, BatchSize = batchSize };
            StringBuilder log = new StringBuilder();

            parms += DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            do
            {
                epoch++;
                double totalProfit = 0.0, totalGain = 0.0, totalLoss = 0.0, totalSpent = 0.0;
                for (int fromTime = 0; data.Count > fromTime + range; fromTime++)
                {
                    episode++;

                    var toTime = fromTime + range;
                    var input = GetData(fromTime, toTime);
                    input.Add(inventory.Count);
                    var inputVol = ToVolume(input);
                    var result = mainNet.Forward(inputVol);

                    var state = new State();
                    state.state = input;
                    state.action = GetAction(result, false);
                    state.done = toTime < data.Count ? false : true;

                    if (state.action == BUY)
                    {
                        state.price = data[toTime][LOW];
                        inventory.Add(state);

                        totalSpent += state.price;
                    }
                    else if (state.action == SELL)
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

                    var nextInput = state.done ? input : GetData(fromTime + 1, toTime + 1);
                    nextInput.Add(inventory.Count);
                    state.next_state = nextInput;
                    stateMemory.Add(state);

                    if (stateMemory.Count > batchSize)
                        ExperienceReplay();

                    if (episode % 10 == 0 && age > 0)
                        targetNet = SerializationExtensions.FromJson<double>(mainNet.ToJson());

                    var _loss = averageLossMemory.Count > 0 ? averageLossMemory.Average() : 0;
                    log.AppendLine($"{epoch},{episode},{state.action},{state.price},{totalSpent},{totalGain},{totalLoss},{totalProfit},{totalProfit / fromTime},{age},{_loss},{_loss / batchSize}");

                    if (episode % 1000 == 0)
                    {
                        Console.WriteLine($"{parms}\n" +
                                          $"epoch: {epoch}\n" +
                                          $"episode: {episode}\n" +
                                          $"totalGain: {totalGain}\n" +
                                          $"totalLoss: {totalLoss}\n" +
                                          $"totalSpent: {totalSpent}\n" +
                                          $"totalProfit: {totalProfit}\n" +
                                          $"av.Profit: {totalProfit / fromTime}\n" +
                                          $"av.Loss: {(averageLossMemory.Count > 0 ? averageLossMemory.Average() : 0)}");

                        File.AppendAllText(projectPath + "/log/log" + parms + ".log", log.ToString());
                        log.Clear();
                        File.AppendAllText(projectPath + "/log/loss" + parms + ".log", lossWriter.ToString());
                        lossWriter.Clear();
                        File.WriteAllText(projectPath + "/model/main" + parms + ".model", mainNet.ToJson());
                        File.WriteAllText(projectPath + "/model/target" + parms + ".model", targetNet.ToJson());
                    }
                }
                Console.WriteLine("------------------------------");
                Console.WriteLine($"epoch: {epoch}\n" +
                  $"episode: {episode}\n" +
                  $"totalGain: {totalGain}\n" +
                  $"totalLoss: {totalLoss}\n" +
                  $"totalSpent: {totalSpent}\n" +
                  $"profit: {totalProfit}\n" +
                  $"av.Profit: {totalProfit/dataSize}\n" +
                  $"av.Loss: {(averageLossMemory.Count > 0 ? averageLossMemory.Average() : 0)}\n");
                Console.WriteLine("------------------------------");

                File.AppendAllText(projectPath + "/log/log" + parms + ".log", log.ToString());
                log.Clear();
                File.AppendAllText(projectPath + "/log/loss" + parms + ".log", lossWriter.ToString());
                lossWriter.Clear();

                inventory.Clear();
                stateMemory.Clear();
                averageLossMemory.Clear();

                qda.Net.SendMail.Send(subject: $"{parms}", body: $"epoch: {epoch}\n" +
                  $"episode: {episode}\n" +
                  $"totalGain: {totalGain}\n" +
                  $"totalLoss: {totalLoss}\n" +
                  $"totalSpent: {totalSpent}\n" +
                  $"av.Profit: {totalProfit}\n" +
                  $"av.Loss: {(averageLossMemory.Count > 0 ? averageLossMemory.Average() : 0)}\n");
            } while (true);
        }

        public void BuildNetwork(Net<double> net)
        {
            net.AddLayer(new InputLayer(1, 1, screenDepth));
            net.AddLayer(new FullyConnLayer(128));
            net.AddLayer(new ReluLayer());
            net.AddLayer(new FullyConnLayer(128));
            net.AddLayer(new ReluLayer());
            net.AddLayer(new FullyConnLayer(16));
            net.AddLayer(new ReluLayer());
            net.AddLayer(new FullyConnLayer(numAction));
            net.AddLayer(new SoftmaxLayer(3));
        }

        public void ExperienceReplay()
        {
            var miniBatch = new List<State>();
            for (int i = 0; i < batchSize; i++)
                miniBatch.Add(stateMemory[random.Next(0, stateMemory.Count)]);

            miniBatch.Shuffle();

            var inputList = miniBatch.Select(x => x.state).ToList();
            var nextInputList = miniBatch.Select(x => x.next_state).ToList();

            var inputVol = ToVolume(inputList, true);
            var nextInputVol = ToVolume(nextInputList, true);

            var result = mainNet.Forward(inputVol);
            var nextResult = targetNet.Forward(nextInputVol);

            var value = GetAction(result);
            var nextValue = GetAction(nextResult);


            for (int i = 0; i < batchSize; i++)
            {
                var _index = Convert.ToInt32(value[i][0]);
                value[i][1] += gamma * nextValue[i][1];

                result.Set(0, 0, _index, i, value[i][1]);
            }

            trainer.Train(inputVol, result);
            averageLossMemory.Add(trainer.Loss);

            if (epsilon > epsilonMin)
                epsilon *= epsilonDecay;

            age++;

            lossWriter.AppendLine($"{age},{trainer.Loss}");
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

        public Volume<double> ToVolume(List<List<double>> list, bool isTraining)
        {
            var vol = BuilderInstance.Volume.SameAs(new Shape(screenWidth, screenHeight, screenDepth, batchSize));

            for (int ib = 0; ib < batchSize; ib++)
                for (int iw = 0; iw < screenWidth; iw++)
                    for (int ih = 0; ih < screenHeight; ih++)
                        for (int ic = 0; ic < screenDepth; ic++)
                            vol.Set(iw, ih, ic, ib, list[ib][ic]);
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

        public double[][] GetAction(Volume<double> input)
        {
            var arr = new double[batchSize][];
            for (int ib = 0; ib < batchSize; ib++)
            {
                arr[ib] = new double[numAction];

                for (int ic = 0; ic < numAction; ic++)
                    arr[ib][ic] = input.Get(0, 0, ic, ib);
            }

            var arrOut = new double[batchSize][];
            for (int ib = 0; ib < batchSize; ib++)
            {
                arrOut[ib] = new double[2];
                var _value = arr[ib].Max();
                var _index = Array.IndexOf(arr[ib], _value);
                arrOut[ib][0] = _index;
                arrOut[ib][1] = _value;
            }

            return arrOut;

        }

        private static void Main(string[] args)
        {
            Program p = new Program();
            p.Main();
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

    static class Extension
    {
        private static Random random = new Random();
        public static List<T> Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
