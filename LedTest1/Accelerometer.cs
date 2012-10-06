using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace LedTest1
{
    public class Accelerometer : IDisposable
    {
        public static int Precision { get; set; }
        private readonly OutputPort _sleepPort;

        public Accelerometer(Cpu.Pin sleepPin)
        {
            _sleepPort = new OutputPort(sleepPin, true);
        }

        public const double xAxisVoltage = 1.65;
        public const double yAxisVoltage = 1.70;
        public const double zAxisVoltage = 2.45;

        public class Axis : IDisposable
        {
            private readonly double _voltage;
            private readonly AnalogInput _analogInput;

            public Axis(Cpu.Pin pin, Cpu.AnalogChannel analogChannel, double voltage)
            {
                _voltage = voltage;

                var ax = new InterruptPort(pin, false, Port.ResistorMode.Disabled, Port.InterruptMode.InterruptEdgeBoth);
                ax.Dispose();

                _analogInput = new AnalogInput(analogChannel);
            }


            public double Read()
            {
                var analogValue = _analogInput.Read();

                // now, if we want, we can calculate the number of volts by multiplying
                // the converted value by 3.3
                var voltageValue = analogValue * 3.3;//convert analog_x-->voltage value(v)

                var rangeValue = voltageValue - _voltage;

                var gramValue = rangeValue / 0.8;//calculate the gram value

                if (gramValue > 1)
                    return 90;
                if (gramValue < -1)
                    return -90;

                var degrees = System.Math.Asin(gramValue) * 180.0 / System.Math.PI;

                var multiplier = System.Math.Pow(10, Precision);
                return System.Math.Round(degrees*multiplier)/multiplier;

            }

            public void Dispose()
            {
                _analogInput.Dispose();
            }
        }

        public Axis xAxis { get; set; }
        public Axis yAxis { get; set; }
        public Axis zAxis { get; set; }

        public Result Read()
        {
            return new Result() {X = xAxis.Read(), Y = yAxis.Read(), Z = zAxis.Read()};
        }

        public class Result
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            public override string ToString()
            {
                var format = "N" + Precision;
                return "X: " + X.ToString(format) + " Y: " + Y.ToString(format) + " Z: " + Z.ToString(format);
            }
        }

        public void Dispose()
        {
            _sleepPort.Dispose();

            if (xAxis != null)
                xAxis.Dispose();
            if (yAxis != null)
                yAxis.Dispose();
            if (zAxis != null)
                zAxis.Dispose();

            
        }
    }
}
