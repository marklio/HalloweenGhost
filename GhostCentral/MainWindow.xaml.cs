using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Threading;
using Microsoft.Xna.Framework.Input;

namespace GhostCentral
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        SerialPort _UART;
        Thread _ControlThread;
        byte _ThrottlePosition = 90;
        //the following indicate that one of the represented operations should be sent
        bool _Play = false;
        bool _Stop = false;
        bool _LightOn = false;
        bool _LightOff = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            //close the serial port and wait for the control thread to end
            if (_UART != null && _UART.IsOpen)
            {
                _UART.Close();
                if (_ControlThread != null) _ControlThread.Join();
            }
            //Create the serial port and hook up the data received event
            _UART = new SerialPort(PortNameBox.Text, 115200, Parity.None, 8, StopBits.One);
            _UART.Encoding = Encoding.UTF8;
            _UART.DataReceived += (s, ea) =>
            {
                var readBuffer = new byte[256];
                //read whatever we've got and write it to the output box (lame)
                while (_UART.BytesToRead > 0)
                {
                    var readBytes = _UART.Read(readBuffer, 0, Math.Min(readBuffer.Length, _UART.BytesToRead));
                    var data = Encoding.UTF8.GetString(readBuffer, 0, readBytes);
                    Dispatch(() =>
                    {
                        OutputBox.Text += data;
                    });
                }
            };
            //open the port and start the control thread
            _UART.Open();
            _ControlThread = new Thread(() =>
            {
                //template for the data frame
                var data = new byte[] { 0xA3, 90, 0, 0, 0, 0 };
                //loop while the port is open
                while (_UART.IsOpen)
                {
                    //wait 20 milliseconds (this is lame, but things are very reliable this way so far
                    Thread.Sleep(20);
                    //read the state from a GamePad attached as player one
                    var state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
                    //read button states
                    if (state.IsButtonDown(Buttons.A))
                    {
                        _Play = true;
                    }
                    if (state.IsButtonDown(Buttons.B))
                    {
                        _Stop = true;
                    }
                    if (state.IsButtonDown(Buttons.X))
                    {
                        _LightOn = true;
                    }
                    if (state.IsButtonDown(Buttons.Y))
                    {
                        _LightOff = true;
                    }
                    //read the throttle position and calculate the servo position
                    var throttlePos = state.ThumbSticks.Left.X;
                    //get the range (the speed controller only supports -70 degrees (20 degrees is the min, this was discovered by experimentation)
                    var throttleRange = throttlePos < 0 ? 70 : 90;
                    var throttle = (byte)(90 + (throttlePos * throttleRange));
                    //use the UI position unless we got data from the controller (intended to be used to implement automation here in this app)
                    data[1] = throttlePos != 0 ? throttle : _ThrottlePosition;
                    //This data isn't used currently
                    data[2] = 0;
                    data[3] = 0;
                    data[4] = 0;
                    //encode audio and sound transition events
                    data[5] = (byte)((_Stop ? 2 : _Play ? 1 : 0) | (_LightOff ? 8 : _LightOn ? 4 : 0));
                    //clear the events so we don't have to deal with "bounce"
                    _Play = false;
                    _Stop = false;
                    _LightOn = false;
                    _LightOff = false;
                    //write the data
                    _UART.Write(data, 0, 6);
                }
            }) { IsBackground = true };
            _ControlThread.Start();
        }

        /// <summary>
        /// Easier to dispatch to the UI.
        /// </summary>
        void Dispatch(Action action) {
            Dispatcher.Invoke(action);
        }

        private void Throttle0Button_Click(object sender, RoutedEventArgs e)
        {
            _ThrottlePosition = 20;
        }

        private void Throttle45Button_Click(object sender, RoutedEventArgs e)
        {
            _ThrottlePosition = 55;
        }

        private void Throttle90Button_Click(object sender, RoutedEventArgs e)
        {
            _ThrottlePosition = 90;
        }

        private void Throttle135Button_Click(object sender, RoutedEventArgs e)
        {
            _ThrottlePosition = 135;
        }

        private void Throttle180Button_Click(object sender, RoutedEventArgs e)
        {
            _ThrottlePosition = 180;
        }

        private void Sound0Button_Click(object sender, RoutedEventArgs e)
        {
            _Play = true;
        }

        private void SoundStopButton_Click(object sender, RoutedEventArgs e)
        {
            _Stop = true;
        }
    }
}
