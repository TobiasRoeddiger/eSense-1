using System;
using System.ComponentModel;
using Karl.Model;
using System.Windows.Input;
using Xamarin.Forms;
using Karl.View;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace Karl.ViewModel
{
	public class MainPageVM : INotifyPropertyChanged
	{
		private NavigationHandler _handler;
		private SettingsHandler _settingsHandler;
		private ConnectivityHandler _connectivityHandler;
		private LangManager _langManager;
		private ImageSource _iconOn;
		private ImageSource _iconOff;

		//Eventhandling
		public event PropertyChangedEventHandler PropertyChanged;

		//Properties binded to MainPage of View
		public CustomColor CurrentColor { get => _settingsHandler.CurrentColor; }
		public string DeviceName
		{
			get
			{
				if (_connectivityHandler.EarableConnected) {
					return _langManager.CurrentLang.Get("device_name") + " " +
						_settingsHandler.DeviceName; }
				return null;
			}
		}
		public string StepsAmount
		{
			get
			{
				if (_connectivityHandler.EarableConnected) {
					return _langManager.CurrentLang.Get("steps") + " " +
						Convert.ToString(_settingsHandler.Steps); }
				return null;
			}
		}
		public ImageSource Icon
		{
			get
			{
				return _connectivityHandler.EarableConnected ? _iconOn : _iconOff;
			}
		}

		//Commands binded to MainPage of View
		public ICommand AudioPlayerPageCommand { get; }
		public ICommand AudioLibPageCommand { get; }
		public ICommand TryConnectCommand { get; }
		public ICommand ModesPageCommand { get; }
		public ICommand SettingsPageCommand { get; }

		/// <summary>
		/// Initializises Commands, NavigationHandler and ConnectivityHandler, SettingsHandler of Model
		/// </summary>
		/// <param name="handler">For navigation</param>
		public MainPageVM(NavigationHandler handler)
		{
			_handler = handler;
			_connectivityHandler = ConnectivityHandler.SingletonConnectivityHandler;
			_settingsHandler = SettingsHandler.SingletonSettingsHandler;
			_langManager = LangManager.SingletonLangManager;
			AudioPlayerPageCommand = new Command(GotoAudioPlayerPage);
			AudioLibPageCommand = new Command(GotoAudioLibPage);
			TryConnectCommand = new Command(TryConnect);
			ModesPageCommand = new Command(GotoModesPage);
			SettingsPageCommand = new Command(GotoSettingsPage);
			_iconOn = ImageSource.FromResource("Karl.Resources.bluetooth_on.png");
			_iconOff = ImageSource.FromResource("Karl.Resources.bluetooth_off.png");
			_settingsHandler.SettingsChanged += Refresh;
			_connectivityHandler.ConnectionChanged += Refresh;
		}

		private void Refresh(object sender, SettingsEventArgs args)
		{
			switch (args.Value)
			{
				case nameof(_settingsHandler.CurrentLang):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StepsAmount)));
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
					break;
				case nameof(_settingsHandler.DeviceName):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
					break;
				case nameof(_settingsHandler.Steps):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StepsAmount)));
					break;
				case nameof(_settingsHandler.CurrentColor):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentColor)));
					break;
			}	
		}

		private void Refresh(object sender, ConnectionEventArgs args)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StepsAmount)));
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
		}

		private void GotoAudioPlayerPage()
		{
			_handler.GotoPage<AudioPlayerPage>();
		}

		private void GotoAudioLibPage()
		{
			_handler.GotoPage<AudioLibPage>();
		}

		private async void TryConnect()
		{
			if (_connectivityHandler.EarableConnected) { await _connectivityHandler.Disconnect(); }
			else
			{
				var success = await _connectivityHandler.Connect();
				if (!success)
				{
					INavToSettings navigator = DependencyService.Get<INavToSettings>();
					navigator.NavToSettings();
				}
			}
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
		}

		private void GotoModesPage()
		{
			_handler.GotoPage<ModesPage>();
		}

		private void GotoSettingsPage()
		{
			_handler.GotoPage<SettingsPage>();
		}

	}
}
