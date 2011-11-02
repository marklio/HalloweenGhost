using System;
using System.Threading;

using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

using GHIElectronics.NETMF.FEZ;
using System.IO.Ports;
using System.Text;

namespace HalloweenGhost
{
    /// <summary>
    /// Main Program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Ummm.  Main
        /// </summary>
        public static void Main()
        {
            //The object that handles receiving data
            var controlData = new ControlData("COM3", 115200);
            //The servo output to control the ghost drive
            var throttle = new ServoOutput(FEZ_Pin.Digital.Di2);
            //The object that controls the audio
            var audio = new Audio(FEZ_Pin.Digital.Di3);
            //The output pin that controls the LEDs
            var lightPin = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.Di4, false);
            //hook up handlers for the various control events
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

            //This is for the onboard LED
            bool ledState = false;
            OutputPort led = new OutputPort((Cpu.Pin)FEZ_Pin.Digital.LED, ledState);

            //silly buffer we'll send as an indicator of life
            var outBuffer = Encoding.UTF8.GetBytes("ALIVE\r");

            //This timer will fire once a second and we'll jsut write out the life indicator
            //TODO: make use of this to convey state, etc.
            var aliveTimer = new Timer((o) =>
            {
                controlData.Write(outBuffer, 0, outBuffer.Length);
            }, null, 0, 1000);
            //We'll udpate the throttle every 20 milliseconds... because we can.
            var throttleTimer = new Timer((o) =>
            {
                throttle.SetPosition(controlData.ThrottlePosition);
                //toggle the LED
                ledState = !ledState; 
                led.Write(ledState);
            }, null, 0, 20);
            //we're done, let the timers take over
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
