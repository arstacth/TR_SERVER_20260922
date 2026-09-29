using System;
using System.Runtime.InteropServices;
using Serilog;

namespace AgentServer.XignCode
{
	internal static class XignCodeNative
	{
		private static IntPtr _helper;

		public static bool Loaded => _helper != IntPtr.Zero;

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		[DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		public static bool TryActivate()
		{
			if (!XignCodeRuntime.PackageComplete)
			{
				return false;
			}
			if (_helper != IntPtr.Zero)
			{
				return true;
			}
			string path = System.IO.Path.Combine(XignCodeRuntime.GetPackageDirectory(), XignCodeRuntime.HelperFileName);
			_helper = LoadLibrary(path);
			if (_helper == IntPtr.Zero)
			{
				Log.Error("XignCode LoadLibrary failed for {0} win32={1}", path, Marshal.GetLastWin32Error());
				return false;
			}
			Log.Information("XignCode SDK helper loaded: {0}", path);
			LogKnownExports();
			return true;
		}

		public static void Unload()
		{
			if (_helper != IntPtr.Zero)
			{
				FreeLibrary(_helper);
				_helper = IntPtr.Zero;
			}
		}

		private static void LogKnownExports()
		{
			string[] names =
			{
				"ZCWAVE_SysInit",
				"ZCWAVE_SysCleanup",
				"ZCWAVE_HelperInitialize",
				"ZWAveCreate",
				"Create"
			};
			foreach (string name in names)
			{
				IntPtr proc = GetProcAddress(_helper, name);
				if (proc != IntPtr.Zero)
				{
					Log.Information("XignCode export present: {0}", name);
				}
			}
		}
	}
}
