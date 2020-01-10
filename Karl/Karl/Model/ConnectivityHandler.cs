using System;
using EarableLibrary;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Karl.Data;

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
		private ActivityLog _log;
		public bool Connected { get => EarableConnected(); }
		public ObservableCollection<EarableHandle> DiscoveredDevices { get; }

		private ConnectivityHandler()
		{
			DiscoveredDevices = new ObservableCollection<EarableHandle>();
			_earableScanner = new EarableLibrary.EarableLibrary(scanTimeout: 60);
			_earableScanner.EarableDiscovered += (s, e) => {
				DiscoveredDevices.Add(new EarableHandle(e.Earable));
			};
			_log = new ActivityLog();
		}

		/// <summary>
		/// Search for Bluetooth Devices.
		/// </summary>
		public async void SearchDevices()
		{
			await _earableScanner.StopScanningAsync();
			DiscoveredDevices.Clear();
			await _earableScanner.StartScanningAsync();
		}

		/// <summary>
		/// Connect to the Device given as a parameter.
		/// </summary>
		/// <param name="device">Device to connect with.</param>
		public async void ConnectDevice(EarableHandle device)
		{
			await _earableScanner.StopScanningAsync();
			await device.handle.ConnectAsync();

			_log.StopLogging();
			_log.StartLogging();

			var imu = (MotionSensor)device.handle.Sensors[typeof(MotionSensor)];
			imu.SamplingRate = 50;
			imu.ValueChanged += (s, args) =>
			{
				// Debug.WriteLine("(Id,Acc(x,y,z))\t{0}\t{1}\t{2}\t{3}", args.PacketId, args.Acc.x, args.Acc.y, args.Acc.z);
				_log.Log(args);
			};
			await imu.StartSamplingAsync();

			var button = (PushButton)device.handle.Sensors[typeof(PushButton)];
			button.ValueChanged += (s, args) =>
			{
				Debug.WriteLine("Button pushed: {0}", args.Pressed);
			};
			await button.StartSamplingAsync();

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
			_log.StopLogging();
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
