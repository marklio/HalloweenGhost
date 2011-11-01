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
            if (_UART != null && _UART.IsOpen)
            {
                _UART.Close();
                if (_ControlThread != null) _ControlThread.Join();
            }
            _UART = new SerialPort(PortNameBox.Text, 115200, Parity.None, 8, StopBits.One);
            _UART.Encoding = Encoding.UTF8;
            _UART.DataReceived += (s, ea) =>
            {
                var readBuffer = new byte[256];
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
            _UART.Open();
            _ControlThread = new Thread(() =>
            {
                var data = new byte[] { 0xA3, 90, 0, 0, 0, 0 };
                while (_UART.IsOpen)
                {
                    Thread.Sleep(20);
                    var state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
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
                    var throttlePos = state.ThumbSticks.Left.X;
                    var throttleRange = throttlePos < 0 ? 70 : 90;
                    var throttle = (byte)(90 + (throttlePos * throttleRange));
                    data[1] = throttlePos != 0 ? throttle : _ThrottlePosition;
                    data[2] = 0;
                    data[3] = 0;
                    data[4] = 0;
                    data[5] = (byte)((_Stop ? 2 : _Play ? 1 : 0) | (_LightOff ? 8 : _LightOn ? 4 : 0));
                    _Play = false;
                    _Stop = false;
                    _LightOn = false;
                    _LightOff = false;
                    _UART.Write(data, 0, 6);
                }
            }) { IsBackground = true };
            _ControlThread.Start();
        }

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
