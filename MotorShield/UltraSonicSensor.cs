using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{
    class UltraSonicSensor
    {

        private static int ticks;

        private static InterruptPort EchoPin = new InterruptPort(Pins.GPIO_PIN_D10, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        private static OutputPort TriggerPin = new OutputPort(Pins.GPIO_PIN_D12, false);

        public static void Main()
        {
            EchoPin.OnInterrupt += new NativeEventHandler(port_OnInterrupt);
            EchoPin.DisableInterrupt();
            while (true)
            {
                Distance();
                //Debug.Print("distance = " + myDistance + " mm.");
                Thread.Sleep(1000);
            }
        }

        public static void Distance()
        {
            EchoPin.EnableInterrupt();
            TriggerPin.Write(false);
            Thread.Sleep(2);
            TriggerPin.Write(true);
            Thread.Sleep(10);
            TriggerPin.Write(false);
            Thread.Sleep(2);
        }

        private static void port_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (state == 0) // falling edge, end of pulse
            {
                int pulseWidth = (int)time.Ticks - ticks;
                // valid for 20°C
                //int pulseWidthMilliSeconds = pulseWidth * 10 / 582;
                //valid for 24°C
                int pulseWidthMilliSeconds = (int)(pulseWidth * 10 / 22.7673228346);
                Debug.Print("Distance = " + pulseWidthMilliSeconds.ToString() + " in.");
            }
            else
            {
                ticks = (int)time.Ticks;
            }
            EchoPin.ClearInterrupt();
        }

    }
}
