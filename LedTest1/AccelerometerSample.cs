using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace LedTest1
{
    class AccelerometerSample
    {


        public static void Run()
        {

            var button = new InterruptPort(Pins.ONBOARD_SW1, false, Port.ResistorMode.Disabled,
                                           Port.InterruptMode.InterruptEdgeHigh);

            var buttonIndex = 0;

            button.OnInterrupt += (data1, data2, time) =>
                {
                    buttonIndex++;
                    if (buttonIndex == 3)
                        buttonIndex = 0;
                };
            const int readings = 10;

            Accelerometer.Precision = 2;
            using (var accel = new Accelerometer(Pins.GPIO_PIN_A4))
            {

                accel.xAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A1, Cpu.AnalogChannel.ANALOG_1,
                                                     Accelerometer.xAxisVoltage);
                accel.yAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A2, Cpu.AnalogChannel.ANALOG_2,
                                                     Accelerometer.yAxisVoltage);
                accel.zAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A3, Cpu.AnalogChannel.ANALOG_3,
                                                     Accelerometer.zAxisVoltage);

                using (var servo = new ServoController(Pins.GPIO_PIN_D9, 600, 3000,startPercent:50))
                {

                    var q = new Queue();
                    double total = 0;

                    // Keep application alive via loop
                    while (true)
                    {

                        while (q.Count < readings)
                        {
                            var reading = accel.Read();
                            double value =0;
                            switch (buttonIndex)
                            {
                                case 0:
                                    value = reading.Z;
                                    break;
                                case 1:
                                    value = reading.Y;
                                    break;
                                case 2:
                                    value = reading.Z;
                                    break;

                            }

                            q.Enqueue(value);
                            total += value;

                        }
                        var avg = total/readings;

                        total -= (double) q.Dequeue();

                        //double total = 0;
                        //for (var x = 0; x < readings; x++)
                        //{
                        //    var reading = accel.Read();
                        //    total += reading.Y/2;
                        //}
                        //var avg = total/readings;

                        //Thread.Sleep(5);

                        var rotation = ((avg+90)/180)*100;
                        //var adjusted = 1000 + (int)rotation;

                        Debug.Print("avg=" + avg + ", rotation=" + rotation);

                        servo.Rotate((int)rotation);

                    }

                }
            }
        }

    }
}
