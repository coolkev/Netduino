using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace LedTest1
{
    class LedSequence
    {

        public static void Run()
        {

            var pins = new[]
                {
                    Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3,
                    Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6, Pins.GPIO_PIN_D7
                };

            var leds = new OutputPort[8];

            for (var x = 0; x < leds.Length; x++)
                leds[x] = new OutputPort(pins[x], false);

            var delay = 500;

            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);

            var reverse = -1;

            button.OnInterrupt += (data1, data2, time) =>
            {

                delay += reverse * 10;

                if (delay == 0 || delay == 500)
                    reverse *= -1;

            };

            while (true)
            {
                for (var x = 0; x < leds.Length - 1; x++)
                {
                    var led = leds[x];
                    led.Write(true);
                    Thread.Sleep(delay);
                    led.Write(false);
                }
                for (var x = leds.Length - 1; x > 0; x--)
                {
                    var led = leds[x];
                    led.Write(true);
                    Thread.Sleep(delay);
                    led.Write(false);
                }

            }

        }

    }
}
