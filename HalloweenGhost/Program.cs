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
        const int BUFFER_LENGTH = 256;
        static byte[] _Buffer = new byte[BUFFER_LENGTH];
        public static void Main()
        {
            var controlData = new ControlData("COM3", 115200);
            var throttle = new ServoOutput(FEZ_Pin.Digital.Di2);
            var audio = new Audio(FEZ_Pin.Digital.Di3);
            var lightPin = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, false);
            controlData.PlaySound += () =>
            {
                audio.Play();
            };
            controlData.StopSound += () =>
            {
                audio.Stop();
            };
            controlData.TurnOnLight += () =>
            {
                lightPin.Write(true);
            };
            controlData.TurnOffLight += () =>
            {
                lightPin.Write(false);
            };

            bool ledState = false;
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            var outBuffer = Encoding.UTF8.GetBytes("ALIVE\r");

            var decoder = Encoding.UTF8.GetDecoder();

            var aliveTimer = new Timer((o) =>
            {
                controlData.Write(outBuffer, 0, outBuffer.Length);
            }, null, 0, 1000);
            var throttleTimer = new Timer((o) =>
            {
                throttle.SetPosition(controlData.ThrottlePosition);
                ledState = !ledState; 
                led.Write(ledState);
            }, null, 0, 20);
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
