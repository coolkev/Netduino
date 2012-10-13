using System;
using System.Collections;
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


            TestAccelerometer();
            //var _ultrasensor = new UltraSonicSensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            //var debugMode = Debugger.IsAttached;

            //while (!debugMode || Debugger.IsAttached)
            //{

            //    var distance = _ultrasensor.TakeReading();
            //    Debug.Print(distance.ToString());
            //    Thread.Sleep(100);

            //}
            using (var robot = new TankRobot())
            {

                //var timer = new Timer(new TimerCallback( state =>
                //    {
                //        var distance = robot.TakeReading();
                //        Debug.Print(distance.ToString());
                //    }));

                //robot.Rotate(TankRobot.RotateDirection.Left, 360,70);

                //robot.Forward(200,100);
                //robot.Run();
                //for (var x = 0; x < 36; x++)
                //{
                //    robot.Rotate(TankRobot.RotateDirection.Left, 10, 75);
                //    var distance = robot.TakeReading();
                //    Debug.Print(distance.ToString());
                //}
                //for (var x = 0; x < 36; x++)
                //{
                //    robot.Rotate(TankRobot.RotateDirection.Right, 10, 75);
                //    var distance = robot.TakeReading();
                //    Debug.Print(distance.ToString());
                //}

            }

        }

        private static void TestAccelerometer()
        {
            
            
            const int readings = 10;

            Accelerometer.Precision = 2;
            using (var accel = new Accelerometer(Pins.GPIO_PIN_A2))
            {

                accel.xAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A0, Cpu.AnalogChannel.ANALOG_0,
                                                     Accelerometer.xAxisVoltage);
                accel.zAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A1, Cpu.AnalogChannel.ANALOG_1,
                                                     Accelerometer.zAxisVoltage);

                var q = new Queue();
                double total = 0;

                // Keep application alive via loop
                while (Debugger.IsAttached)
                {

                    while (q.Count < readings)
                    {
                        var reading = accel.Read();
                        var value = reading.Z;

                        q.Enqueue(value);
                        total += value;

                    }
                    var avg = total/readings;

                    total -= (double) q.Dequeue();

                    var rotation = ((avg + 90)/180)*100;
                    //var adjusted = 1000 + (int)rotation;

                    Debug.Print("avg=" + avg + ", rotation=" + rotation);


                }
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
