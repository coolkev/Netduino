using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace LedTest1
{
    public class ServoController : IDisposable
    {
        private readonly int _minDuration;
        private readonly int _maxDuration;
        private readonly uint _period;
        private readonly PWM _servo;
        private int _range;

        public ServoController(Cpu.Pin pin, int minDuration, int maxDuration, uint period = 20000, int startPercent = 0)
        {
            _minDuration = minDuration;
            _maxDuration = maxDuration;
            _range = maxDuration - minDuration;
            _period = period;
            _servo = new PWM(pin);

            if (startPercent>0)
                Rotate(startPercent);
        }

        public void Rotate(int percent)
        {
            if (percent<0 || percent>100)
                throw new ArgumentException("percent");

            var duration = (uint) (_minDuration + (_range/100) * percent);
            _servo.SetPulse(_period, duration);
        }
        public void Dispose()
        {
            _servo.Dispose();
        }
    }
}
