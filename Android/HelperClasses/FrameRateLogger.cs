using System;
using System.Threading;
using Fusee.Engine.Core;

namespace Fusee.Tutorial.Android.HelperClasses
{
    /// <summary>
    /// FrameRate displays the current FrameRate on a given text.
    /// </summary>

    class FrameRateLogger
    {
        private Timer _timer;

        public FrameRateLogger()
        {
            TimerCallback timerDelegate = new TimerCallback(PrintFrameRate);
            _timer = new Timer(timerDelegate, this, 0, 1000);
        }

        private void PrintFrameRate(Object state)
        {
            Console.WriteLine(Time.FramePerSecond.ToString());
        }
    }
}