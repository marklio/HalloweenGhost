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
        /// <summary>
        /// Holds the "current" control state
        /// </summary>
        byte[] _Data = new byte[5];
        /// <summary>
        /// The serial port
        /// </summary>
        SerialPort _Port;
        /// <summary>
        /// The thread reading from the serial port
        /// </summary>
        Thread _ReaderThread;
        /// <summary>
        /// The "last read time" (used to tell if the controller is still sending us data)
        /// </summary>
        TimeSpan _LastReadTime = TimeSpan.MinValue;

        /// <summary>
        /// Fires up the serial port and starts recieving data
        /// </summary>
        public ControlData(string port, int baud)
        {
            _Port = new SerialPort(port, baud, Parity.None, 8, StopBits.One);
            _ReaderThread = new Thread(Reader);
            _ReaderThread.Priority = ThreadPriority.Highest;
            _ReaderThread.Start();

        }

        #region Data Members

        /// <summary>
        /// Fires when we're supposed to play a sound
        /// </summary>
        public event EventHandler PlaySound;
        /// <summary>
        /// Fires when we're supposed to stop playing a sound
        /// </summary>
        public event EventHandler StopSound;
        /// <summary>
        /// Fires when we're supposed to turn on the light
        /// </summary>
        public event EventHandler TurnOnLight;
        /// <summary>
        /// Fires when we're supposed to turn off the light
        /// </summary>
        public event EventHandler TurnOffLight;

        TimeSpan _ConnectionTimeout = TimeSpan.FromTicks(TimeSpan.TicksPerSecond);
        /// <summary>
        /// Indicates connectedness
        /// </summary>
        public bool Connected
        {
            get { return (_LastReadTime != TimeSpan.MinValue) && (Utility.GetMachineTime() - _LastReadTime) < _ConnectionTimeout; }
        }

        /// <summary>
        /// Gets a byte of data from the current state, or a default value
        /// </summary>
        byte GetDataByte(int index, byte defaultValue)
        {
            if (_Data == null || !Connected)
            {
                return defaultValue;
            }
            lock (_Data)
            {
                return _Data[index];
            }
        }

        /// <summary>
        /// Gets the current throttle position (or 90 if we're not connected)
        /// </summary>
        public byte ThrottlePosition
        {
            get
            {
                return GetDataByte(0, 90);
            }
        }

        /// <summary>
        /// The sound file to play (not used since we aren't using the new module)
        /// (was the red value when the ghost supported color)
        /// </summary>
        [Obsolete]
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

        /// <summary>
        /// Indicates if we requested to turn the sound on (not exposed)
        /// </summary>
        bool SoundOn
        {
            get
            {
                return (GetDataByte(4, 0) & 0x01) > 0;
            }
        }

        /// <summary>
        /// Indicates if we requested to turn the sound off (not exposed)
        /// </summary>
        bool SoundStop
        {
            get
            {
                return (GetDataByte(4, 0) & 0x02) > 0;
            }
        }

        /// <summary>
        /// Indicates if we requested to turn the light on (not exposed)
        /// </summary>
        bool LightOn
        {
            get
            {
                return (GetDataByte(4, 0) & 0x04) > 0;
            }
        }

        /// <summary>
        /// Indicates if we requested to turn the light off (not exposed)
        /// </summary>
        bool LightOff
        {
            get
            {
                return (GetDataByte(4, 0) & 0x08) > 0;
            }
        }

        #endregion

        /// <summary>
        /// Allows writing to the serial port
        /// </summary>
        public void Write(byte[] buffer, int offset, int count)
        {
            _Port.Write(buffer, offset, count);
        }

        /// <summary>
        /// Buffer used for reading
        /// </summary>
        byte[] _ReadArr = new byte[6];
        /// <summary>
        /// The implementation of the reader thread
        /// </summary>
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
                //fire any events that should be fired
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
