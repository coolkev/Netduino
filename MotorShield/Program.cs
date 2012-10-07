using System;
using System.Diagnostics;
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
        //private static Mshield _myMotors;
        //private static UltraSonicSensor _sensor;
        ////private static Timer _timer;
        //private static ServoController _servo1;

        //private static int _leftMotorSpeed;
        //private static int _rightMotorSpeed;

        //private static Thread _sonicThread;
        //private static Thread _servoThread;

        public static void Main()
        {

            using (var robot = new TankRobot())
            {

                robot.Run();
                //robot.Rotate(180);


            }

        }

        //public static void Main()
            //{


            //    _servo1 = new ServoController(Robot.Drivers.Adafruit.Mshield.Servo1, 600, 3000, startDegree: 90);

            //    while (Debugger.IsAttached)
            //    {

            //        var _servoPosition = 0;
            //        while (Debugger.IsAttached)
            //        {
            //            _servoPosition = _servoPosition == 0 ? 180 : 0;

            //            _servo1.Rotate(_servoPosition);

            //            Thread.Sleep(1000);
            //        }
            //    }


            //}
            //public static void Main()
            //{

            //    _myMotors = new Mshield();

            //    var button = new InterruptPort(Pins.GPIO_PIN_D2, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            //    button.OnInterrupt += (data1, data2, time) =>
            //        {
            //            if (Moving)
            //                StopMoving();
            //            else
            //                StartMoving();
            //        };

            //    _sensor = new UltraSonicSensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            //    _servo1 = new ServoController(Robot.Drivers.Adafruit.Mshield.Servo1, 600, 3000,startDegree:90);

            //    //_servoThread = new Thread(new ThreadStart(ServoPatrol));
            //    //_servoThread.Start();

            //    //_sonicThread = new Thread(SonicPatrol);

            //    if (Debugger.IsAttached)
            //    {
            //        while (Debugger.IsAttached) 
            //            Thread.Sleep(1000);

            //    }
            //    else 
            //        Thread.Sleep(Timeout.Infinite);

            //    _myMotors.Dispose();



            //}

            //private static void ServoPatrol()
            //{
            //    var _servoPosition = 0;
            //    while (Debugger.IsAttached)
            //    {
            //        _servoPosition = _servoPosition == 0 ? 180 : 0;

            //        _servo1.Rotate(_servoPosition);

            //    }
            //}


            //private static void StartMoving()
            //{
            //    Moving = true;


            //    //_timer = new Timer(StopIfClose, null, 0, 30);
            //    Debug.Print("StartMoving");
            //    _myMotors.MotorControl2(Mshield.Motors.M3, 70, false);
            //    _myMotors.MotorControl2(Mshield.Motors.M4, 70, false);
            //}

            //protected static bool Moving { get; set; }

            //private static void StopMoving()
            //{
            //    Moving = false;

            //    //if (_timer != null)
            //    //_timer.Dispose();
            //    //_timer = null;
            //    Debug.Print("StopMoving");

            //    _myMotors.MotorControl2(Mshield.Motors.M3, 0, false);
            //    _myMotors.MotorControl2(Mshield.Motors.M4, 0, false);


            //    //_myMotors.Dispose();
            //    //_myMotors = null;


            //}

            //private static void SonicPatrol()
            //{

            //    while (true)
            //    {

            //        if (Moving)
            //        {
            //            var distance = _sensor.TakeReading();

            //            Debug.Print("Distance = " + distance.ToString() + " mm.");

            //            if (Moving && distance < 200)
            //            {
            //                StopMoving();
            //            }
            //        }

            //        Thread.Sleep(60);

            //    }

            //}



       

    }
}
