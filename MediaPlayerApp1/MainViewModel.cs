using ChatterBox.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaPlayerApp1
{
    public class MainViewModel : BindableBase
    {
        Uri one = new Uri("http://clips.vorwaerts-gmbh.de/VfE_html5.mp4");
        Uri two = new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4");

        public MainViewModel()
        {
            Src = one;
            ChangeUrlCmd = new DelegateCommand(ChangeUrl);
            PlayCmd = new DelegateCommand(Play);
            StopCmd = new DelegateCommand(Stop);
            PauseCmd = new DelegateCommand(Pause);
            CommandLog = "Command Log Display :";
        }

        private void Pause()
        {
            CommandLog = CommandLog + Environment.NewLine + "Pause Pressed...";
        }

        private void Stop()
        {
            CommandLog = CommandLog + Environment.NewLine + "Stop Pressed...";
        }

        private void Play()
        {
            CommandLog = CommandLog + Environment.NewLine + "Play Pressed...";
        }
        
        private void ChangeUrl()
        {
            Src = Src.Equals(one) ? two : one;
        }

        private ICommand _changeUrlCmd;

        public ICommand ChangeUrlCmd
        {
            get { return _changeUrlCmd; }
            set { SetProperty(ref _changeUrlCmd, value); }
        }

        private ICommand _playCmd;

        public ICommand PlayCmd
        {
            get { return _playCmd; }
            set { SetProperty(ref _playCmd, value); }
        }

        private ICommand _stopCmd;

        public ICommand StopCmd
        {
            get { return _stopCmd; }
            set { SetProperty(ref _stopCmd, value); }
        }

        private ICommand _pauseCmd;

        public ICommand PauseCmd
        {
            get { return _pauseCmd; }
            set { SetProperty(ref _pauseCmd, value); }
        }
        
        private string _commandLog;

        public string CommandLog
        {
            get { return _commandLog; }
            set { SetProperty(ref _commandLog, value); }
        }

        private Uri _src;
        
        public Uri Src
        {
            get { return _src; }
            set { SetProperty(ref _src, value); }
        }
        

    }
}
