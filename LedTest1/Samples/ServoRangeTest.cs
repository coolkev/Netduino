using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace LedTest1.Samples
{
    class ServoRangeTest
    {

        public static void Run()
        {

            var servo = new ServoController(Pins.GPIO_PIN_D9, 600, 3000);

            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            button.OnInterrupt += (data1, data2, time) =>
            {

                //servo.Duration = 1500;
                if (data2 == 1)
                    servo.Rotate(100);
                else
                {
                    servo.Rotate(0);

                }
            };

            while (Debugger.IsAttached)
            {
                Thread.Sleep(1000);

            }

            button.Dispose();
            servo.Dispose();

        }

    }
}
