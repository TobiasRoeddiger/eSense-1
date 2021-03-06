using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Karl.Model;
using System.Timers;

namespace Karl.ViewModel
{
	public class AudioPlayerPageVM : INotifyPropertyChanged
	{
		private AudioPlayer _audioPlayer;
		private string _iconPlay;
		private string _iconPause;
		private string _icon;
		private Timer _timer;
		private double _dragValue;
		private bool _wasPaused;

		/**
		 Properties binded to AudioPlayerPage of View
		**/

		public AudioTrack AudioTrack
		{
			get { return _audioPlayer.CurrentTrack; }
		}

		public double Volume
		{
			get { return _audioPlayer.Volume; }
			set
			{
				_audioPlayer.Volume = value;
				OnPropertyChanged("Volume");
			}
		}

		public double CurrentPosition
		{
			get
			{
				if (AudioTrack == null) { return 0; }
				return _audioPlayer.CurrentSecInTrack / AudioTrack.Duration;
			}
			set { _dragValue = value; }
		}
		
		public string Icon
		{
			get { return _icon; }
			set
			{
				_icon = value;
				OnPropertyChanged("Icon");
			}
		}

		public string TimePlayed
		{
			get
			{
				if (AudioTrack == null) { return "-:--"; }
				return string.Format("{0}:{1:00}",
				(int) TimeSpan.FromSeconds(_audioPlayer.CurrentSecInTrack).TotalMinutes,
				TimeSpan.FromSeconds(_audioPlayer.CurrentSecInTrack).Seconds);
			}
		}

		public string TimeLeft
		{
			get
			{
				if (AudioTrack == null) { return "-:--"; }
				return string.Format("{0}:{1:00}",
				(int) TimeSpan.FromSeconds(AudioTrack.Duration - _audioPlayer.CurrentSecInTrack).TotalMinutes,
				TimeSpan.FromSeconds(AudioTrack.Duration - _audioPlayer.CurrentSecInTrack).Seconds);
			}
		}

		public string Cover
		{
			get
			{
				//Image cover = new Image();
				if (AudioTrack == null)
				{
					//cover.Source = "art.png";
					//return cover;
					return "art.png";
				}
				//cover.Source = ImageSource.FromStream(() => new System.IO.MemoryStream(AudioTrack.Cover));
				//return cover;
				return "art.png";
			}
		}

		/**
		 Commands binded to AudioPlayerPage of View
		**/

		public ICommand PausePlayCommand { get; }
		public ICommand PlayPrevCommand { get; }
		public ICommand PlayNextCommand { get; }
		public ICommand PositionDragStartedCommand { get; }
		public ICommand PositionDragCompletedCommand { get; }
		
		/// <summary>
		/// Initializises Commands, Images and AudioPlayer of Model
		/// </summary>
		public AudioPlayerPageVM()
		{
			_audioPlayer = AudioPlayer.SingletonAudioPlayer;
			PausePlayCommand = new Command(PausePlay);
			PlayPrevCommand = new Command(PlayPrev);
			PlayNextCommand = new Command(PlayNext);
			PositionDragStartedCommand = new Command(PositionDragStarted);
			PositionDragCompletedCommand = new Command(PositionDragCompleted);
			_iconPlay = "play.png";
			_iconPause = "pause.png";
			Icon = _iconPause;
			_timer = new Timer();
			_timer.Interval = 100;
			_timer.Elapsed += new ElapsedEventHandler(Tick);
			_timer.AutoReset = true;
			_dragValue = 0;
		}

		/// <summary>
		/// Refreshes all Properties
		/// </summary>
		public void RefreshPage()
		{
			if (_audioPlayer.Paused)
			{
				Icon = _iconPlay;
			}
			else
			{
				_timer.Start();
				Icon = _iconPause;
			}
			OnPropertyChanged("CurrentPosition");
			OnPropertyChanged("Duration");
			OnPropertyChanged("Volume");
			OnPropertyChanged("TimePlayed");
			OnPropertyChanged("TimeLeft");
			OnPropertyChanged("AudioTrack");
			OnPropertyChanged("Cover");
		}

		/// <summary>
		/// Pauses/Plays song in AudioPlayer of Model and updates Icon
		/// </summary>
		private void PausePlay()
		{
			if (AudioTrack == null) { return; }
			_audioPlayer.TogglePause();
			if (_audioPlayer.Paused)
			{
				_timer.Stop();
				Icon = _iconPlay;
				
			}
			else
			{
				_timer.Start();
				Icon = _iconPause;
			}
		}

		/// <summary>
		/// Plays previous song in AudioPlayer of Model
		/// </summary>
		private void PlayPrev()
		{
			if (AudioTrack == null) { return; }
			_audioPlayer.PrevTrack();
			RefreshPage();
		}

		/// <summary>
		/// Plays next song in AudioPlayer of Model
		/// </summary>
		private void PlayNext()
		{
			if (AudioTrack == null) { return; }
			_audioPlayer.NextTrack();
			RefreshPage();
		}

		/// <summary>
		/// Pauses the AudioPlayer of Model
		/// </summary>
		private void PositionDragStarted()
		{
			if (AudioTrack == null) { return; }
			if (!_audioPlayer.Paused)
			{
				PausePlay();
				_wasPaused = false;
			}
			else { _wasPaused = true; }
		}

		/// <summary>
		/// Updates CurrentSecInTrack of AudioPlayer of Model and continues playback
		/// </summary>
		private void PositionDragCompleted()
		{
			if (AudioTrack == null) { return; }
			if (!_wasPaused) { PausePlay(); }
			_audioPlayer.CurrentSecInTrack = _dragValue * AudioTrack.Duration;
			OnPropertyChanged("CurrentPosition");
		}

		/// <summary>
		/// Refreshes Properties that change while song is playing
		/// </summary>
		private void Tick(object sender, EventArgs e)
		{
			OnPropertyChanged("AudioTrack");
			OnPropertyChanged("CurrentPosition");
			OnPropertyChanged("TimePlayed");
			OnPropertyChanged("TimeLeft");
		}

		//Eventhandling

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
