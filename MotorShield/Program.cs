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

        public static void Main()
        {
            var MyMotors = new Mshield(Mshield.Drivers.Both);

            //var servo = new PWM(MyMotors.Servo1);

            //servo.SetPulse(20000,700);
            //MyMotors.MotorControl(Mshield.Motors.M4, (byte)100,true,10000);
            MyMotors.MotorControl(Mshield.Motors.M1,1, true);
            Thread.Sleep(1000);

            MyMotors.MotorControl(Mshield.Motors.M1, 0, true);

            //var ledState = false;
            //OutputPort led = new OutputPort(Pins.ONBOARD_LED, ledState);

            //while (true)
            //{
            //    ledState = !ledState;
            //    led.Write(ledState);

            //    // We have one motor connected on port M4 (Warning ! on some shields, M3 and M4 indications are reversed !)
            //    // Increase its speed to its maximum. Direction change at each loop
            //    for (int i = 0; i <= 255; i++, Thread.Sleep(20)) MyMotors.MotorControl(Mshield.Motors.M4, (byte)i, ledState);
            //    // Then decrease it to zero
            //    for (int i = 255; i >= 0; i--, Thread.Sleep(20)) MyMotors.MotorControl(Mshield.Motors.M4, (byte)i, ledState);
            //    Thread.Sleep(1000);

            //    // We have one stepper connected on driver1 (M1 + M2 ports)
            //    // Have it run 5s in "Hi-Torque" (High power, high speed, high power usage...)
            //    //MyMotors.StepperMove(Mshield.Steppers.S1, Mshield.BipolarStepping.HiTorque, 0, true, 0, 5000, false);
            //    //Thread.Sleep(1000);

            //    //// Then 5s in the other direction
            //    //MyMotors.StepperMove(Mshield.Steppers.S1, Mshield.BipolarStepping.HiTorque, 0, false, 0, 5000, false);
            //    //Thread.Sleep(1000);
            //}
        }


    }
}
