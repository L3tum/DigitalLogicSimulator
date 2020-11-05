using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DigitalLogicSimulator
{
    public class TimeManager
    {
        private const double NANOSECONDS_PER_SECOND = 1000d * 1000d * 1000d;
        private static readonly double TicksPerNanosecond = Stopwatch.Frequency / NANOSECONDS_PER_SECOND;
        internal readonly double NanosecondsPerSpin;
        private readonly double spinsPerNanosecond;

        public TimeManager()
        {
            var sw = Stopwatch.StartNew();
            var i = 0;
            while (i < 10000000)
            {
                i++;
            }

            var ticks = sw.ElapsedTicks;
            var elapsedNanoseconds = ticks / TicksPerNanosecond;
            NanosecondsPerSpin = elapsedNanoseconds / 10000000d;
            spinsPerNanosecond = 1 / NanosecondsPerSpin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Wait(long nanoseconds)
        {
            // Subtract one because of some overhead
            var spins = nanoseconds * spinsPerNanosecond - NanosecondsPerSpin * TicksPerNanosecond;
            var i = 0;
            while (i < spins)
            {
                i++;
            }

            return i;
        }

        public long GetTicksFromNanoseconds(long nanoseconds)
        {
            return (long) (nanoseconds * TicksPerNanosecond);
        }
    }
}