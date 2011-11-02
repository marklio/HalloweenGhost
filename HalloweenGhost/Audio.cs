using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace HalloweenGhost
{
    /// <summary>
    /// Abstracts the audio (I did this originally when we were doing more with audio)
    /// </summary>
    class Audio
    {
        /// <summary>
        /// The pin controlling the audio (connected to a transistor that switches on the power to the
        /// audio module)
        /// </summary>
        OutputPort _EnablePin;
        public Audio(FEZ_Pin.Digital ioPin)
        {
            _EnablePin = new OutputPort((Cpu.Pin)ioPin, false);
        }

        /// <summary>
        /// Turn on the audio
        /// </summary>
        public void Play()
        {
            _EnablePin.Write(true);
        }

        //turn off the audio
        public void Stop()
        {
            _EnablePin.Write(false);
        }
    }
}
