using System;
using Microsoft.SPOT;
using System.IO.Ports;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace HalloweenGhost
{
    public delegate void EventHandler();

    /// <summary>
    /// Encapsulates the control data coming from a remote controlling device
    /// </summary>
    public class ControlData
    {
        byte[] _Data = new byte[5];
        SerialPort _Port;
        Thread _ReaderThread;
        TimeSpan _LastReadTime = TimeSpan.MinValue;

        public ControlData(string port, int baud)
        {
            _Port = new SerialPort(port, baud, Parity.None, 8, StopBits.One);
            _ReaderThread = new Thread(Reader);
            _ReaderThread.Priority = ThreadPriority.Highest;
            _ReaderThread.Start();

        }

        #region Data Members

        public event EventHandler PlaySound;
        public event EventHandler StopSound;
        public event EventHandler TurnOnLight;
        public event EventHandler TurnOffLight;

        TimeSpan _ConnectionTimeout = TimeSpan.FromTicks(TimeSpan.TicksPerSecond);
        public bool Connected
        {
            get { return (_LastReadTime != TimeSpan.MinValue) && (Utility.GetMachineTime() - _LastReadTime) < _ConnectionTimeout; }
        }

        byte GetDataByte(int index, byte dflt)
        {
            if (_Data == null || !Connected)
            {
                return dflt;
            }
            lock (_Data)
            {
                return _Data[index];
            }
        }

        public byte ThrottlePosition
        {
            get
            {
                return GetDataByte(0, 90);
            }
        }


        byte SoundFile
        {
            get
            {
                return GetDataByte(1, 0);
            }
        }

        [Obsolete]
        public byte Green
        {
            get
            {
                return GetDataByte(2, 0);
            }
        }

        [Obsolete]
        public byte Blue
        {
            get
            {
                return GetDataByte(3, 0);
            }
        }

        bool SoundOn
        {
            get
            {
                return (GetDataByte(4, 0) & 0x01) > 0;
            }
        }

        bool SoundStop
        {
            get
            {
                return (GetDataByte(4, 0) & 0x02) > 0;
            }
        }

        bool LightOn
        {
            get
            {
                return (GetDataByte(4, 0) & 0x04) > 0;
            }
        }

        bool LightOff
        {
            get
            {
                return (GetDataByte(4, 0) & 0x08) > 0;
            }
        }

        #endregion

        public void Write(byte[] buffer, int offset, int count)
        {
            _Port.Write(buffer, offset, count);
        }

        byte[] _ReadArr = new byte[6];
        void Reader()
        {
            _Port.Open();
            _Port.ErrorReceived += (s, e) =>
            {
                Debug.Print("COM Error:" + e.EventType);
                if (e.EventType == SerialError.RXOver)
                {
                    _Port.DiscardInBuffer();
                }
            };
            _Port.DiscardInBuffer();
            while (true)
            {
                //just read data like crazy until we find the magic number
                while (true)
                {
                    var read = _Port.GuaranteedRead(_ReadArr, 0, 6);
                    Debug.Assert(read == 6, "Didn't read 6 bytes");
                    if (_ReadArr[0] == 0xA3) break;
                    _Port.GuaranteedRead(_ReadArr, 0, 1);
                }
                lock (_Data)
                {
                    Array.Copy(_ReadArr, 1, _Data, 0, 5);
                    _LastReadTime = Utility.GetMachineTime();
                }
                if (SoundOn)
                {
                    var playSound = PlaySound;
                    if (playSound != null) playSound();
                }
                else if (SoundStop)
                {
                    var stopSound = StopSound;
                    if (stopSound != null) stopSound();
                }
                if (LightOn)
                {
                    var turnOnLight = TurnOnLight;
                    if (turnOnLight != null) turnOnLight();
                }
                else if (LightOff)
                {
                    var turnOffLight = TurnOffLight;
                    if (turnOffLight != null) turnOffLight();
                }
            }
        }
    }
}
