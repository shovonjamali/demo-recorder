using Demo_VLC_Player.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Wpf;

namespace Demo_VLC_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow //: Window
    {
        long mediaLength;        
        DispatcherTimer timerVideoPlayback;
        bool isDraging = false;

        public MainWindow()
        {
            InitializeComponent();
            //LoadVLCPlayer();
            //DataContext = new PlayerViewModel();
        }

        private void LoadVLCPlayer()
        {
            DirectoryInfo vlcLibDirectory = GetVLCLibDirectory();

            InitVLCPlayer(vlcLibDirectory);

            RegisterVLCEventHandler();

            ManageVLCButtons(status: false);
        }

        private static DirectoryInfo GetVLCLibDirectory()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            return vlcLibDirectory;
        }

        private void InitVLCPlayer(DirectoryInfo vlcLibDirectory)
        {
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
                "--file-logging",
                "-vvv",
                "--extraintf=logger",
                "--logfile=Logs.log"
            };

            //this.VlcControl?.Dispose();
            //this.VlcControl = new VlcControl();
            //this.VlcControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);
        }

        private void RegisterVLCEventHandler()
        {
            //this.VlcControl.SourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;
            //this.VlcControl.SourceProvider.MediaPlayer.EndReached += MediaPlayer_EndReached;            

            SeekbarControl.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Seekbar_MouseLeftButtonUp), true);
            VolumeControl.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Volume_MouseLeftButtonUp), true);
        }

        private void MediaPlayer_EndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            //this.VlcControl.SourceProvider.MediaPlayer.Position = 0;

            ManageVLCButtons(status: false);

            Dispatcher.Invoke(new Action(() =>
            {
                LengthLabel.Content = "00:00:00";
            }));
        }

        private void ManageVLCButtons(bool status)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                PauseButton.IsEnabled = status;
                StopButton.IsEnabled = status;
                ForwardButton.IsEnabled = status;
                BackwardButton.IsEnabled = status;
                StartButton.IsEnabled = !status;
            }));
        }

        private void MediaPlayer_Playing(object sender, VlcMediaPlayerPlayingEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => {                
                //mediaLength = this.VlcControl.SourceProvider.MediaPlayer.Length / 1000;
                
                LengthLabel.Content = MeidaTimeElapsed(mediaLength);

                SeekbarControl.Maximum = mediaLength; //- 1;
                SeekbarControl.SmallChange = 1;                
                SeekbarControl.LargeChange = Math.Min(10, GetSeconds() / 10);

                timerVideoPlayback.Start();                
            }));       
        }

        private long GetSeconds()
        {
            long v, sec, hr, min;
            sec = mediaLength;
            hr = sec / 3600;
            v = sec % 3600;
            min = v / 60;
            sec = v % 60;
            sec = Math.Min(10, sec / 10);
            return sec;
        }

        private string MeidaTimeElapsed(long timeLength) {
            var ts = TimeSpan.FromSeconds(timeLength);
            var timeElapsed = string.Format("{0:T}", ts);
            return timeElapsed;
        }

        private void TimerVideoPlayback_Tick(object sender, object e)
        {
            //if(this.VlcControl != null)
            //{
            //    long currentMediaTicks = this.VlcControl.SourceProvider.MediaPlayer.Time / 1000;
            //    CurrentPositionLabel.Content = MeidaTimeElapsed(currentMediaTicks);
                
            //    if (!isDraging)
            //    {
            //        if (mediaLength > 0)
            //            SeekbarControl.Value = currentMediaTicks;
            //        else
            //            SeekbarControl.Value = 0;
            //    }
            //}                        
        }
        
        //private void StartButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ManageVLCButtons(status: true);

        //    //this.VlcControl.SourceProvider.MediaPlayer.Log += (_, args) =>
        //    //{
        //    //    string message = $"libVlc : {args.Level} {args.Message} @ {args.Module}";
        //    //    System.Diagnostics.Debug.WriteLine(message);
        //    //};

        //    //this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri(@"H:\dev tool.mp4"));    // place local path here   
        //    //this.VlcControl.SourceProvider.MediaPlayer.Play(new Uri("https://www.radiantmediaplayer.com/media/bbb-360p.mp4"));    // place stream url here  

        //    //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = Convert.ToInt32(VolumeControl.Value);            

        //    timerVideoPlayback = new DispatcherTimer();
        //    timerVideoPlayback.Interval = TimeSpan.FromSeconds(1);
        //    timerVideoPlayback.Interval = TimeSpan.FromMilliseconds(10);
        //    timerVideoPlayback.Tick += new EventHandler(TimerVideoPlayback_Tick);
        //    //timerVideoPlayback.Start();
        //}

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            //this.VlcControl.SourceProvider.MediaPlayer.Pause();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteStop();
        }

        private void ExecuteStop()
        {
            ManageVLCButtons(status: false);

            //this.VlcControl?.Dispose();
            //this.VlcControl = null;

            Dispatcher.Invoke(new Action(() =>
            {
                LengthLabel.Content = "00:00:00";
            }));

            LoadVLCPlayer();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            //this.VlcControl.SourceProvider.MediaPlayer.Time += 10000;            
        }

        private void BackwardButton_Click(object sender, RoutedEventArgs e)
        {
            //if (this.VlcControl.SourceProvider.MediaPlayer.Time > 10000)
            //{
            //    this.VlcControl.SourceProvider.MediaPlayer.Time -= 10000;
            //}
            //else
            //{
            //    this.VlcControl.SourceProvider.MediaPlayer.Time = 0;
            //}
        }

        private void SeekbarControl_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (mediaLength > 0)
            {
                isDraging = false;
            }
        }

        private void Seekbar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (mediaLength > 0)
            {
                //this.VlcControl.SourceProvider.MediaPlayer.Time = (long)SeekbarControl.Value * 1000;
            }
        }

        private void SeekbarControl_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDraging = true;
        }

        private void Volume_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //this.VlcControl.SourceProvider.MediaPlayer.Audio.Volume = Convert.ToInt32(VolumeControl.Value);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //this.VlcControl?.Dispose();
            base.OnClosing(e);
        }

        private void TogglePlayer_IsCheckedChanged(object sender, EventArgs e)
        {
            if(TogglePlayer.IsChecked == true)
            {
                Console.WriteLine("Checked");
            }
            else
            {
                Console.WriteLine("Not checked");
            }
        }
    }
}
