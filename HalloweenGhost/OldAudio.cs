using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using System.Threading;

namespace HalloweenGhost
{
    /// <summary>
    /// The audio implementation for the sound module
    /// </summary>
    class OldAudio
    {
        OutputPort _ClockPin;
        OutputPort _DataPin;
        InputPort _BusyPin;
        int _CurrentSound = -1;
        public OldAudio(FEZ_Pin.Digital clock, FEZ_Pin.Digital data, FEZ_Pin.Digital busy)
        {
            _ClockPin = new OutputPort((Cpu.Pin)clock, true);
            _DataPin = new OutputPort((Cpu.Pin)data, false);
            _BusyPin = new InputPort((Cpu.Pin)busy, false, Port.ResistorMode.PullDown);
        }

        public void Play(int audioIndex)
        {
            if (_CurrentSound != audioIndex || !IsBusy())
            {
                _CurrentSound = audioIndex;
                SendCommand((ushort)audioIndex);
            }
        }

        public void TogglePause()
        {
            SendCommand(0xFFFE);
        }

        public void Stop()
        {
            SendCommand(0xFFFF);
        }

        public void SetVolume(byte level)
        {
            level = level > 7 ? (byte)7 : level;
            level = level < 0 ? (byte)0 : level;
            SendCommand((ushort)(0xFFF0 + level));
        }
        public bool IsBusy()
        {
            return _BusyPin.Read();
        }

        object _Lock = new object();
        void SendCommand(ushort command)
        {
            lock (_Lock)
            {
                //start bit
                _ClockPin.Write(false);
                Thread.Sleep(2);
                for (int mask = 0x8000; mask > 0; mask >>= 1)
                {
                    _DataPin.Write((command & mask) != 0);
                    _ClockPin.Write(false);
                    _ClockPin.Write(true);
                }
            }
        }
    }
}
