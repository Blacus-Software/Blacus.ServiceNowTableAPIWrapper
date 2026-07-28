namespace Blacus
{

#pragma warning disable format
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.

    using System;
    using System.Diagnostics;

    public class CodeTimer : IDisposable
    {

        public static bool  GlobalEnabled { get; set; } = true;

        public bool         Enabled { get; set; } = true;

        public string       Name;

        public long         ElapsedMilliseconds => Stopwatch.ElapsedMilliseconds;
        public long         ElapsedTicks        => Stopwatch.ElapsedTicks;
        
        public TimeSpan     Elapsed             => Stopwatch.Elapsed;

        public Stopwatch    Stopwatch           = new();

        public Action<CodeTimer> OnTimerStarted   = DefaultOnTimerStarted;
        public Action<CodeTimer> OnTimerStopped   = DefaultOnTimerStopped;
        public Action<CodeTimer> OnTimerInterval  = DefaultOnTimerStopped;

        public CodeTimer(string timerName = null) =>
                Name = timerName ?? Guid.NewGuid().ToString();

        public CodeTimer Start()
        {

            if (Enabled && GlobalEnabled) 
            {

                Stopwatch = Stopwatch.StartNew();
                OnTimerStarted(this);

                return this;

            }
            return null;

        }

        public void Interval() => OnTimerInterval(this);

        public static void DefaultOnTimerStopped(CodeTimer codeTimer) =>
            Console.WriteLine($"{codeTimer.Name} => Elapsed Time: {codeTimer.ElapsedMilliseconds}ms");

        public static void DefaultOnTimerStarted(CodeTimer codeTimer) { }

        void IDisposable.Dispose()
        {

            if (Enabled && GlobalEnabled)
            {

                Stopwatch.Stop();

                OnTimerStopped(this);

            }

        }

    }

}
