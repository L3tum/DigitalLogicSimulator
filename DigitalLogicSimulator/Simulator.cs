using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DigitalLogicSimulator.Nodes;

namespace DigitalLogicSimulator
{
    public static class Simulator
    {
        private const int MAX_ITERATIONS_STEADY_STATE = 100;
        public static readonly List<Clock> Clocks = new List<Clock>();
        public static readonly List<Node> Nodes = new List<Node>();
        private static readonly List<Node> ChangeSets = new List<Node>();
        private static readonly List<Task> ClockTasks = new List<Task>();
        public static TimeManager TimeManager;
        private static readonly Dictionary<int, List<Task>> TasksByClock = new Dictionary<int, List<Task>>();

        public static int CollectClocks()
        {
            var clocks = Nodes.OfType<Clock>().ToList();

            Clocks.AddRange(clocks);

            return clocks.Count;
        }

        public static void Simulate(CancellationToken cancellationToken)
        {
            Console.WriteLine("Analyzing System Performance...");
            TimeManager = new TimeManager();
            Console.WriteLine("Analyzed System Performance. Smallest possible delay: ~{0}ns",
                TimeManager.NanosecondsPerSpin);

            Console.WriteLine("Trying to reach steady state...");
            var sw = Stopwatch.StartNew();
            var iterations = 0;
            do
            {
                SimulateChangeSets();
                SimulateNodes();
                iterations++;
            } while (!cancellationToken.IsCancellationRequested && ChangeSets.Count > 0 &&
                     iterations < MAX_ITERATIONS_STEADY_STATE);

            sw.Stop();

            if (iterations == MAX_ITERATIONS_STEADY_STATE)
            {
                Console.WriteLine("Could not reach steady state after {0} iterations or {1}ms", iterations,
                    sw.ElapsedMilliseconds);
            }
            else
            {
                Console.WriteLine("Reached steady state after {0} iterations or {1}ms", iterations,
                    sw.ElapsedMilliseconds);
            }

            Console.WriteLine("Starting sampler");
            Sampler.StartSampler();

            Console.WriteLine("Starting simulation threads");

            foreach (var clock in Clocks)
            {
                var task = Task.Run(() => SimulateClock(clock, cancellationToken));
                ClockTasks.Add(task);
                TasksByClock.Add(clock.ID, new List<Task>());
            }

            Console.WriteLine("Started {0} simulation threads", ClockTasks.Count);

            Task.WaitAll(ClockTasks.ToArray());
            ClockTasks.Clear();
            TasksByClock.Clear();

            Console.WriteLine("Simulation stopped.");
            Sampler.StopSampler();
            Console.WriteLine("Sampler stopped.");

            // var stopwatch = Stopwatch.StartNew();
            // while (!cancellationToken.IsCancellationRequested)
            // {
            //     TotalElapsedTicks = stopwatch.ElapsedTicks;
            //     ElapsedNanoseconds = (long) ((TotalElapsedTicks - LastTotalElapsedTicks) / TicksPerNanosecond);
            //     LastTotalElapsedTicks = TotalElapsedTicks;
            //     Hz = (long) (NanosecondsPerSecond / ElapsedNanoseconds);
            //
            //     SimulateChangeSets();
            // }
        }

        private static void SimulateClock(Clock clock, CancellationToken cancellationToken)
        {
            var tasksByClock = TasksByClock[clock.ID];

            while (!cancellationToken.IsCancellationRequested)
            {
                var task = Task.Run(() => SimulateAndTraceNode(clock, cancellationToken, clock.ID));
                tasksByClock.Add(task);

                tasksByClock.RemoveAll(t => t.IsCanceled || t.IsCompleted || t.IsFaulted);

                Thread.Sleep(TimeSpan.FromTicks(TimeManager.GetTicksFromNanoseconds(clock.DelayInNanoseconds) - 50));
            }
        }

        private static void SimulateAndTraceNode(Node node, CancellationToken cancellationToken, int clockId)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var delayInTicks = TimeManager.GetTicksFromNanoseconds(node.DelayInNanoseconds);

                if (delayInTicks > 200)
                {
                    Thread.Sleep(TimeSpan.FromTicks(delayInTicks));

                    // Stop if simulation stopped after delay
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    TimeManager.Wait(node.DelayInNanoseconds);
                }
                
                var changed = node.Simulate();

                if (!changed)
                {
                    return;
                }

                var tasksByClock = TasksByClock[clockId];

                for (var index = 0; index < node.OutputWiresConnected.Count; index++)
                {
                    var wires = node.OutputWiresConnected[index];
                    var value = node.Outputs[index];

                    foreach (var wire in wires)
                    {
                        if (value != wire.Inputs[0])
                        {
                            wire.Inputs[0] = value;
                            var task = Task.Run(() => SimulateAndTraceWire(wire, cancellationToken, clockId));
                            tasksByClock.Add(task);
                        }
                    }
                }
            }
        }

        private static void SimulateAndTraceWire(Wire wire, CancellationToken cancellationToken, int clockId)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var delayInTicks = TimeManager.GetTicksFromNanoseconds(wire.DelayInNanoseconds);

                if (delayInTicks > 200)
                {
                    Thread.Sleep(TimeSpan.FromTicks(delayInTicks));

                    // Stop if simulation stopped after delay
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    TimeManager.Wait(wire.DelayInNanoseconds);
                }
                
                var changed = wire.Simulate();

                if (!changed)
                {
                    return;
                }

                wire.NodeToOutputTo.Inputs[wire.InputAtNodeConnectedTo] = wire.Outputs[0];
                SimulateAndTraceNode(wire.NodeToOutputTo, cancellationToken, clockId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SimulateNodes()
        {
            foreach (var node in Nodes)
            {
                if (!(node is Clock))
                {
                    var changed = node.Simulate();

                    if (changed)
                    {
                        ChangeSets.Add(node);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SimulateChangeSets()
        {
            foreach (var changeSet in ChangeSets)
            {
                var node = changeSet;
                WalkNodeOutputs(ref node);
            }

            ChangeSets.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WalkNodeOutputs(ref Node node)
        {
            for (var index = 0; index < node.OutputWiresConnected.Count; index++)
            {
                var wires = node.OutputWiresConnected[index];
                var value = node.Outputs[index];

                foreach (var wire in wires)
                {
                    if (wire.Inputs[0] != value)
                    {
                        wire.Inputs[0] = value;
                        wire.NodeToOutputTo.Inputs[wire.InputAtNodeConnectedTo] = value;
                    }
                }
            }
        }
    }
}