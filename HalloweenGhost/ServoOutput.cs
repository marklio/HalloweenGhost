using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;

namespace HalloweenGhost
{
    /// <summary>
    /// Handles talking to a servo
    /// </summary>
    public class ServoOutput : IDisposable
    {
        /// <summary>
        /// The pin we will do servo output on (output compare lets us program accurate timing easily)
        /// </summary>
        OutputCompare _OutputCompare;
        /// <summary>
        /// Holds the timing data for the output pin
        /// </summary>
        uint[] _Timings = new uint[5];

        public void Dispose()
        {
            _OutputCompare.Dispose();
        }

        /// <summary>
        /// Creates servo output defaulting to 90 degrees
        /// </summary>
        public ServoOutput(FEZ_Pin.Digital pin) : this(pin, 90) { }

        /// <summary>
        /// Creates servo output with an initial position
        /// </summary>
        public ServoOutput(FEZ_Pin.Digital pin, byte initialDegrees)
        {
            _OutputCompare = new OutputCompare((Cpu.Pin)pin, false, 5);
            SetPosition(initialDegrees);
        }

        /// <summary>
        /// Sets the position of the servo (0-180 degrees)
        /// </summary>
        public void SetPosition(byte degrees)
        {
            uint position = (uint)(((float)((2500 - 400) / 180) * (degrees)) + 400);
            _Timings[0] = position;
            _Timings[1] = 50000;
            _OutputCompare.Set(true, _Timings, 0, 2, true);
        }
    }
}
