using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Robot.Drivers.Adafruit;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{

    public class DistanceReading
    {
        /// <summary>
        /// Distance in millimeters
        /// </summary>
        public int Distance { get; set; }
        public int Degrees { get; set; }
        public DateTime Time { get; set; }

        public double Inches { get { return Distance / 25.4; } }
        public double Feet { get { return Inches/12; } }
    }

    public class DistanceHistory
    {
        private readonly Queue _queue = new Queue();

        public void Add(DistanceReading reading)
        {
            if (_queue.Count > 100)
                _queue.Dequeue();

            _queue.Enqueue(reading);
        }

    }

    public class TankRobot : IDisposable
    {
        private readonly Mshield _myMotors;
        private readonly UltraSonicSensor _ultrasensor;
        //private static Timer _timer;
        private readonly ServoController _servo1;

        //private int _leftMotorSpeed;
        //private int _rightMotorSpeed;

        private readonly DistanceHistory _distanceHistory = new DistanceHistory();
        private readonly InterruptPort _onOffButton;
        private bool _doFunStuff;
        private bool _stopFunStuff;

        public TankRobot()
        {
            _onOffButton = new InterruptPort(Pins.GPIO_PIN_D13, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeHigh);
            _onOffButton.OnInterrupt += OnOffButton;

            _ultrasensor = new UltraSonicSensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

            _servo1 = new ServoController(Mshield.Servo1, 600, 2400, startDegree: 90);

            _myMotors = new Mshield();
            

        }


        public void Run()
        {
            var prevPosition = 0;

            var debugMode = Debugger.IsAttached;

            while (!debugMode || Debugger.IsAttached)
            {

                if (_doFunStuff)
                {
                    _doFunStuff = false;
                    DoFunStuff();
                }


                Thread.Sleep(100);
            }

        }

        public enum RotateDirection
        {
            Left,
            Right
        }
        private void DoFunStuff()
        {
            //var rand = (new Random()).Next(1);
            while (!_stopFunStuff)
            {

                //var direction = rand == 0 ? RotateDirection.Left : RotateDirection.Right;
                int distance = 0;

                int maxDistance = 0;
                int maxDistanceDegrees = 0;
                for (var x = 0; x < 12; x++)
                {
                    var degrees = 30*x;
                    distance = _ultrasensor.TakeReading();

                    //var reading = new DistanceReading() { Degrees = degrees, Distance = distance, Time = DateTime.Now };
                    //_distanceHistory.Add(reading);

                    //if (distance == 0)
                    //{
                    //Debugger.Break();
                    //}
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        maxDistanceDegrees = degrees;
                    }

                    Rotate(RotateDirection.Left, 30);
                    //Stop();
                    Thread.Sleep(200);

                    if (_stopFunStuff) return;
                }

                Thread.Sleep(1000);


                if (maxDistanceDegrees > 0)
                {
                    Rotate(RotateDirection.Right, (uint) (360 - maxDistanceDegrees));

                    Thread.Sleep(1000);

                }

                Forward(100);

                while (distance > 200)
                {

                    distance = _ultrasensor.TakeReading();

                    Thread.Sleep(50);
                    if (_stopFunStuff) return;

                }
                Stop();

                Thread.Sleep(1000);

                distance = _ultrasensor.TakeReading();

                if (distance < 100)
                    ReverseAndStop(1000);

                Thread.Sleep(1000);
                
                //Stop();
                //Forward(1000);
                //Stop();
            }
        }

        private void ReverseAndStop(int duration, int speed = 100)
        {
            Moving = true;

            _myMotors.BothMotors(-speed, -speed);

            Thread.Sleep(duration);

            Stop();
        }

        public void Forward(int speed = 100)
        {
            Moving = true;

            _myMotors.BothMotors(speed, speed);
        }
        
        public void ForwardAndStop(int duration, int speed = 100)
        {
            Moving = true;
            _myMotors.BothMotors(speed, speed);

            Thread.Sleep(duration);

            Stop();
        }

        //public void Run()
        //{
        //    var prevPosition = 0;

        //    var debugMode = Debugger.IsAttached;

        //    while (!debugMode || Debugger.IsAttached)
        //    {

        //        if (Moving)
        //        {
        //            int newPosition;

        //            if (_servo1.Position == 90)
        //            {
        //                newPosition = prevPosition == 0 ? 180 : 0;
        //            }
        //            else
        //            {
        //                prevPosition = _servo1.Position;
        //                newPosition = 90;
        //            }

        //            _servo1.Rotate(newPosition);

        //            Thread.Sleep(500);

        //            var distance = _ultrasensor.TakeReading();

        //            var reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
        //            _distanceHistory.Add(reading);

        //            if (Moving)
        //            {

        //                if (newPosition == 90)
        //                {
        //                    if (reading.Feet < 1)
        //                    {
        //                        BothMotorSpeed = 0;

        //                        Thread.Sleep(1000);

        //                        Rotate(90);

        //                        distance = _ultrasensor.TakeReading();

        //                        reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
        //                        _distanceHistory.Add(reading);

        //                        //if more than 5 feet just go this way. otherwise turn 90 more
        //                        if (reading.Feet < 5)
        //                        {

        //                            Thread.Sleep(1000);

        //                            Rotate(90);

        //                            var lastReading = reading;

        //                            distance = _ultrasensor.TakeReading();

        //                            reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
        //                            _distanceHistory.Add(reading);

        //                            if (reading.Distance < lastReading.Distance)
        //                            {
        //                                Thread.Sleep(1000);

        //                                Rotate(-90);

        //                                distance = _ultrasensor.TakeReading();

        //                                reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
        //                                _distanceHistory.Add(reading);

        //                            }
        //                        }

        //                        if (reading.Feet > 1)
        //                        {
        //                            BothMotorSpeed = 70;
        //                        }

        //                    }
        //                    else if (reading.Feet < 5)
        //                    {
        //                        BothMotorSpeed = 70;
        //                    }
        //                    else if (reading.Feet > 5)
        //                    {
        //                        BothMotorSpeed = 85;
        //                    }
        //                }
        //            }

        //        }
        //        else
        //        {
        //            Thread.Sleep(500);
        //        }
                

        //    }

        //}

        //protected int BothMotorSpeed
        //{
        //    set
        //    {
        //        _leftMotorSpeed = value;
        //        _rightMotorSpeed = value;
        //        _myMotors.BothMotors(value,value);
        //    }
        //}

        private bool Moving;

        private void OnOffButton(uint data1, uint data2, DateTime time)
        {

            if (Moving)
            {
                _stopFunStuff = true;

                Stop();
            }
            else
            {
                _stopFunStuff = false;
                _doFunStuff = true;
                //BothMotorSpeed = 70;
            }
        }

        public void Stop()
        {
            _myMotors.BothMotors(0, 0);
            Moving = false;
        }

        //protected int LeftMotorSpeed
        //{
        //    get { return _leftMotorSpeed; }
        //    set
        //    {
        //        if (_leftMotorSpeed != value)
        //        {
        //            _leftMotorSpeed = value;
        //            _myMotors.MotorControl(Mshield.Motors.M4, (uint) value, value < 0);
        //        }
        //    }
        //}

        //protected int RightMotorSpeed
        //{
        //    get { return _rightMotorSpeed; }
        //    set
        //    {
        //        if (_rightMotorSpeed != value)
        //        {
        //            _rightMotorSpeed = value;
        //            _myMotors.MotorControl(Mshield.Motors.M3, (uint) value, value < 0);
        //        }
        //    }
        //}

        private const int DefaultRotateSpeed = 100;
        public int RotateDuration360 = 2800; 
        public void Rotate(RotateDirection rotateDirection, uint degrees, int speed = DefaultRotateSpeed)
        {

            //var leftSpeed = LeftMotorSpeed;
            //var rightSpeed = RightMotorSpeed;

            var direction = rotateDirection== RotateDirection.Left ? 1 : -1;

            Moving = true;
            //LeftMotorSpeed = 67;
            //RightMotorSpeed = -67;
            _myMotors.BothMotors(direction * speed, -direction * speed);
            //depends on degrees

            var duration = degrees/360d*RotateDuration360;
            Thread.Sleep((int)duration);

            //_myMotors.BothMotors(leftSpeed, rightSpeed);
            Stop();
            
        }
        public void Dispose()
        {
            //LeftMotorSpeed = 0;
            //RightMotorSpeed = 0;

            _myMotors.Dispose();
            _servo1.Dispose();
            _ultrasensor.Dispose();

        }

        public int TakeReading()
        {
            return _ultrasensor.TakeReading();
        }
    }
}