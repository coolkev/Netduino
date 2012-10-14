using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using NetduinoSerbRemote;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{
    public class Program
    {

        private static bool _cbuttonPressed;
        private static bool _zbuttonPressed;
        private static OutputPort _redLed;
        private static OutputPort _greenLed;

        public static void Main()
        {
            var wiiChuck = new WiiChuck(true);
            var debugMode = Debugger.IsAttached;

            _redLed = new OutputPort(Pins.GPIO_PIN_D13, false);
            _greenLed = new OutputPort(Pins.GPIO_PIN_A0, false);

            //_speaker = new PWM(Pins.GPIO_PIN_D9);

            //_speakerThread = new Thread(PlaySound);
            //_speakerThread.Start();
            //_servo1 = new ServoController(Mshield.Servo1, 600, 2400, startDegree: 90);
            _robot = new TankRobot();


            while (!debugMode || Debugger.IsAttached)
            {
                // try to read the data from nunchucku
                if (wiiChuck.GetData())
                {

                    CheckButtons(wiiChuck.CButtonDown, wiiChuck.ZButtonDown);

                    if (!PlayingBack)
                        SetMotorSpeed(_robot, wiiChuck);

                    //var degrees = ((int) (90+(-90*wiiChuck.AccelerationXGs))/2)*2;

                    //if (degrees < 0)
                    //    degrees = 0;
                    //else if (degrees > 180)
                    //    degrees = 180;

                    //_servo1.Rotate(degrees);
                    //Debug.Print("AccelX = " + wiiChuck.AccelerationXGs + "   AccelY=" + wiiChuck.AccelerationYGs);
                }
            }
            
            wiiChuck.Dispose();

            _robot.Dispose();

            //_speaker.Dispose();
            //_speaker = null;

            //_servo1.Dispose();

            _redLed.Dispose();
            _greenLed.Dispose();

        }

        private static void CheckButtons(bool cButtonDown, bool zButtonDown)
        {
            if (cButtonDown)
            {
                _cbuttonPressed = true;
                //_speaker.SetPulse(1654, 1654 / 2);
            }
            else if (_cbuttonPressed)
            {
                _cbuttonPressed = false;

                if (Recording)
                    StopRecording();
                else
                    StartRecording();
            }

            if (zButtonDown)
            {
                _zbuttonPressed = true;
                //_speaker.SetPulse(1654, 1654 / 2);
            }
            else if (_zbuttonPressed)
            {
                _zbuttonPressed = false;

                if (PlayingBack)
                    StopPlayingBack();
                else
                    StartPlayingBack();
            }
                        

        }


        private static void StartRecording()
        {

            Recording = true;
            _record.Clear();
            var now = Utility.GetMachineTime();
            _lastTime = TotalMilliseconds(now);
            _lastDataPoint = new DataPoint();
            Debug.Print("StartRecording");
            _startedRecording = now;
        }

        public class DataPoint
        {
            public int LeftSpeed { get; set; }
            public int RightSpeed { get; set; }
            //public TimeSpan Time { get; set; }

            public int Duration { get; set; }
        }

        public static int TotalMilliseconds(TimeSpan timespan)
        {
            var seconds = (timespan.Minutes * 60) + timespan.Seconds;
            var mili = (seconds * 1000) + timespan.Milliseconds;

            return mili;
        }
        private static void StopRecording()
        {
            Recording = false;
            var now = Utility.GetMachineTime();
            
            _lastDataPoint.Duration  =  TotalMilliseconds(now) - _lastTime;

            _record.Add(_lastDataPoint);

            _lastDataPoint = null;

            var totalDuration = TotalMilliseconds(now.Subtract(_startedRecording));
            Debug.Print("StopRecording " + totalDuration + "ms");

        }

        protected static bool Recording
        {
            get { return _recording; }
            set
            {
                _recording = value;
                _redLed.Write(value);
            }
        }


        private static void RecordData(int leftSpeed, int rightSpeed)
        {
            if (leftSpeed != _lastDataPoint.LeftSpeed || rightSpeed != _lastDataPoint.RightSpeed)
            {
                var now = Utility.GetMachineTime();
                var nowMili = TotalMilliseconds(now);
                var duration = nowMili - _lastTime;
                _lastTime = nowMili;

                _lastDataPoint.Duration = duration;

                _record.Add(_lastDataPoint);

                Debug.Print("RecordData LeftSpeed=" + _lastDataPoint.LeftSpeed + "  RightSpeed=" + _lastDataPoint.RightSpeed + "  for " + duration);

                _lastDataPoint = new DataPoint() {LeftSpeed = leftSpeed, RightSpeed = rightSpeed};


            }
        }

        private static void StartPlayingBack()
        {
            PlayingBack = true;

            _playbackThread = new Thread(PlayBackAsync);
            _playbackThread.Start();

        }

        private static int _currentStep;
        private static TimeSpan lastTime;

        private static void PlayBackAsync()
        {

            lastTime = Utility.GetMachineTime();
            var started = lastTime;

            var expectedDuration = 0;
            //var actualDuration = 0;
            
            foreach (DataPoint record in _record)
            {
                if (!PlayingBack)
                    return;
                
                Debug.Print("Playing Back " + record.LeftSpeed + ", " + record.RightSpeed);
                _robot.Move(record.LeftSpeed, record.RightSpeed);

                var now = Utility.GetMachineTime();

                var diff = now.Subtract(lastTime);
                var diffMili = TotalMilliseconds(diff);

                var sleep = record.Duration - diffMili;
                //var actualDuration = TotalMilliseconds(now.Subtract(started));
                //Debug.Print("Off by " + (expectedDuration-actualDuration) + "ms");
                Thread.Sleep(sleep);

                lastTime = Utility.GetMachineTime();
                //var actual = TotalMilliseconds(now.Subtract(last));
                //Debug.Print("expected: " + sleep + " actual: " + actual);
                expectedDuration += record.Duration;

            }

            var totalDuration = TotalMilliseconds(Utility.GetMachineTime().Subtract(started));

            Debug.Print("Playback Finished " + totalDuration + "ms");
            Debug.Print("Expected Time " + expectedDuration + "ms");
            Debug.Print("Off By " + (totalDuration-expectedDuration) + "ms");

            PlayingBack = false;
        }

        private static void StopPlayingBack()
        {
           
            PlayingBack = false;
            _robot.Stop();

        }

        protected static bool PlayingBack
        {
            get { return _playingBack; }
            set
            {
                _playingBack = value;
                _greenLed.Write(value);
            }
        }

        private static int lastX;
        private static int lastY;
        private static bool _recording;
        private static bool _playingBack;
        private static ArrayList _record = new ArrayList();
        private static int _lastTime;
        private static DataPoint _lastDataPoint;
        private static TankRobot _robot;
        private static Thread _playbackThread;
        private static TimeSpan _startedRecording;

        private static void SetMotorSpeed(TankRobot robot, WiiChuck wiiChuck)
        {

            var y = (int)(wiiChuck.AnalogY * 100);

            if (System.Math.Abs(y) < 10)
                y = 0;

            var x = (int)(wiiChuck.AnalogX * 100);

            if (System.Math.Abs(x) < 10)
                x = 0;

            var leftSpeed = y;
            var rightSpeed = y;

            leftSpeed += (int)(x * (1 - (y / 200f)));
            rightSpeed -= (int)(x * (1 - (y / 200f)));

            if (leftSpeed > 100)
                leftSpeed = 100;

            if (rightSpeed > 100)
                rightSpeed = 100;
            if (x != lastX || y != lastY)
            {
                Debug.Print("X,Y = " + x + "," + y + " \t L,R = " + leftSpeed + "," + rightSpeed);
                lastX = x;
                lastY = y;
            }

            if (Recording)
                RecordData(leftSpeed, rightSpeed);

            robot.Move(leftSpeed, rightSpeed);

        }

        //private static void PlaySound()
        //{
        //    const int timeToPlay = 500;

        //    while (_speaker!=null)
        //    {
        //        if (_playSound)
        //        {

        //            _speaker.SetPulse(1654, 1654 / 2);

        //            //if (_playSound)
        //            //{
        //            //    play(1543, timeToPlay);
        //            //}
        //        }
        //        else
        //        {
        //            _speaker.SetDutyCycle(0);
        //            Thread.Sleep(50);

        //        }
                
        //    }
        //}

        //static void play(int tone, int duration)
        //{
        //    uint period = (uint)(tone);
        //    _speaker.SetPulse(period, period / 2);
        //    Thread.Sleep(duration);
        //}
        //public static void Main()
        //{


        //    TestAccelerometer();
        //    //var _ultrasensor = new UltraSonicSensor(Pins.GPIO_PIN_D0, Pins.GPIO_PIN_D1);

        //    //var debugMode = Debugger.IsAttached;

        //    //while (!debugMode || Debugger.IsAttached)
        //    //{

        //    //    var distance = _ultrasensor.TakeReading();
        //    //    Debug.Print(distance.ToString());
        //    //    Thread.Sleep(100);

        //    //}
        //    using (var robot = new TankRobot())
        //    {

        //        //var timer = new Timer(new TimerCallback( state =>
        //        //    {
        //        //        var distance = robot.TakeReading();
        //        //        Debug.Print(distance.ToString());
        //        //    }));

        //        //robot.Rotate(TankRobot.RotateDirection.Left, 360,70);

        //        //robot.Forward(200,100);
        //        //robot.Run();
        //        //for (var x = 0; x < 36; x++)
        //        //{
        //        //    robot.Rotate(TankRobot.RotateDirection.Left, 10, 75);
        //        //    var distance = robot.TakeReading();
        //        //    Debug.Print(distance.ToString());
        //        //}
        //        //for (var x = 0; x < 36; x++)
        //        //{
        //        //    robot.Rotate(TankRobot.RotateDirection.Right, 10, 75);
        //        //    var distance = robot.TakeReading();
        //        //    Debug.Print(distance.ToString());
        //        //}

        //    }

        //}

        //private static void TestAccelerometer()
        //{
            
            
        //    const int readings = 10;

        //    Accelerometer.Precision = 2;
        //    using (var accel = new Accelerometer(Pins.GPIO_PIN_A2))
        //    {

        //        accel.xAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A0, Cpu.AnalogChannel.ANALOG_0,
        //                                             Accelerometer.xAxisVoltage);
        //        accel.zAxis = new Accelerometer.Axis(Pins.GPIO_PIN_A1, Cpu.AnalogChannel.ANALOG_1,
        //                                             Accelerometer.zAxisVoltage);

        //        var q = new Queue();
        //        double total = 0;

        //        // Keep application alive via loop
        //        while (Debugger.IsAttached)
        //        {

        //            while (q.Count < readings)
        //            {
        //                var reading = accel.Read();
        //                var value = reading.Z;

        //                q.Enqueue(value);
        //                total += value;

        //            }
        //            var avg = total/readings;

        //            total -= (double) q.Dequeue();

        //            var rotation = ((avg + 90)/180)*100;
        //            //var adjusted = 1000 + (int)rotation;

        //            Debug.Print("avg=" + avg + ", rotation=" + rotation);


        //        }
        //    }
        //}

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
