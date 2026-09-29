using AgentServer.Holders;
using Serilog;

namespace AgentServer.XignCode
{
	public static class XignCodeServer
	{
		public static bool DoStartup()
		{
			if (!Conf.EnableXignCode && (ServerSettingHolder.ServerSettings == null || !ServerSettingHolder.ServerSettings.useXignCode))
			{
				Log.Information("XignCode disabled (EnableXignCode=false)");
				return false;
			}
			XignCodeRuntime.InspectNativePackage();
			if (!XignCodeRuntime.PackageComplete)
			{
				return false;
			}
			return XignCodeNative.TryActivate();
		}

		public static void DoShutdown()
		{
			XignCodeNative.Unload();
		}
	}
}
