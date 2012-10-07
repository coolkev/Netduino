using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{
    public static class FindButton
    {

        public static void Main()
        {
            //Cpu.Pin[] pins = new Cpu.Pin[]
            //    {
            //        Pins.GPIO_PIN_A0, Pins.GPIO_PIN_A1, Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3, Pins.GPIO_PIN_A4, Pins.GPIO_PIN_A5, Pins.ONBOARD_SW1
            //        //Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1, Pins.GPIO_PIN_D2, Pins.GPIO_PIN_D3, Pins.GPIO_PIN_D4, Pins.GPIO_PIN_D5, Pins.GPIO_PIN_D6
            //        //, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8, Pins.GPIO_PIN_D9, Pins.GPIO_PIN_D10, Pins.GPIO_PIN_D11, Pins.GPIO_PIN_D12, Pins.GPIO_PIN_D13
            //    };

            //var ports = new InterruptPort[pins.Length];
            //var index = 0;
            //foreach (var pin in pins)
            //{
            //    var port = new InterruptPort(pin, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            //    ports[index] = port;
            //    index++;

            //    Cpu.Pin pin1 = pin;
            //    port.OnInterrupt += (data1, data2, time) =>
            //        {
            //            Debug.Print("Port " + pin1 + " pushed");
            //        };
            //}
            //foreach (var p in )
            var switchPort = new InterruptPort(Pins.GPIO_PIN_D0, false,
                                                     Port.ResistorMode.PullUp,
                                                     Port.InterruptMode.InterruptEdgeBoth);

            switchPort.OnInterrupt += (data1, data2, time) =>
                {
                    Debug.Print("Port pushed " + data2);
                };

            Debug.Print("Ready to find port");

            Thread.Sleep(20000);

        }
    }
}
