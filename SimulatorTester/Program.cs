using System;
using System.Threading;
using System.Threading.Tasks;
using DigitalLogicSimulator;
using DigitalLogicSimulator.Nodes;

namespace SimulatorTester
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // var sw = Stopwatch.StartNew();
            // Thread.Sleep(TimeSpan.FromTicks(1));
            // var ticks = sw.ElapsedTicks;
            // var microseconds = ticks / (Stopwatch.Frequency / (1000d * 1000d));
            // var nanoseconds = ticks / (Stopwatch.Frequency / (1000d * 1000d * 1000d));

            // Console.WriteLine("Frequency: {0} ticks/s", Stopwatch.Frequency);
            // Console.WriteLine("Frequency: {0} ticks/ms", Stopwatch.Frequency / 1000d);
            // Console.WriteLine("Frequency: {0} ticks/us", Stopwatch.Frequency / 1000d / 1000d);
            // Console.WriteLine("Frequency: {0} ticks/ns", Stopwatch.Frequency / 1000d / 1000d / 1000d);
            // Console.WriteLine("Microseconds: {0}", microseconds);
            // Console.WriteLine("Nanoseconds: {0}", nanoseconds);
            // Console.WriteLine("Ticks: {0}", ticks);


            var clock = new Clock(0, 100);
            var lamp = new LEDNode(1);
            var wire = new Wire(2);

            var not = new NOTNode(3);
            var wire2 = new Wire(4);
            wire.NodeToOutputTo = not;
            wire.InputAtNodeConnectedTo = 0;
            clock.OutputWiresConnected[0].Add(wire);
            wire2.NodeToOutputTo = lamp;
            wire2.InputAtNodeConnectedTo = 0;
            not.OutputWiresConnected[0].Add(wire2);
            Simulator.Nodes.Add(clock);
            Simulator.Nodes.Add(lamp);
            Simulator.Nodes.Add(not);
            Simulator.Nodes.Add(wire);
            Simulator.Nodes.Add(wire2);
            Simulator.CollectClocks();

            Sampler.Configure(100, false);

            var cancellationTokenSource = new CancellationTokenSource();
            var task = Task.Factory.StartNew(() => Simulator.Simulate(cancellationTokenSource.Token),
                TaskCreationOptions.LongRunning);

            Thread.Sleep(1000);

            cancellationTokenSource.Cancel();
            task.Wait(1000);

            Console.WriteLine("Stop {0}", Sampler.Samples.Count);
            Console.WriteLine();
            WavePrinter.PrintWave(Sampler.Samples);
        }
    }
}