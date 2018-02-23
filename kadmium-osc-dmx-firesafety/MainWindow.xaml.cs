using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    public partial class MainWindow : Window
    {
        private DispatcherTimer updateTimer;
        private TimeSpan updateTime = new TimeSpan(0, 0, 0, 0, 250);
        private EventHandler updateHandler;
        private SafetyStatus safetyStatus;
        private UdpClient udpClient;
        
        public MainWindow()
        {
            InitializeComponent();
            safetyStatus = Resources["Status"] as SafetyStatus;
            
            updateHandler = new EventHandler(onUpdate);
            updateTimer = new DispatcherTimer(updateTime, DispatcherPriority.Normal, onUpdate, Dispatcher);
            
        }

        private async void onUpdate(object caller, EventArgs args)
        {
            if (tabStrip.SelectedItem == tabStatus && udpClient != null)
            {
                safetyStatus.Status = Keyboard.IsKeyDown(Key.Space);
                string oscAddress = "/group/" + Properties.Settings.Default.Group + "/FireSafety";
                Bespoke.Common.Osc.OscMessage message = new Bespoke.Common.Osc.OscMessage(null, oscAddress, safetyStatus.StatusFloat);
                var bytes = message.ToByteArray();
                await udpClient.SendAsync(bytes, bytes.Length);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var control = sender as TabControl;
            var selected = control.SelectedItem as TabItem;
            if (selected == tabStatus)
            {
                Properties.Settings.Default.Save();
                udpClient = new UdpClient(Properties.Settings.Default.Hostname, Properties.Settings.Default.Port);
            }
        }
    }
}
