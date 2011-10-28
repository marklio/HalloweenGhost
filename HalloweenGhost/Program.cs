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
        static SerialPort _UART = new SerialPort("COM1");
        const int BUFFER_LENGTH = 256;
        static byte[] _Buffer = new byte[BUFFER_LENGTH];
        public static void Main()
        {
            _UART.Open();
            bool ledState = false;

            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            var outBuffer = Encoding.UTF8.GetBytes("TEST");

            while (true)
            {
                // Sleep for 500 milliseconds
                Thread.Sleep(500);
                if (_UART.BytesToRead > 0)
                {
                    var read = _UART.Read(_Buffer, 0, BUFFER_LENGTH);

                    // toggle LED state
                    ledState = !ledState;
                    led.Write(ledState);
                }
                _UART.Write(outBuffer, 0, outBuffer.Length);
            }
        }

    }
}
