using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MediaPlayerApp1.Controls
{
    public sealed partial class CustomMediaPlayer : UserControl
    {
        #region Constants

        public static readonly object EMPTY_PARAMETER = new object();

        #endregion
        
        #region Dependency Properties

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set
            {
                SetValue(SourceProperty, value);
            }
        }

        public ICommand PlayCommand
        {
            get { return (ICommand)GetValue(PlayCommandProperty); }
            set { SetValue(PlayCommandProperty, value); }
        }

        public ICommand StopCommand
        {
            get { return (ICommand)GetValue(StopCommandProperty); }
            set { SetValue(StopCommandProperty, value); }
        }

        public ICommand PauseCommand
        {
            get { return (ICommand)GetValue(PauseCommandProperty); }
            set { SetValue(PauseCommandProperty, value); }
        }

        public ICommand MediaChangePositionCommand
        {
            get { return (ICommand)GetValue(MediaChangePositionCommandProperty); }
            set { SetValue(MediaChangePositionCommandProperty, value); }
        }
        
        public bool UseControls
        {
            get { return (bool)GetValue(UseControlsProperty); }
            set { SetValue(UseControlsProperty, value); }
        }

        public TimeSpan VideoCurrentTime
        {
            get { return (TimeSpan)GetValue(VideoCurrentTimeProperty); }
            set { SetValue(VideoCurrentTimeProperty, value); }
        }

        public TimeSpan VideoMaxTime
        {
            get { return (TimeSpan)GetValue(VideoMaxTimeProperty); }
            set { SetValue(VideoMaxTimeProperty, value); }
        }

        public double SoundVolume
        {
            get { return (double)GetValue(SoundVolumeProperty); }
            set { SetValue(SoundVolumeProperty, value); }
        }

        public bool VolumeMute
        {
            get { return (bool)GetValue(VolumeMuteProperty); }
            set { SetValue(VolumeMuteProperty, value); }
        }

        public static readonly DependencyProperty VolumeMuteProperty =
           DependencyProperty.Register("VolumeMute", typeof(bool), typeof(CustomMediaPlayer), new PropertyMetadata(false, OnSoundMuteValueChanged));

        public static readonly DependencyProperty SoundVolumeProperty =
           DependencyProperty.Register("SoundVolume", typeof(double), typeof(CustomMediaPlayer), new PropertyMetadata((double)50, OnSoundVolumeChanged));
        
        public static readonly DependencyProperty VideoMaxTimeProperty =
           DependencyProperty.Register("VideoMaxTime", typeof(TimeSpan), typeof(CustomMediaPlayer), null);

        public static readonly DependencyProperty VideoCurrentTimeProperty =
            DependencyProperty.Register("VideoCurrentTime", typeof(TimeSpan), typeof(CustomMediaPlayer), null);
        
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(Uri), typeof(CustomMediaPlayer), new PropertyMetadata(null, OnSourceValueChanged));

        public static readonly DependencyProperty MediaChangePositionCommandProperty =
            DependencyProperty.Register("MediaChangePositionCommand", typeof(ICommand), typeof(CustomMediaPlayer), null);

        public static readonly DependencyProperty PlayCommandProperty =
            DependencyProperty.Register("PlayCommand", typeof(ICommand), typeof(CustomMediaPlayer), null);

        public static readonly DependencyProperty StopCommandProperty =
            DependencyProperty.Register("StopCommand", typeof(ICommand), typeof(CustomMediaPlayer), null);

        public static readonly DependencyProperty PauseCommandProperty =
            DependencyProperty.Register("PauseCommand", typeof(ICommand), typeof(CustomMediaPlayer), null);
        
        public static readonly DependencyProperty UseControlsProperty =
            DependencyProperty.Register("UseControls", typeof(bool), typeof(CustomMediaPlayer), new PropertyMetadata(false));

        #endregion

        #region Dependency Property Callbacks

        private static void OnSourceValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OnSourceValueChanged(d);
        }

        private static void OnSourceValueChanged(DependencyObject d)
        {
            var mediaPlayerControl = d as CustomMediaPlayer;
            mediaPlayerControl.InitializeMediaPlayer();
        }

        private static void OnSoundVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mediaPlayerControl = d as CustomMediaPlayer;
            var ratio = ((double)e.NewValue - mediaPlayerControl.VolumeSlider.Minimum) / mediaPlayerControl.VolumeSlider.Maximum;

            mediaPlayerControl.mediaPlayer.Volume = ratio;
        }
        
        private static void OnSoundMuteValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mediaPlayerControl = d as CustomMediaPlayer;
            mediaPlayerControl.AudioMuteSymbol.Symbol = (bool)e.NewValue ? Symbol.Mute : Symbol.Volume;

            if (mediaPlayerControl.isMediaPlayerInitialized)
            {
                mediaPlayerControl.mediaPlayer.IsMuted = (bool)e.NewValue;
            }
        }
        
        #endregion

        #region Class Properties

        private MediaPlayer mediaPlayer;
        private bool mediaPlaying;

        public static async Task CallOnUiThreadAsync(CoreDispatcher dispatcher, DispatchedHandler handler) =>
    await dispatcher.RunAsync(CoreDispatcherPriority.Normal, handler);
        
        public bool MediaPlaying
        {
            get
            {
                return mediaPlaying;
            }
            set
            {
                if (mediaPlaying != value)
                {
                    mediaPlaying = value;
                    OnMediaPlayingChanged(value);
                }
            }
        }

        private void OnMediaPlayingChanged(bool newValue)
        {
            PlayPauseSymbol.Symbol = newValue ? Symbol.Pause : Symbol.Play;

            if (newValue)
            {
                Task.Run(async () =>
                {
                    while (MediaPlaying)
                    {
                        await CallOnUiThreadAsync(this.Dispatcher, () =>
                        {
                            UpdateTimeline();
                        });
                        await Task.Delay(10);
                        
                    }
                });
            }
        }

        private bool isMediaPlayerInitialized;

        #endregion

        #region Private Methods

        private void UpdateTimeline()
        {
            VideoCurrentTime = mediaPlayer.PlaybackSession.Position;
        }

        private void PositionChange(TimeSpan pos)
        {
            mediaPlayer.PlaybackSession.Position = pos;
        }

        private void Play()
        {
            if (Source.Equals(null))
                return;

            if (!isMediaPlayerInitialized)
                InitializeMediaPlayer();

            if (!MediaPlaying)
            {
                //if (PlayCommand.CanExecute(EMPTY_PARAMETER))
                //{
                //    PlayCommand.Execute(EMPTY_PARAMETER);
                //}
                mediaPlayer.Play();
                MediaPlaying = true;
                VideoMaxTime = mediaPlayer.PlaybackSession.NaturalDuration;
            }


            


        }

        private void Pause()
        {
            if (Source.Equals(null))
                return;

            if (!isMediaPlayerInitialized)
                InitializeMediaPlayer();
            if (MediaPlaying)
            {

                //if (PauseCommand.CanExecute(EMPTY_PARAMETER))
                //{
                //    PauseCommand.Execute(EMPTY_PARAMETER);
                //}
                mediaPlayer.Pause();
                MediaPlaying = false;
            }

            
        }

        private void Stop()
        {
            if (Source.Equals(null))
                return;
            if (!isMediaPlayerInitialized)
                InitializeMediaPlayer();

            //if (StopCommand.CanExecute(EMPTY_PARAMETER))
            //{
            //    StopCommand.Execute(EMPTY_PARAMETER);
            //}
            mediaPlayer.Pause();
            mediaPlayer.PlaybackSession.Position = new TimeSpan(0);
            MediaPlaying = false;
            UpdateTimeline();
        }

        private void MediaPlayerPositionChange(TimeSpan newPos)
        {
            VideoCurrentTime = newPos;
        }
        
        private void InitializeMediaPlayer()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.Pause();
            }
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Source = MediaSource.CreateFromUri(Source);
            VideoMaxTime = mediaPlayer.PlaybackSession.NaturalDuration;
            VideoCurrentTime = mediaPlayer.PlaybackSession.Position;
            
            MediaPlaying = false;
            mediaPlayerElement.SetMediaPlayer(mediaPlayer);
            mediaPlayer.Volume = (SoundVolume - VolumeSlider.Minimum) / VolumeSlider.Maximum;
            mediaPlayer.IsMuted = VolumeMute;
            isMediaPlayerInitialized = true;
        }
        
        #endregion

        #region Event Handlers

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!MediaPlaying)
            {
                Play();
                if (PlayCommand != null && PlayCommand.CanExecute(EMPTY_PARAMETER))
                {
                    PlayCommand.Execute(EMPTY_PARAMETER);
                }
            }
            else
            {
                Pause();
                if (PauseCommand != null && PauseCommand.CanExecute(EMPTY_PARAMETER))
                {
                    PauseCommand.Execute(EMPTY_PARAMETER);
                }
            }
        }
        
        private void AudioMuteButton_Click(object sender, RoutedEventArgs e)
        {
            VolumeMute = !VolumeMute;
        }

        private void FullWindowButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            Stop();
            if (StopCommand != null && StopCommand.CanExecute(EMPTY_PARAMETER))
            {
                StopCommand.Execute(EMPTY_PARAMETER);
            }
        }
        
        private void CloseControlsButton_Click(object sender, RoutedEventArgs e)
        {
            ControlPanelGrid.Visibility = Visibility.Collapsed;
        }

        private void MediaTransportControls_Command_Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ControlPanelGrid.Visibility = Visibility.Visible;
        }

        private void ProgressSlider_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var slider = (Slider)sender;

            var max = slider.ActualWidth;
            var pos = e.GetPosition(slider).X;

            var posMilliseconds = VideoMaxTime.TotalMilliseconds * (pos / max);
            var time = TimeSpan.FromMilliseconds(posMilliseconds);
            PositionChange(time);
            
            if (MediaChangePositionCommand != null && MediaChangePositionCommand.CanExecute(time))
            {
                MediaChangePositionCommand.Execute(time);
            }
        }

        #endregion

        #region Constructor

        public CustomMediaPlayer()
        {
            this.InitializeComponent();
        }

        #endregion
        
    }
}
