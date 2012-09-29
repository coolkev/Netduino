using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace LedTest1
{
    public class Program
    {
        public static void Main()
        {

            //const int firstpin = (int) Cpu.Pin.GPIO_Pin8;
            //const int pinCount = 3;

            var currentPin = 0;

            var leds = new[] {new OutputPort(Pins.GPIO_PIN_D8, false), new OutputPort(Pins.GPIO_PIN_D9, false), new OutputPort(Pins.GPIO_PIN_D10, false)};

            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            button.OnInterrupt += (data1, data2, time) =>
                {
                    leds[currentPin%3].Write(data2 == 1);

                    if (currentPin >= 3)
                        leds[(currentPin + 1)%3].Write(data2 == 1);

                    if (currentPin >= 6)
                        leds[(currentPin + 2)%3].Write(data2 == 1);

                    if (data2 == 0)
                    {
                        currentPin++;
                        if (currentPin > 6)
                            currentPin = 0;
                    }
                };


            Thread.Sleep(Timeout.Infinite);

        }

    }
}
