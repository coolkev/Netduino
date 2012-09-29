using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace PomodoroTimer
{
    public class Program
    {
        private static OutputPort led;
        private static InterruptPort button;

        private static Timer timer;
        private static Timer timerFinishedFlash;

        public static void Main()
        {
            // write your code here
            led = new OutputPort(Pins.ONBOARD_LED, false);

            button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled,Port.InterruptMode.InterruptEdgeLow);
            button.OnInterrupt += ToggleTimer;

            Thread.Sleep(Timeout.Infinite);


        }

        private static void ToggleTimer(uint data1, uint data2, DateTime time)
        {

            if (timerFinishedFlash != null)
            {
                timerFinishedFlash.Dispose();
                timerFinishedFlash = null;
            }

            if (timer == null)
            {
                StartTimer();
            }
            else
            {
                timer.Dispose();
                timer = null;
            }
        }

        private static void StartTimer()
        {
            timer = new Timer(TimerFinished, null, new TimeSpan(0, 25, 0), TimeSpan.FromTicks(-TimeSpan.TicksPerMillisecond));
            FlashOnOff();
            Thread.Sleep(100);
            FlashOnOff();

        }

        private static void TimerFinished(object state)
        {
            timer.Dispose();
            timer = null;

            timerFinishedFlash = new Timer(FlashOnOff, null, 0,200);
            
        }

        private static void FlashOnOff(object state = null)
        {
            led.Write(true);
            Thread.Sleep(100);
            led.Write(false);
        }
    }

    
}
