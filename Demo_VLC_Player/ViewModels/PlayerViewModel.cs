using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Demo_VLC_Player.Models;
using Demo_VLC_Player.Commands;
using Vlc.DotNet.Wpf;
using System.Windows;
using Vlc.DotNet.Core;
using System.Timers;
using System.Windows.Input;

namespace Demo_VLC_Player.ViewModels
{
    public class PlayerViewModel
    {
        // original link: https://stackoverflow.com/questions/8360209/how-to-add-system-windows-interactivity-to-project
        // Expression.Blend.Sdk - worked
        // Microsoft.Xaml.Behaviors.Wpf
        private Timer _timer;        

        public VLCPlayer VLCPlayer { get; set; } = null;   // todo: play pause same button

        public ICommand StartPlaybackCommand { get; set; }
        public ICommand PausePlaybackCommand { get; set; }
        public ICommand StopPlaybackCommand { get; set; }        
        public ICommand ForwardPlaybackCommand { get; set; }
        public ICommand RewindPlaybackCommand { get; set; }

        public ICommand TrackControlMouseDownCommand { get; set; }
        public ICommand TrackControlMouseUpCommand { get; set; }
        public ICommand VolumeControlValueChangedCommand { get; set; }

        public PlayerViewModel()
        {
            LoadCommands();
            LoadVLCPlayer();
            SetTimer();
        }

        private void SetTimer()
        {
            _timer = new Timer();
            //_timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Interval = 10;  //300;
            _timer.Elapsed += Timer_Elapsed;
            //_timer.Start();
        }

        private void UpdateSeekBar()
        {
            if (VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Playing)
            {
                VLCPlayer.CurrentTrackPosition = GetCurrentTrackTime();
                VLCPlayer.CurrentTrackPositionString = MeidaTimeElapsed(VLCPlayer.CurrentTrackPosition);                
            }            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateSeekBar();
        }

        private void LoadCommands()
        {
            // Player commands
            StartPlaybackCommand = new RelayCommand(StartPlayback, CanStartPlayback);
            PausePlaybackCommand = new RelayCommand(PausePlayback, CanPausePlayback);
            StopPlaybackCommand = new RelayCommand(StopPlayback, CanStopPlayback);
            ForwardPlaybackCommand = new RelayCommand(ForwardPlayback, CanForwardPlayback);
            RewindPlaybackCommand = new RelayCommand(RewindPlayback, CanRewindPlayback);

            // Event commands
            TrackControlMouseDownCommand = new RelayCommand(TrackControlMouseDown, CanTrackControlMouseDown);
            TrackControlMouseUpCommand = new RelayCommand(TrackControlMouseUp, CanTrackControlMouseUp);
            VolumeControlValueChangedCommand = new RelayCommand(VolumeControlValueChanged, CanVolumeControlValueChanged);
        }

        private void LoadVLCPlayer()
        {
            VLCPlayer = new VLCPlayer();

            DirectoryInfo vlcLibDirectory = GetVLCLibDirectory();

            string[] options = GetVLCOptions();

            InitVLCPlayer(vlcLibDirectory, options);

            RegisterVLCEventHandler();
        }

        private DirectoryInfo GetVLCLibDirectory()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            var vlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            //var vlcLibDirectory = new DirectoryInfo(currentDirectory);
            return vlcLibDirectory;
        }

        private void InitVLCPlayer(DirectoryInfo vlcLibDirectory, string[] options)
        {
            VLCPlayer.VlcPlayerControl?.Dispose();
            VLCPlayer.VlcPlayerControl = new VlcControl();
            VLCPlayer.VlcPlayerControl.SourceProvider.CreatePlayer(vlcLibDirectory, options);

            // Initialize volume and labels
            VLCPlayer.CurrentVolume = 85;
            VLCPlayer.CurrentTrackLength = 100;            
            VLCPlayer.EnableState = false;

            ResetStringlLabel();            
        }

        private void ResetStringlLabel()
        {
            VLCPlayer.CurrentTrackLengthString = "00:00:00";
            VLCPlayer.CurrentTrackPositionString = "00:00:00";
        }

        private static string[] GetVLCOptions()
        {
            // VLC options can be given here. Please refer to the VLC command line documentation.
            return new string[]
                                {
                                    "--file-logging",
                                    "-vvv",
                                    "--extraintf=logger",
                                    "--logfile=Logs.log"
                                };
        }

        private void RegisterVLCEventHandler()
        {
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Playing += MediaPlayer_Playing;
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.EndReached += MediaPlayer_EndReached;
            //VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Stopped += MediaPlayer_Stopped;
        }

        //private void MediaPlayer_Stopped(object sender, VlcMediaPlayerStoppedEventArgs e)
        //{
        //    int a = 10;
        //    VLCPlayer.EnableState = false;
        //}

        private void MediaPlayer_Playing(object sender, VlcMediaPlayerPlayingEventArgs e)
        {         
            VLCPlayer.CurrentTrackLength = GetCurrentTrackLength();
            VLCPlayer.CurrentTrackLengthString = MeidaTimeElapsed(VLCPlayer.CurrentTrackLength);
            _timer.Start();
        }

        private long GetCurrentTrackLength()
        {
            return VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Length / 1000;            
        }

        private long GetCurrentTrackTime()
        {
            return VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time / 1000;
        }

        private string MeidaTimeElapsed(long timeLength)
        {
            var ts = TimeSpan.FromSeconds(timeLength);
            var timeElapsed = string.Format("{0:T}", ts);
            return timeElapsed;
        }

        private void MediaPlayer_EndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            _timer.Stop();
            //VLCPlayer.EnableState = false;
            //VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Position = 0;
            var thread = new System.Threading.Thread(delegate ()
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Position = 0;
                VLCPlayer.EnableState = false;
            });
            thread.Start();

            VLCPlayer.CurrentTrackLength = 100;
            VLCPlayer.CurrentTrackPosition = 0;

            ResetStringlLabel();
            //ExecuteStop();
        }

        private bool CanStartPlayback(object parameter)
        {
            if (VLCPlayer.EnableState == true)
            {
                return false;
            }
            return true;
        }

        private void StartPlayback(object parameter)
        {
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Play(new Uri(@"H:\dev tool.mp4"));

            //VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Audio.Volume = 0;
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Audio.Volume = VLCPlayer.CurrentVolume;

            VLCPlayer.EnableState = true;
        }

        private bool CanPausePlayback(object parameter)
        {
            if (VLCPlayer.EnableState == true)
            {
                return true;
            }
            return false;
        }

        private void PausePlayback(object parameter)
        {
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Pause();
        }

        private bool CanStopPlayback(object parameter)
        {
            if (VLCPlayer.EnableState == true)
            {
                return true;
            }
            return false;
        }

        private void StopPlayback(object parameter)
        {
            ExecuteStop();    
            //var thread = new System.Threading.Thread(delegate ()
            //{
            //    var temp = VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Audio.Volume;
            //    VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Stop();
            //    VLCPlayer.CurrentVolume = temp;
            //    _timer.Stop();
            //});
            //thread.Start();

            //ResetStringlLabel();

            //VLCPlayer.EnableState = false;            
        }

        private void ExecuteStop()
        {
            var thread = new System.Threading.Thread(delegate ()
            {
                //var temp = VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Audio.Volume;
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Stop();
                //VLCPlayer.CurrentVolume = temp;
                _timer.Stop();
            });
            thread.Start();

            ResetStringlLabel();

            VLCPlayer.EnableState = false;
        }

        private bool CanForwardPlayback(object parameter)
        {
            if (VLCPlayer.EnableState == true)
            {
                return true;
            }
            return false;
        }

        private void ForwardPlayback(object parameter)
        {
            VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time += 10000;
        }

        private bool CanRewindPlayback(object parameter)
        {
            if (VLCPlayer.EnableState == true)
            {
                return true;
            }
            return false;
        }

        private void RewindPlayback(object parameter)
        {
            if (VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time > 10000)
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time -= 10000;
            }
            else
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time = 0;
            }
        }

        // Events
        private void TrackControlMouseDown(object p)
        {
            if (VLCPlayer.VlcPlayerControl != null)
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Pause();
            }
        }

        private void TrackControlMouseUp(object p)
        {
            if (VLCPlayer.VlcPlayerControl != null)
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Time = VLCPlayer.CurrentTrackPosition * 1000;
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Pause();
            }
        }

        private bool CanTrackControlMouseDown(object parameter)
        {
            if (VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Playing)
            {
                return true;
            }
            return false;
        }

        private bool CanTrackControlMouseUp(object parameter)
        {
            if (VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.State == Vlc.DotNet.Core.Interops.Signatures.MediaStates.Paused)
            {
                return true;
            }
            return false;
        }

        private void VolumeControlValueChanged(object parameter)
        {
            if (VLCPlayer.VlcPlayerControl != null)
            {
                VLCPlayer.VlcPlayerControl.SourceProvider.MediaPlayer.Audio.Volume = Convert.ToInt32(VLCPlayer.CurrentVolume);
            }
        }

        private bool CanVolumeControlValueChanged(object parameter)
        {
            return true;
        }
    }
}
