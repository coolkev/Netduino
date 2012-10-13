using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;

namespace MotorShield
{
    class UltraSonicSensor : IDisposable
    {


        //private static InterruptPort EchoPin = new InterruptPort(Pins.GPIO_PIN_D10, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
        //private static OutputPort TriggerPin = new OutputPort(Pins.GPIO_PIN_D12, false);

       // private static bool Running = true;

        private readonly InterruptPort _echoPin;
        private readonly OutputPort _triggerPin;

        public UltraSonicSensor(Cpu.Pin echoPin, Cpu.Pin triggerPin)
        {
            _echoPin = new InterruptPort(echoPin, true, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
            _triggerPin = new OutputPort(triggerPin, false);
            
            _echoPin.OnInterrupt += port_OnInterrupt;
            _echoPin.DisableInterrupt();
        }



        //public delegate void ReadingCallback(int millimeters);

        //private ReadingCallback _callback;

        //public int TakeReading()
        //{
        //    int result = 0;

        //    var reset = new ManualResetEvent(false);
        //    TakeReadingAsync(millimeters =>
        //        {
        //            result = millimeters;
        //            reset.Set();
        //        });

        //    reset.WaitOne();

        //    return result;
        //}
        private ManualResetEvent _reset;

        public int TakeReading()
        {
            _pulseWidthMm = 0;

            //_callback = callback;
            _reset = new ManualResetEvent(false);
            
            _echoPin.EnableInterrupt();
            _triggerPin.Write(false);
            Thread.Sleep(2);
            _triggerPin.Write(true);
            Thread.Sleep(10);
            _triggerPin.Write(false);
            Thread.Sleep(2);


            _reset.WaitOne(500,false);

            return _pulseWidthMm;
        }

        private int _ticks;
        private int _pulseWidthMm;

        private void port_OnInterrupt(uint port, uint state, DateTime time)
        {
            if (state == 0) // falling edge, end of pulse
            {
                int pulseWidth = (int)time.Ticks - _ticks;
                // valid for 20°C
                //int pulseWidthMilliSeconds = pulseWidth * 10 / 582;
                //valid for 24°C
                _pulseWidthMm= (int)(pulseWidth /58);
                //Debug.Print("Distance = " + pulseWidthMm.ToString() + " mm.");
                //_callback(pulseWidthMm);
                _reset.Set();
            }
            else
            {
                _ticks = (int)time.Ticks;
            }
            _echoPin.ClearInterrupt();
        }

        public void Dispose()
        {
            _echoPin.Dispose();
            _triggerPin.Dispose();
            
        }
    }
}
