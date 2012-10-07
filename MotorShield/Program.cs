using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Robot.Drivers.Adafruit;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{
    public class Program
    {
        private static Mshield _myMotors;
        private static UltraSonicSensor _sensor;
        private static Timer _timer;

        public static void Main()
        {
            Debug.Print("Started...");

            //var servo = new PWM(MyMotors.Servo1);

            //servo.SetPulse(20000,700);
            //MyMotors.MotorControl(Mshield.Motors.M4, (byte)100,true,10000);

            //Thread.Sleep(1000);
            _myMotors = new Mshield(Mshield.Drivers.Driver2);
            
            var button = new InterruptPort(Pins.GPIO_PIN_D2, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            button.OnInterrupt += (data1, data2, time) =>
                {
                    if (Moving)
                        StopMoving();
                    else
                        StartMoving();
                };

            _sensor = new UltraSonicSensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            
            Thread.Sleep(Timeout.Infinite);
        }


        private static void StartMoving()
        {
            Moving = true;


            _timer = new Timer(StopIfClose, null, 0, 30);
            Debug.Print("StartMoving");
            _myMotors.MotorControl2(Mshield.Motors.M3, 70, false);
            _myMotors.MotorControl2(Mshield.Motors.M4, 70, false);
        }

        protected static bool Moving { get; set; }

        private static void StopMoving()
        {
            Moving = false;

            if (_timer != null)
            _timer.Dispose();
            _timer = null;
            Debug.Print("StopMoving");

            _myMotors.MotorControl2(Mshield.Motors.M3, 0, false);
            _myMotors.MotorControl2(Mshield.Motors.M4, 0, false);


            //_myMotors.Dispose();
            //_myMotors = null;


        }
        private static void StopIfClose(object state)
        {
            if (!Moving) return;

            var distance = _sensor.TakeReading();
            if (!Moving) return;
            
            Debug.Print("Distance = " + distance.ToString() + " mm.");

            if (distance < 200)
            {
                StopMoving();
            }

        }


        


    }
}
