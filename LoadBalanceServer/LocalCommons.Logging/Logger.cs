using System;
using System.IO;

namespace LocalCommons.Logging
{
	public class Logger
	{
		private static StreamWriter writer;

		public static void Init()
		{
			if (!Directory.Exists("log"))
			{
				Directory.CreateDirectory("log");
			}
			writer = new StreamWriter("log/logging.log");
			writer.AutoFlush = true;
		}

		public static void Trace(string data, params object[] prms)
		{
			Console.WriteLine(DateTime.Now.ToString("g") + " [INFO] - " + data, prms);
			writer.WriteLine(DateTime.Now.ToString("g") + " [INFO]  - " + data, prms);
		}

		public static void Trace(string data)
		{
			Console.WriteLine(DateTime.Now.ToString("g") + " [INFO] - " + data);
			writer.WriteLine(DateTime.Now.ToString("g") + " [INFO] - " + data);
		}

		public static void Section(string data)
		{
			data = "[ " + data + " ]";
			while (data.Length < 79)
			{
				data = "-" + data;
			}
			Console.WriteLine(data);
			writer.WriteLine(data);
		}
	}
}
