using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace HalloweenGhost
{
    class Audio
    {
        OutputPort _EnablePin;
        public Audio(FEZ_Pin.Digital ioPin)
        {
            _EnablePin = new OutputPort((Cpu.Pin)ioPin, false);
        }

        public void Play()
        {
            _EnablePin.Write(true);
        }

        public void Stop()
        {
            _EnablePin.Write(false);
        }
    }
}
