using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace DigitalLogicSimulator
{
    public static class Sampler
    {
        private static int delayBetweenSamplesInMilliseconds;
        private static bool sampleWires;
        private static Task sampleTask;
        private static bool doSample;
        public static readonly Dictionary<long, List<Sample>> Samples = new Dictionary<long, List<Sample>>();

        public static void Configure(int samplesPerSecondTaken, bool shouldSampleWires)
        {
            delayBetweenSamplesInMilliseconds = 1000 / samplesPerSecondTaken;
            sampleWires = shouldSampleWires;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StartSampler()
        {
            if (delayBetweenSamplesInMilliseconds > 0)
            {
                doSample = true;
                Samples.Clear();
                sampleTask = Task.Run(Sampling);
            }
        }

        private static void Sampling()
        {
            var sw = Stopwatch.StartNew();
            while (doSample)
            {
                var time = sw.ElapsedMilliseconds;

                if (!sampleWires)
                {
                    Samples.Add(time, Simulator.Nodes.Where(n => !(n is Wire)).Select(node => node.Sample()).ToList());
                }
                else
                {
                    Samples.Add(time, Simulator.Nodes.ConvertAll(node => node.Sample()));
                }

                Thread.Sleep(delayBetweenSamplesInMilliseconds);
            }

            sw.Stop();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void StopSampler()
        {
            doSample = false;

            sampleTask?.Wait();

            sampleTask = null;
        }
    }
}