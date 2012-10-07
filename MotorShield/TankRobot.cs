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

        private int _leftMotorSpeed;
        private int _rightMotorSpeed;

        private readonly DistanceHistory _distanceHistory = new DistanceHistory();
        private readonly InterruptPort _onOffButton;

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

                if (Moving)
                {
                    int newPosition;

                    if (_servo1.Position == 90)
                    {
                        newPosition = prevPosition == 0 ? 180 : 0;
                    }
                    else
                    {
                        prevPosition = _servo1.Position;
                        newPosition = 90;
                    }

                    _servo1.Rotate(newPosition);

                    Thread.Sleep(500);

                    var distance = _ultrasensor.TakeReading();

                    var reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
                    _distanceHistory.Add(reading);

                    if (Moving)
                    {

                        if (newPosition == 90)
                        {
                            if (reading.Feet < 1)
                            {
                                BothMotorSpeed = 0;

                                Thread.Sleep(1000);

                                Rotate(90);

                                distance = _ultrasensor.TakeReading();

                                reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
                                _distanceHistory.Add(reading);

                                //if more than 5 feet just go this way. otherwise turn 90 more
                                if (reading.Feet < 5)
                                {

                                    Thread.Sleep(1000);

                                    Rotate(90);

                                    var lastReading = reading;

                                    distance = _ultrasensor.TakeReading();

                                    reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
                                    _distanceHistory.Add(reading);

                                    if (reading.Distance < lastReading.Distance)
                                    {
                                        Thread.Sleep(1000);

                                        Rotate(-90);

                                        distance = _ultrasensor.TakeReading();

                                        reading = new DistanceReading() {Degrees = newPosition, Distance = distance, Time = DateTime.Now};
                                        _distanceHistory.Add(reading);

                                    }
                                }

                                if (reading.Feet > 1)
                                {
                                    BothMotorSpeed = 70;
                                }

                            }
                            else if (reading.Feet < 5)
                            {
                                BothMotorSpeed = 70;
                            }
                            else if (reading.Feet > 5)
                            {
                                BothMotorSpeed = 85;
                            }
                        }
                    }

                }
                else
                {
                    Thread.Sleep(500);
                }
                

            }

        }

        protected int BothMotorSpeed
        {
            set
            {
                _leftMotorSpeed = value;
                _rightMotorSpeed = value;
                _myMotors.BothMotors(value,value);
            }
        }

        private bool Moving
        {
            get { return LeftMotorSpeed != 0 || RightMotorSpeed != 0; }
        }

        private void OnOffButton(uint data1, uint data2, DateTime time)
        {

            if (Moving)
            {
                BothMotorSpeed = 0;
            }
            else
            {
                BothMotorSpeed = 70;
            }
        }

        protected int LeftMotorSpeed
        {
            get { return _leftMotorSpeed; }
            set
            {
                if (_leftMotorSpeed != value)
                {
                    _leftMotorSpeed = value;
                    _myMotors.MotorControl(Mshield.Motors.M4, (uint) value, value < 0);
                }
            }
        }

        protected int RightMotorSpeed
        {
            get { return _rightMotorSpeed; }
            set
            {
                if (_rightMotorSpeed != value)
                {
                    _rightMotorSpeed = value;
                    _myMotors.MotorControl(Mshield.Motors.M3, (uint) value, value < 0);
                }
            }
        }


        public void Rotate(int degrees)
        {

            var leftSpeed = LeftMotorSpeed;
            var rightSpeed = RightMotorSpeed;

            var direction = degrees > 0 ? 1 : -1;

            //LeftMotorSpeed = 67;
            //RightMotorSpeed = -67;
            _myMotors.BothMotors(direction * 70, -direction * 70);
            //depends on degrees

            var duration = System.Math.Abs(degrees)/90d*800d;
            Thread.Sleep((int)duration);

            _myMotors.BothMotors(leftSpeed, rightSpeed);

            
        }
        public void Dispose()
        {
            LeftMotorSpeed = 0;
            RightMotorSpeed = 0;

            _myMotors.Dispose();
            _servo1.Dispose();
            _ultrasensor.Dispose();

        }
    }
}