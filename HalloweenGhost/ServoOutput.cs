using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Hardware;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;

namespace HalloweenGhost
{
    public class ServoOutput : IDisposable
    {
        OutputCompare _OutputCompare;
        uint[] _Timings = new uint[5];

        public void Dispose()
        {
            _OutputCompare.Dispose();
        }

        public ServoOutput(FEZ_Pin.Digital pin) : this(pin, 90) { }

        public ServoOutput(FEZ_Pin.Digital pin, byte initialDegrees)
        {
            _OutputCompare = new OutputCompare((Cpu.Pin)pin, false, 5);
            SetPosition(initialDegrees);
        }

        public void SetPosition(byte degrees)
        {
            uint position = (uint)(((float)((2500 - 400) / 180) * (degrees)) + 400);
            _Timings[0] = position;
            _Timings[1] = 50000;
            _OutputCompare.Set(true, _Timings, 0, 2, true);
        }
    }
}
