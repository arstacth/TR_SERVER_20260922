using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Akka.Actor;
using Akka.Configuration;
using IniParser;
using IniParser.Model;
using Serilog;
using Serilog.Events;

namespace LoadBalanceServer
{
	public static class Program
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern bool SetDllDirectory(string lpPathName);

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
			DeleteEmptyDirsAndFiles(".\\Logs\\LBS\\");
			string dates = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_LBS.log";
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Information).WriteTo.File(".\\Logs\\LBS\\Info\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Warning).WriteTo.File(".\\Logs\\LBS\\Warning\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Error).WriteTo.File(".\\Logs\\LBS\\Error\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).CreateLogger();
			Console.Title = "TR SERVER : LoadBalanceServer";
			if (!File.Exists("settings.ini"))
			{
				Log.Error("settings.ini doesn't exist!");
				return;
			}
			Boot();
			Config config = ConfigurationFactory.ParseString("\r\n                                akka { \r\n                                    actor {\r\n                                        provider = remote\r\n                                    }\r\n                                    remote {\r\n                                        dot-netty.tcp {\r\n                                            port = {LBSPORT}\r\n                                            hostname = 0.0.0.0\r\n                                            public-hostname = localhost\r\n                                        }\r\n                                    }\r\n                                    loglevel = INFO,\r\n                                    log-dead-letters-during-shutdown = off,\r\n                                    log-dead-letters = 0,\r\n                                    loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]\r\n                                }".Replace("{LBSPORT}", Conf.LBSLocalPort.ToString()));
			using ActorSystem actorSystem = ActorSystem.Create("LoadBalance", config);
			actorSystem.ActorOf(Props.Create(() => new LoadBalance(new IPEndPoint(IPAddress.Parse(Conf.ServerIP), Conf.LoadBalanceServerPort))), "Client");
			Server.AgentServerActor = actorSystem.ActorOf(Props.Create(() => new AgentServer()), "AgentClient");
			Log.Information("LoadBalanceServer is listening on {0}:{1}", Conf.ServerIP, Conf.LoadBalanceServerPort);
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
				Conf.LBSLocalPort = Convert.ToUInt16(iniData["Server"]["LoadBalanceServerLocalPort"].Replace(" ", ""));
				Conf.Connstr = iniData["Server"]["MySQLConnection"];
				Conf.HashCheck = Convert.ToBoolean(iniData["Server"]["HashCheck"].Replace(" ", ""));
				Conf.BlockDDOS = Convert.ToBoolean(iniData["Server"]["BlockDDOS"].Replace(" ", ""));
				Conf.JudgeTime = Convert.ToInt32(iniData["Server"]["JudgeTime"].Replace(" ", ""));
				Conf.MaxConnectTime = Convert.ToInt32(iniData["Server"]["MaxConnectTime"].Replace(" ", ""));
				Log.Information("Loading Settings.ini........Done");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Boot Error:{0}", ex.ToString());
				return false;
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
