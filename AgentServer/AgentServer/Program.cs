using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AgentServer.Structuring;
using Serilog;

namespace AgentServer
{
	internal static class Program
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		[STAThread]
		private static void Main()
		{
			// Exe lives in bin\; DLLs are in bin\bin (probing + native bin\...).
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
			bool createdNew;
			using Mutex single = new Mutex(initiallyOwned: true, @"Local\TRServer_AgentServer", out createdNew);
			if (!createdNew)
			{
				MessageBox.Show("AgentServer is already running.", "TRServer", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool num = File.Exists(".\\bin\\TRServer.dll");
			flag = File.Exists(".\\hash.ini");
			flag2 = File.Exists(".\\settings.ini");
			if (num && flag && flag2)
			{
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				Application.ThreadException += Application_ThreadException;
				AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
				DeleteEmptyDirsAndFiles(".\\Logs\\Agent\\");
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(defaultValue: false);
				ServerStatus.form1 = new Form1();
				Application.Run(ServerStatus.form1);
			}
			else
			{
				MessageBox.Show(
					"Missing startup files in:\n" + Directory.GetCurrentDirectory()
					+ "\n\nNeed: hash.ini, settings.ini, bin\\TRServer.dll",
					"TRServer",
					MessageBoxButtons.OK,
					MessageBoxIcon.Hand);
			}
			GC.KeepAlive(single);
		}

		private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
		{
			Exception exception = e.Exception;
			try
			{
				if (exception != null)
				{
					Log.Error("Unhandled exception: {0}\r\n{1}\r\n{2}", exception.GetType().Name, exception.Message, exception.StackTrace);
				}
				else
				{
					Log.Error("Unhandled thread exception: {0}", e);
				}
			}
			catch
			{
				MessageBox.Show(exception != null ? exception.ToString() : e.ToString(), "TRServer", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
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
			catch
			{
				MessageBox.Show(e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown fatal error", "TRServer", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
