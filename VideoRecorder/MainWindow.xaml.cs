using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;

namespace VideoRecorder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer dt = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        string currentTime = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            SystemEvents.SessionEnding += new SessionEndingEventHandler(SystemEvents_SessionEnding);
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if (Environment.HasShutdownStarted)
            {
                System.Windows.MessageBox.Show("Total amount of time the system logged on:" + ClockTextBlock.Text);
            }

        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                // ...
                case SessionSwitchReason.SessionLock:
                    if (stopWatch.IsRunning)
                        stopWatch.Stop();
                    break;
                case SessionSwitchReason.SessionUnlock:
                    stopWatch.Start();
                    dt.Start();
                    break;
                case SessionSwitchReason.SessionLogoff:
                    System.Windows.MessageBox.Show("Total amount of time the system logged on:" + ClockTextBlock.Text);
                    break;
                    // ...
            }
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                currentTime = String.Format("{0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds);
                ClockTextBlock.Text = currentTime;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            stopWatch.Start();
            dt.Start();
            if (stopWatch.IsRunning) { 
                stopWatch.Reset();
                stopWatch.Start();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (stopWatch.IsRunning)
                stopWatch.Stop();
        }
    }
}
