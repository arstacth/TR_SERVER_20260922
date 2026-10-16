using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace RoomServer
{
	internal static class Program
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		[STAThread]
		private static void Main()
		{
			try
			{
				string baseDir = AppDomain.CurrentDomain.BaseDirectory;
				if (!string.IsNullOrEmpty(baseDir))
				{
					Directory.SetCurrentDirectory(baseDir);
					string deps = Path.Combine(baseDir, "bin");
					if (Directory.Exists(deps))
					{
						SetDllDirectory(deps);
					}
				}
			}
			catch
			{
			}
			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
			Application.ThreadException += Application_ThreadException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			DeleteEmptyDirsAndFiles(".\\Logs\\Room\\");
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			Application.Run(new Form1());
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Exception exception = e.Exception;
			if (exception != null)
			{
				Log.Error("Unhandled exception: {0}\r\n{1}\r\n{2}", exception.GetType().Name, exception.Message, exception.StackTrace);
			}
			else
			{
				Log.Error("Unhandled thread exception: {0}", e);
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				Log.Error("Unhandled exception: {0}\r\n{1}", ex.Message, ex.StackTrace);
			}
			else
			{
				Log.Error("Unhandled exception: {0}", e);
			}
		}

		private static void DeleteEmptyDirsAndFiles(string dir)
		{
			if (string.IsNullOrEmpty(dir))
			{
				throw new ArgumentException("Starting directory is a null reference or an empty string", "dir");
			}
			if (!Directory.Exists(dir))
			{
				return;
			}
			try
			{
				foreach (string item in Directory.EnumerateDirectories(dir))
				{
					DeleteEmptyDirsAndFiles(item);
				}
				if (!Directory.EnumerateFileSystemEntries(dir).Any())
				{
					try
					{
						Directory.Delete(dir);
					}
					catch (UnauthorizedAccessException)
					{
					}
					catch (DirectoryNotFoundException)
					{
					}
					return;
				}
				if (!Directory.Exists(dir))
				{
					return;
				}
				string[] files = Directory.GetFiles(dir);
				foreach (string text in files)
				{
					if (new FileInfo(text).Length == 0L)
					{
						try
						{
							File.Delete(text);
						}
						catch (Exception)
						{
						}
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}
		}
	}
}
