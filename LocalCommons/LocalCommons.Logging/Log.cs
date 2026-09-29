using System;
using System.Globalization;
using System.IO;

namespace LocalCommons.Logging
{
	public static class Log
	{
		private static string _logFile;

		private static string _logFileErr;

		public static LogLevel Hide { get; set; }

		public static string Archive { get; set; }

		public static string LogFile
		{
			get
			{
				return _logFile;
			}
			set
			{
				if (value != null)
				{
					string directoryName = Path.GetDirectoryName(value);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					if (File.Exists(value))
					{
						if (Archive != null)
						{
							if (!Directory.Exists(Archive))
							{
								Directory.CreateDirectory(Archive);
							}
							string path = File.GetLastWriteTime(value).ToString("yyyy-MM-dd_hh-mm");
							string text = Path.Combine(Archive, path);
							string text2 = Path.Combine(text, Path.GetFileName(value));
							if (!Directory.Exists(text))
							{
								Directory.CreateDirectory(text);
							}
							if (File.Exists(text2))
							{
								File.Delete(text2);
							}
							File.Move(value, text2);
						}
						File.Delete(value);
					}
				}
				_logFile = value;
			}
		}

		public static string LogFileErr
		{
			get
			{
				return _logFileErr;
			}
			set
			{
				if (value != null)
				{
					string directoryName = Path.GetDirectoryName(value);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					if (File.Exists(value))
					{
						if (Archive != null)
						{
							if (!Directory.Exists(Archive))
							{
								Directory.CreateDirectory(Archive);
							}
							string path = File.GetLastWriteTime(value).ToString("yyyy-MM-dd_hh-mm");
							string text = Path.Combine(Archive, path);
							string text2 = Path.Combine(text, Path.GetFileName(value));
							if (!Directory.Exists(text))
							{
								Directory.CreateDirectory(text);
							}
							if (File.Exists(text2))
							{
								File.Delete(text2);
							}
							File.Move(value, text2);
						}
						File.Delete(value);
					}
				}
				_logFileErr = value;
			}
		}

		public static void Info(string format, params object[] args)
		{
			WriteLine(LogLevel.Info, format, args);
		}

		public static void Warning(string format, params object[] args)
		{
			WriteLine(LogLevel.Warning, format, args);
		}

		public static void Error(string format, params object[] args)
		{
			WriteLine(LogLevel.Error, format, args);
		}

		public static void Debug(string format, params object[] args)
		{
			WriteLine(LogLevel.Debug, format, args);
		}

		public static void Debug(object obj)
		{
			WriteLine(LogLevel.Debug, obj.ToString());
		}

		public static void Status(string format, params object[] args)
		{
			WriteLine(LogLevel.Status, format, args);
		}

		public static void Exception(Exception ex, string description = null, params object[] args)
		{
			if (description != null)
			{
				if (Hide.HasFlag(LogLevel.Exception))
				{
					description += " See log file for more details.";
				}
				WriteLine(LogLevel.Error, description, args);
			}
			WriteLine(LogLevel.Exception, ex.ToString());
		}

		public static void Progress(int current, int max)
		{
			float num = 100f / (float)max * (float)current;
			int num2 = (int)Math.Ceiling(20f / (float)max * (float)current);
			Write(LogLevel.Info, false, "[" + "".PadRight(num2, '#') + "".PadLeft(20 - num2, '.') + "] {0,5}%\r", num.ToString("0.0", CultureInfo.InvariantCulture));
		}

		public static void WriteLine(LogLevel level, string format, params object[] args)
		{
			Write(level, format, args);
		}

		public static void WriteLine()
		{
			WriteLine(LogLevel.None, "");
		}

		public static void Write(LogLevel level, string format, params object[] args)
		{
			Write(level, toFile: true, format, args);
		}

		private static void Write(LogLevel level, bool toFile, string format, params object[] args)
		{
			lock (Console.Out)
			{
				if (!Hide.HasFlag(level))
				{
					Console.Write($"[{level}] - {format}\n", args);
				}
				if (level != LogLevel.Error)
				{
					if (!(_logFile != null && toFile))
					{
						return;
					}
					using StreamWriter streamWriter = new StreamWriter(_logFile, append: true);
					streamWriter.Write(DateTime.Now.ToString() + " ");
					if (level != LogLevel.None)
					{
						streamWriter.Write("[{0}] - ", level);
					}
					streamWriter.WriteLine(format, args);
					streamWriter.Flush();
					return;
				}
				if (!(_logFileErr != null && toFile))
				{
					return;
				}
				using StreamWriter streamWriter2 = new StreamWriter(_logFileErr, append: true);
				streamWriter2.Write(DateTime.Now.ToString() + " ");
				if (level != LogLevel.None)
				{
					streamWriter2.Write("[{0}] - ", level);
				}
				streamWriter2.WriteLine(format, args);
				streamWriter2.Flush();
			}
		}
	}
}
