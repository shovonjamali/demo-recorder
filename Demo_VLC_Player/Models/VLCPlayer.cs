using System.ComponentModel;
using Vlc.DotNet.Wpf;

namespace Demo_VLC_Player.Models
{
    public class VLCPlayer : INotifyPropertyChanged
    {
        private VlcControl _vlcPlayerControl = null;

        private long _currentTrackLength = 0;
        private string _currentTrackLengthString = null;

        private long _currentTrackPosition = 0;
        private string _currentTrackPositionString = null;

        private int _currentVolume = 0;
        private bool _enableState = false;

        public VlcControl VlcPlayerControl
        {
            get
            {
                return _vlcPlayerControl;
            }
            set
            {
                _vlcPlayerControl = value;
                OnPropertyChanged("VlcPlayerControl");
            }
        }

        public long CurrentTrackLength
        {
            get
            {
                return _currentTrackLength;
            }
            set
            {
                _currentTrackLength = value;
                OnPropertyChanged("CurrentTrackLength");
            }
        }

        public string CurrentTrackLengthString
        {
            get
            {
                return _currentTrackLengthString;
            }
            set
            {
                _currentTrackLengthString = value;
                OnPropertyChanged("CurrentTrackLengthString");
            }
        }

        public long CurrentTrackPosition
        {
            get
            {
                return _currentTrackPosition;
            }
            set
            {
                _currentTrackPosition = value;
                OnPropertyChanged("CurrentTrackPosition");
            }
        }

        public string CurrentTrackPositionString
        {
            get
            {
                return _currentTrackPositionString;
            }
            set
            {
                _currentTrackPositionString = value;
                OnPropertyChanged("CurrentTrackPositionString");
            }
        }

        public int CurrentVolume
        {
            get
            {
                return _currentVolume;
            }
            set
            {
                _currentVolume = value;
                OnPropertyChanged("CurrentVolume");
            }
        }

        public bool EnableState
        {
            get
            {
                return _enableState;
            }
            set
            {
                _enableState = value;
                OnPropertyChanged("EnableState");
            }
        }

        #region INotifyPropertyChanged Members  

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
