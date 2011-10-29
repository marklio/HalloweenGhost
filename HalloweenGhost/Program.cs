using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using System.IO.Ports;
using System.Text;

namespace HalloweenGhost
{
    public class Program
    {
        static SerialPort _UART = new SerialPort("COM3", 115200);//, 115200, Parity.None, 8, StopBits.One);
        const int BUFFER_LENGTH = 256;
        static byte[] _Buffer = new byte[BUFFER_LENGTH];
        public static void Main()
        {
            _UART.ErrorReceived += (s, e) =>
            {
                Debug.Print("ERROR:"+e.EventType.ToString());
            };
            _UART.Open();

            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            var outBuffer = Encoding.UTF8.GetBytes("THIS IS A TEST OF THE EMERGENCY BROADCAST SYSTEM");

            while (true)
            {
                // Sleep for 500 milliseconds
                if (_UART.BytesToRead > 0)
                {
                    var read = _UART.Read(_Buffer, 0, BUFFER_LENGTH);

                    // toggle LED state
                    ledState = !ledState;
                    led.Write(ledState);
                }
                if (_UART.BytesToWrite == 0)
                {
                    _UART.Write(outBuffer, 0, outBuffer.Length);
                    _UART.Flush();
                }
            }
        }

    }
}
