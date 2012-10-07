using System;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace MotorShield
{
    public class ServoController : IDisposable
    {
        private readonly int _minDuration;
        private readonly int _maxDuration;
        private readonly uint _period;
        private readonly PWM _servo;
        private int _range;
        private int _position;

        public ServoController(Cpu.Pin pin, int minDuration, int maxDuration, uint period = 20000, int startDegree = 0)
        {
            _minDuration = minDuration;
            _maxDuration = maxDuration;
            _range = maxDuration - minDuration;
            _period = period;
            _servo = new PWM(pin);
            _servo.SetDutyCycle(0);

            if (startDegree > 0)
                Rotate(startDegree);
        }

        public int Position { get { return _position; } }

        public void Rotate(int degrees)
        {
            if (degrees < 0 || degrees > 180)
                throw new ArgumentException("percent");

            _position = degrees;
            var duration = (uint) (_minDuration + (_range/100d) * (degrees/180d*100d));

            _servo.SetPulse(_period, duration);
        }
        public void Dispose()
        {
            _servo.Dispose();
        }
    }
}
