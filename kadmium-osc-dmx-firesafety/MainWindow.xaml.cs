﻿using Bespoke.Common.Osc;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace kadmium_osc_dmx_firesafety
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private DispatcherTimer updateTimer;
        private SafetyStatus safetyStatus;
        private UdpClient udpClient;
        private DirectInput directInput;
        private Joystick joystick;
        
        public MainWindow()
        {
            InitializeComponent();
            Dispatcher.ShutdownStarted += Dispatcher_ShutdownStarted;

            safetyStatus = Resources["Status"] as SafetyStatus;
            
            directInput = new DirectInput();
            udpClient = new UdpClient();
            InitJoystick();
            InitTimer();
        }

        private void Dispatcher_ShutdownStarted(object sender, EventArgs e)
        {
            Properties.Settings.Default.Save();
            Dispose();
        }

        public void InitJoystick()
        {
            var inputDevice = directInput.GetDevices().SingleOrDefault(x => x.InstanceGuid == Properties.Settings.Default.InputDeviceGuid);
            if (inputDevice != null)
            {
                if(joystick != null)
                {
                    joystick.Unacquire();
                    joystick.Dispose();
                }
                joystick = new Joystick(directInput, inputDevice.InstanceGuid);
                joystick.Acquire();
            }
        }

        private void InitTimer()
        {
            if(updateTimer != null)
            {
                updateTimer.Stop();
            }
            var updateTime = new TimeSpan(0, 0, 0, 0, 1000 / Properties.Settings.Default.UpdateRate);
            updateTimer = new DispatcherTimer(updateTime, DispatcherPriority.Normal, onUpdate, Dispatcher);
        }

        private async void onUpdate(object caller, EventArgs args)
        {
            if (tabStrip.SelectedItem == tabStatus && udpClient != null)
            {
                bool joystickPressed = false;
                
                if(joystick != null)
                {
                    joystickPressed = joystick.GetCurrentState().Buttons.Any(x => x);
                }
                bool isSafe = joystickPressed || System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.Space);

                safetyStatus.Status = isSafe;
                string oscAddress = "/group/" + Properties.Settings.Default.Group + "/FireSafety";
                OscMessage message = new OscMessage(null, oscAddress, safetyStatus.StatusFloat);
                var bytes = message.ToByteArray();
                try
                {
                    IPAddress[] addresses = await Dns.GetHostAddressesAsync(Properties.Settings.Default.Hostname);
                    foreach(var address in addresses.Where(x => x.AddressFamily == AddressFamily.InterNetwork))
                    {
                        IPEndPoint endPoint = new IPEndPoint(address, Properties.Settings.Default.Port);
                        await udpClient.SendAsync(bytes, bytes.Length, endPoint);
                    }
                    
                }
                catch(Exception e)
                {
                    safetyStatus.Error = e;
                }
            }
        }
        
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = sender as TabControl;
            var selected = control.SelectedItem as TabItem;
            if(e.RemovedItems.Contains(tabSettings))
            {
                var selectedDevice = (cboDevices.SelectedItem as InputDeviceWrapper);
                Properties.Settings.Default.InputDeviceGuid = selectedDevice?.Device.InstanceGuid ?? Guid.Empty;
                Properties.Settings.Default.Save();
                InitJoystick();
                InitTimer();
            }
            else if(e.AddedItems.Contains(tabSettings))
            {
                var wrappers = directInput.GetDevices().Select(x => new InputDeviceWrapper(x)).ToList();
                cboDevices.ItemsSource = wrappers;
                var selectedWrapper = wrappers.SingleOrDefault(x => x.Device.InstanceGuid == Properties.Settings.Default.InputDeviceGuid);
                if (selectedWrapper != null)
                {
                    cboDevices.SelectedIndex = wrappers.IndexOf(selectedWrapper);
                }
            }
        }

        private void cboDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            
        }

        public void Dispose()
        {
            if(joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }
        }
    }
}
