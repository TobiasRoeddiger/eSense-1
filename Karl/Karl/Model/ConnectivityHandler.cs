using System;
using EarableLibrary;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Karl.Model
{
	/// <summary>
	/// This class handles the conntection to the Earables Library and Step Detection Library
	/// </summary>
	public class ConnectivityHandler
	{

		private static ConnectivityHandler _connectivityHandler;
		public static ConnectivityHandler SingletonConnectivityHandler
		{
			get
			{
				if (_connectivityHandler == null)
				{
					_connectivityHandler = new ConnectivityHandler();
				}
				return _connectivityHandler;
			}
		}

		private readonly IEarableScanner _earableScanner;
		private IEarable _connectedEarable;
		public bool Connected { get => EarableConnected(); }
		public ObservableCollection<EarableHandle> DiscoveredDevices { get; }

		private ConnectivityHandler()
		{
			DiscoveredDevices = new ObservableCollection<EarableHandle>();
			_earableScanner = new EarableLibrary.EarableLibrary();
			_earableScanner.EarableDiscovered += (s, e) => {
				DiscoveredDevices.Add(new EarableHandle(e.Earable));
			};
		}

		/// <summary>
		/// Search for Bluetooth Devices.
		/// </summary>
		public void SearchDevices()
		{
			_earableScanner.StopScanning();
			DiscoveredDevices.Clear();
			_earableScanner.StartScanning();
		}

		/// <summary>
		/// Connect to the Device given as a parameter.
		/// </summary>
		/// <param name="device">Device to connect with.</param>
		public async void ConnectDevice(EarableHandle device)
		{
			_earableScanner.StopScanning();
			await device.handle.ConnectAsync();

			var imu = (MotionSensor)device.handle.Sensors[typeof(MotionSensor)];
			imu.SamplingRate = 20;
			imu.ValueChanged += (s, e) =>
			{
				var c = (MotionSensorChangedEventArgs)e;
				Debug.WriteLine("Acc: {0} {1} {2}", c.Acc.x, c.Acc.y, c.Acc.z);
			};
			imu.StartSampling();

			var button = (PushButton)device.handle.Sensors[typeof(PushButton)];
			button.ValueChanged += (s, e) =>
			{
				var args = (PushButtonChangedEventArgs)e;
				Debug.WriteLine("Pushed the button: {0}", args.Pushed);
			};
			button.StartSampling();

			/*var voltage = (VoltageSensor)device.handle.Sensors[typeof(VoltageSensor)];
			voltage.ValueChanged += (s, e) =>
			{
				var args = (VoltageChangedEventArgs)e;
				Debug.WriteLine("Voltage: {0}", args.Voltage);
			};
			voltage.StartSampling();*/
			_connectedEarable = device.handle;
		}

		public void Disconnect()
		{
			if (!EarableConnected()) return;
			_connectedEarable.DisconnectAsync();
			_connectedEarable = null;
		}

		/// <summary>
		/// Set a new Device name.
		/// </summary>
		/// <param name="name">The new Name.</param>
		public void SetDeviceName(String name)
		{
			if (!EarableConnected()) return;
			_connectedEarable.Name = name;
		}

		private bool EarableConnected()
		{
			if (_connectedEarable == null) return false;
			if (!_connectedEarable.IsConnected())
			{
				_connectedEarable = null;
				return false;
			}
			return true;
		}
	}
}
