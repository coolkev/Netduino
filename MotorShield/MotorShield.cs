/*  Adafruit motor "Shield v1.0" / See: http://www.ladyada.net/make/mshield/index.html
*  Driver v0.1a - Copyright 2011 Arron Chapman
* 
*  Licensed under the Apache License, Version 2.0 (the "License");
*  you may not use this file except in compliance with the License.
*  You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
*  Unless required by applicable law or agreed to in writing, software
*  distributed under the License is distributed on an "AS IS" BASIS,
*  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*  See the License for the specific language governing permissions and
*  limitations under the License. 
*  
 * 
 * This Adaptation is based on the code provided by Nicolas Ricquemaque
 * the original code can be found at http://code.tinyclr.com/project/323/adafruit-motor-shield-driver/
 * 
*/

using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace Robot.Drivers.Adafruit
{
    public class Mshield : IDisposable
    {
        //public enum Drivers : byte { None = 0, Driver1, Driver2, Both }
        public enum Motors : byte { M1 = 0, M2, M3, M4 }
        //public enum Steppers : byte { S1 = 0, S2 }
        //public enum BipolarStepping : byte { WaveDrive = 0, HiTorque, HalfStep }

        /// <summary>
        /// On board Servo1 connector, for external reference only
        /// </summary>
        public const Cpu.Pin Servo1 = Pins.GPIO_PIN_D10;
        /// <summary>
        /// On board Servo2 connector, for external reference only
        /// </summary>
        public const Cpu.Pin Servo2 = Pins.GPIO_PIN_D9;

        //private Drivers UsedDriver;
        //private OutputPort Motor1A, Motor1B; // Enable L293D driver #1a and #1b
        private PWM Motor2A, Motor2B; // Enable L293D driver #1a and #1b
        private OutputPort MotorLatch, MotorEnable, MotorClk, MotorData; // 74HCT595 commands
        byte latch_state; // Actual 74HCT595 output state

        // Steper sequences for each stepper. Have a look here : http://www.stepperworld.com/Tutorials/pgBipolarTutorial.htm
        //private byte[][] BipolarSteppingWaveDrive = new byte[][] {  new byte [] {4,2,8,16},
        //                                                            new byte [] {1,128,64,32}};
        //private byte[][] BipolarSteppingHiTorque = new byte[][] {   new byte [] {20,24,10,6},
        //                                                            new byte [] {129,192,96,33}};
        //private byte[][] BipolarSteppingHalfStep = new byte[][] {   new byte [] {4,20,16,24,8,10,2,6},
        //                                                            new byte [] {1,129,128,192,64,96,32,33}};

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="driver"> Which driver to initialize : 1=driver 1, 2=driver 2, 3=both</param>
        public Mshield()
        {
            //UsedDriver = driver;
            //if (driver == Drivers.Driver1 || driver == Drivers.Both)
            //{
            //    Motor1A = new OutputPort(Pins.GPIO_PIN_D11, false);
            //    Motor1B = new OutputPort(Pins.GPIO_PIN_D3, false);
            //}
            //if (driver == Drivers.Driver2 || driver == Drivers.Both)
            //{
                Motor2A = new PWM(Pins.GPIO_PIN_D5);
                Motor2A.SetDutyCycle(0);
                Motor2B = new PWM(Pins.GPIO_PIN_D6);
                Motor2B.SetDutyCycle(0);
            //}
            MotorLatch = new OutputPort(Pins.GPIO_PIN_D12, true);
            MotorEnable = new OutputPort(Pins.GPIO_PIN_D7, false);
            MotorClk = new OutputPort(Pins.GPIO_PIN_D4, true);
            MotorData = new OutputPort(Pins.GPIO_PIN_D8, true);

            latch_state = 0;
            latch_tx();
        }

        #region IDisposable Members
        public void Dispose()
        {
            latch_state = 0;
            latch_tx();

            //if (UsedDriver == Drivers.Driver1 || UsedDriver == Drivers.Both)
            //{
            //    Motor1A.Dispose();
            //    Motor1B.Dispose();
            //}
            //if (UsedDriver == Drivers.Driver2 || UsedDriver == Drivers.Both)
            //{
                Motor2A.Dispose();
                Motor2B.Dispose();
            //}
            MotorLatch.Dispose();
            MotorEnable.Dispose();
            MotorClk.Dispose();
            MotorData.Dispose();
        }
        #endregion IDisposable Members


        // Send byte to the 74HCT595 demux
        private void latch_tx()
        {
            MotorLatch.Write(false);
            for (int i = 8; i >= 0; i--)
            {
                MotorClk.Write(false);
                MotorData.Write((latch_state & (1 << i)) > 0);
                MotorClk.Write(true);
            }
            MotorLatch.Write(true);
        }

        /// <summary>
        /// Move a stepper. Only one param between steps and msec but be set different from 0.
        /// Blocking call.
        /// </summary>
        /// <param name="which">Which stepper to drive</param>
        /// <param name="mode">Stepping mode</param>
        /// <param name="speed">Number of ms between each step (0 : no pause=max speed)</param>
        /// <param name="direction">True = foward, False = backward</param>
        /// <param name="steps">If non-zero, number of steps to run</param>
        /// <param name="msec">If non-zero, number of ms to run</param>
        /// <param name="hold">True if stepper shall stay energized when task done</param>
        /// <returns>Number of steps ran</returns>
        //public uint StepperMove(Steppers which, BipolarStepping mode, int speed, bool direction, int steps, int msec, bool hold = false)
        //{
        //    byte[] MotorSteps = BipolarSteppingWaveDrive[(byte)which]; // Get the stepping pattern
        //    if (mode == BipolarStepping.HiTorque) MotorSteps = BipolarSteppingHiTorque[(byte)which];
        //    if (mode == BipolarStepping.HalfStep) MotorSteps = BipolarSteppingHalfStep[(byte)which];

        //    // Find where we stopped last time
        //    byte last;
        //    int pos;
        //    uint step = 0;
        //    if (which == 0) last = (byte)(latch_state & 0x1E);
        //    else last = (byte)(latch_state & 0xE1);
        //    for (pos = 0; pos < MotorSteps.Length; pos++) if (MotorSteps[pos] == last) break;
        //    if (pos == MotorSteps.Length) pos = 0;

        //    if (which == 0) { Motor1A.Write(true); Motor1B.Write(true); }
        //    else { Motor2A.SetDutyCycle(100); Motor2B.SetDutyCycle(100); }

        //    byte Mask = (byte)((which == 0) ? 0xE1 : 0x1E);

        //    if (steps > 0) // user chose to move from a number of steps
        //    {
        //        for (; step < steps; step++)
        //        {
        //            if (direction) pos = (pos + 1) % MotorSteps.Length;
        //            else if (--pos < 0) pos = MotorSteps.Length - 1;
        //            latch_state = (byte)((latch_state & Mask) | MotorSteps[pos]);
        //            latch_tx();
        //            if (speed > 0) Thread.Sleep(speed);
        //        }

        //    }
        //    else // So no number of steps, the user must have given a time to run
        //    {
        //        long endtime = DateTime.Now.Ticks + msec * TimeSpan.TicksPerMillisecond;
        //        for (; DateTime.Now.Ticks < endtime; step++)
        //        {
        //            if (direction) pos = (pos + 1) % MotorSteps.Length;
        //            else if (--pos < 0) pos = MotorSteps.Length - 1;
        //            latch_state = (byte)((latch_state & Mask) | MotorSteps[pos]);
        //            latch_tx();
        //            if (speed > 0) Thread.Sleep(speed);
        //        }
        //    }

        //    if (!hold) // Remove energy from stepper
        //    {
        //        if (which == 0) { Motor1A.Write(false); Motor1B.Write(false); }
        //        else { Motor2A.SetDutyCycle(0); Motor2B.SetDutyCycle(0); }
        //    }

        //    return step;
        //}

        ///// <summary>
        ///// Control a motor; Non-blocking call. Set speed to 0 to stop.
        ///// Frequency might need to be adjusted deppending on motor, or to match other PWMs
        ///// </summary>
        ///// <param name="which">Which motor to drive</param>
        ///// <param name="speed">Steed, between 0 and 255 (0 to stop, 255 = max speed)</param>
        ///// <param name="direction">True = foward, False = backward</param>
        ///// <param name="freq">Frequency in Hz of the PWM controlling the motor</param>
        //public void MotorControl(Motors which, byte speed, bool direction, int freq = 100)
        //{
        //    uint period = (uint)(1000000D / (double)freq);
        //    switch (which)
        //    {
        //        case Motors.M1:
        //            latch_state = (byte)((latch_state & 0xF3) | (direction ? 4 : 8));
        //            latch_tx();
        //            Motor1A.Write(speed > 0);
        //            break;

        //        case Motors.M2:
        //            latch_state = (byte)((latch_state & 0xED) | (direction ? 2 : 16));
        //            latch_tx();
        //            Motor1B.Write(speed > 0);
        //            break;
        //        case Motors.M3: // This motor can have its speed controlled through PWM                  
        //            if (speed == 0) Motor2A.SetDutyCycle(0);
        //            else
        //            {
        //                latch_state = (byte)((latch_state & 0xBE) | (direction ? 1 : 64));
        //                latch_tx();
        //                Motor2A.SetPulse(period, (uint)(period * (double)speed / 255D));
        //            }
        //            break;
        //        case Motors.M4: // This motor can have its speed controlled through PWM                  
        //            if (speed == 0) Motor2B.SetDutyCycle(0);
        //            else
        //            {
        //                latch_state = (byte)((latch_state & 0x5F) | (direction ? 32 : 128));
        //                latch_tx();
        //                Motor2B.SetPulse(period, (uint)(period * (double)speed / 255D));
        //            }
        //            break;
        //    }
        //}

        public void BothMotors(int motor3Speed, int motor4Speed)
        {
            if (motor3Speed==0)
                Motor2A.SetDutyCycle(0);
            if (motor4Speed==0)
                Motor2A.SetDutyCycle(0);

            var motor3Dir = motor3Speed < 0;
            var motor4Dir = motor4Speed < 0;

             //var motor3latch_state = (byte) ((latch_state & 0xBE) | (motor3Dir ? 1 : 64));

             //var motor4latch_state = (byte) ((latch_state & 0x5F) | (motor4Dir ? 32 : 128));


             //latch_state = (byte) (motor3latch_state & motor4latch_state);

            latch_state = (byte)((latch_state & 0xBE) | (motor3Dir ? 1 : 64));
            latch_tx();
            Motor2A.SetDutyCycle((uint) Math.Abs(motor3Speed));

            latch_state = (byte)((latch_state & 0x5F) | (motor4Dir ? 32 : 128));
            latch_tx();
            
            Motor2B.SetDutyCycle((uint) Math.Abs(motor4Speed));

        }

        public void MotorControl(Motors which, uint speed, bool direction)
        {
            if (speed >= 100)
                speed = 100;

            switch (which)
            {
                case Motors.M3: // This motor can have its speed controlled through PWM                  
                    if (speed == 0) Motor2A.SetDutyCycle(0);
                    else
                    {
                        latch_state = (byte) ((latch_state & 0xBE) | (direction ? 1 : 64));
                        latch_tx();
                        Motor2A.SetDutyCycle(speed);
                    }
                    break;
                case Motors.M4: // This motor can have its speed controlled through PWM                  
                    if (speed == 0) Motor2B.SetDutyCycle(0);
                    else
                    {
                        latch_state = (byte) ((latch_state & 0x5F) | (direction ? 32 : 128));
                        latch_tx();
                        Motor2B.SetDutyCycle(speed);

                    }
                    break;

            }
        }
    }
}