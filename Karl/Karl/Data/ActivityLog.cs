using EarableLibrary;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Karl.Data
{
	public class ActivityLog
	{
		private readonly ActivityFrame[] buffer;
		private SQLiteAsyncConnection database;
		private int counter;

		private byte lastId;

		public ActivityLog()
		{
			buffer = new ActivityFrame[256];
			counter = 0;
			lastId = 0;
		}

		public void StartLogging()
		{
			if (database == null)
			{
				var name = string.Format("activity-log-{0}.db", DateTime.Now.Ticks);
				var path = Path.Combine("/storage/emulated/0/Karl/", name);
				Debug.WriteLine("Creating db at {0}", path);
				database = new SQLiteAsyncConnection(path);
				database.CreateTableAsync<ActivityFrame>().Wait();
			}
		}

		public void Log(MotionArgs args)
		{
			buffer[counter++] = new ActivityFrame(args);
			counter %= buffer.Length;
			if (counter == 0)
			{
				Debug.WriteLine("Syncing DB...");
				database.InsertAllAsync(buffer);
			}
			if (args.PacketId > lastId + 1)
			{
				Debug.WriteLine("Lost packet(s): last={0}, current={1}", lastId, args.PacketId);
			}
			lastId = args.PacketId;
		}

		public void StopLogging()
		{
			if (database != null)
			{
				if (counter != 0)
				{
					ActivityFrame[] notInsertedYet = new ActivityFrame[counter];
					Array.Copy(buffer, 0, notInsertedYet, 0, counter);
					database.InsertAllAsync(notInsertedYet).Wait();
					counter = 0;
				}
				database.CloseAsync();
				database = null;
			}
		}
	}

	[Serializable]
	internal class ActivityFrame
	{
		public ActivityFrame(MotionArgs args)
		{
			AccX = args.Acc.x;
			AccY = args.Acc.y;
			AccZ = args.Acc.z;
			GyroX = args.Gyro.x;
			GyroY = args.Gyro.y;
			GyroZ = args.Gyro.z;
			Counter = args.PacketId;
		}
		public ActivityFrame() {}
		[PrimaryKey, AutoIncrement]
		public long Id { get; set; }
		public short AccX { get; set; }
		public short AccY{ get; set; }
		public short AccZ{ get; set; }
		public short GyroX { get; set; }
		public short GyroY { get; set; }
		public short GyroZ { get; set; }
		public byte Counter { get; set; }
	}
}
