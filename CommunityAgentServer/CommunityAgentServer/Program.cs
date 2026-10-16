using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Akka.Actor;
using CommunityAgentServer.Network.Connections;
using CommunityAgentServer.Structuring;
using IniParser;
using IniParser.Model;
using LocalCommons.Utilities;
using Serilog;
using Serilog.Events;

namespace CommunityAgentServer
{
	public static class Program
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

		private static Timer sockettimer;

		public static void Main(string[] args)
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
			DeleteEmptyDirsAndFiles(".\\Logs\\CMA\\");
			string dates = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_CMA.log";
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Information).WriteTo.File(".\\Logs\\CMA\\Info\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Warning).WriteTo.File(".\\Logs\\CMA\\Warning\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Error).WriteTo.File(".\\Logs\\CMA\\Error\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).CreateLogger();
			Console.Title = "TR SERVER : CommunityAgentServer";
			if (!File.Exists("settings.ini"))
			{
				Log.Error("settings.ini doesn't exist!");
				return;
			}
			Boot();
			using ActorSystem actorSystem = ActorSystem.Create("CommunityAgent", "akka { loglevel=INFO,  loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]}");
			actorSystem.ActorOf(Props.Create(() => new CommunityAgent(new IPEndPoint(IPAddress.Parse(Conf.ServerIP), Conf.CommunityAgentServerPort))), "Client");
			Log.Information("CommunityAgentServer is listening on {0}:{1}", Conf.ServerIP, Conf.CommunityAgentServerPort);
			SocketTimeoutCheck();
			Console.ReadLine();
		}

		private static bool Boot()
		{
			try
			{
				IniData iniData = new FileIniDataParser().ReadFile("settings.ini");
				Conf.ServerIP = iniData["Server"]["AgentServerIP"].Replace(" ", "");
				Conf.AgentPort = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort"].Replace(" ", ""));
				Conf.AgentPort2 = Convert.ToUInt16(iniData["Server"]["AgentServerTCPPort2"].Replace(" ", ""));
				Conf.RelayPort = Convert.ToUInt16(iniData["Server"]["RelayServerPort"].Replace(" ", ""));
				Conf.CommunityAgentServerPort = Convert.ToUInt16(iniData["Server"]["CommunityServerPort"].Replace(" ", ""));
				Conf.LoadBalanceServerPort = Convert.ToUInt16(iniData["Server"]["LoadBalanceServerPort"].Replace(" ", ""));
				Conf.Connstr = iniData["Server"]["MySQLConnection"];
				Conf.HashCheck = Convert.ToBoolean(iniData["Server"]["HashCheck"].Replace(" ", ""));
				Conf.MaxUserCount = Convert.ToInt32(iniData["Server"]["MaxUserCount"].Replace(" ", ""));
				Conf.BlockDDOS = Convert.ToBoolean(iniData["Server"]["BlockDDOS"].Replace(" ", ""));
				Conf.JudgeTime = Convert.ToInt32(iniData["Server"]["JudgeTime"].Replace(" ", ""));
				Conf.MaxConnectTime = Convert.ToInt32(iniData["Server"]["MaxConnectTime"].Replace(" ", ""));
				Conf.TimedOutCheckTime = Convert.ToInt32(iniData["Server"]["TimedOutCheckTime"].Replace(" ", ""));
				Conf.EnableTimedOutCheck = Convert.ToBoolean(iniData["Server"]["EnableTimedOutCheck"].Replace(" ", ""));
				Conf.ProtocolDebug = iniData["Server"].ContainsKey("ProtocolDebug")
					&& Convert.ToBoolean(iniData["Server"]["ProtocolDebug"].Replace(" ", ""));
				Log.Information("ProtocolDebug={0}", Conf.ProtocolDebug);
				Log.Information("Loading Settings.ini........Done");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Boot Error:{0}", ex.ToString());
			}
			return false;
		}

		private static void SocketTimeoutCheck()
		{
			if (!Conf.EnableTimedOutCheck)
			{
				return;
			}
			sockettimer = new Timer(delegate
			{
				long CurrentTime = Utility.CurrentTimeMilliseconds();
				foreach (Account item in ClientConnection.CurrentAccounts.Values.Where((Account w) => !w.isDisconnected && CurrentTime > w.LastPingTime + Conf.TimedOutCheckTime).ToList())
				{
					Account value;
					try
					{
						item.Connection.Disconnect();
						ClientConnection.CurrentAccounts.TryRemove(item.NickName, out value);
					}
					catch (Exception ex)
					{
						ClientConnection.CurrentAccounts.TryRemove(item.NickName, out value);
						Log.Error("Error on disconnect timed-out connection:\r\n{0}", ex.Message);
					}
				}
			}, null, 0, 60000);
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
		}
	}
}
