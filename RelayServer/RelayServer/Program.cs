using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Akka.Actor;
using Akka.Quartz.Actor;
using Akka.Quartz.Actor.Commands;
using IniParser;
using IniParser.Model;
using Quartz;
using RelayServer.Network.Connections;
using Serilog;
using Serilog.Events;

namespace RelayServer
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
			DeleteEmptyDirsAndFiles(".\\Logs\\Relay\\");
			string dates = DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_Relay.log";
			Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().MinimumLevel.Override("Quartz", LogEventLevel.Warning).WriteTo.Console().WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Information).WriteTo.File(".\\Logs\\Relay\\Info\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Warning).WriteTo.File(".\\Logs\\Relay\\Warning\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).WriteTo.Logger(delegate(LoggerConfiguration l)
			{
				l.Filter.ByIncludingOnly((LogEvent e) => e.Level == LogEventLevel.Error).WriteTo.File(".\\Logs\\Relay\\Error\\" + dates, LogEventLevel.Verbose, "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}", null, 1073741824L, null, buffered: false, shared: false, null, RollingInterval.Infinite, rollOnFileSizeLimit: false, 31);
			}).CreateLogger();
			Console.Title = "TR SERVER : RelayServer";
			if (!File.Exists("settings.ini"))
			{
				Log.Error("settings.ini doesn't exist!");
				return;
			}
			Boot();
			using ActorSystem actorSystem = ActorSystem.Create("Relay", "akka { loglevel=WARNING,  loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]}");
			IActorRef receiver = actorSystem.ActorOf(Props.Create(() => new QuartzActor()), "Quartz");
			IActorRef handler = actorSystem.ActorOf(Props.Create(() => new Handler()));
			RawUdpHost.Start(Conf.RelayPort);
			actorSystem.ActorOf(Props.Create(() => new RelayServer(handler, new IPEndPoint(IPAddress.Any, Conf.RelayPort))), "Client");
			if (Conf.EnableTimedOutCheck)
			{
				IActorRef to = actorSystem.ActorOf(Props.Create(() => new SocketCheck()), "ExpireCheck");
				receiver.Tell(new CreateJob(to, "", TriggerBuilder.Create().WithSimpleSchedule(delegate(SimpleScheduleBuilder x)
				{
					x.WithIntervalInSeconds(60).RepeatForever();
				}).Build()));
			}
			Console.ReadLine();
		}

		private static void InstallAgentServer()
		{
			Socket socket;
			do
			{
				IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(Conf.ServerIP), Conf.AgentPort2);
				socket = new Socket(iPEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				try
				{
					socket.Connect(iPEndPoint);
				}
				catch (Exception)
				{
					Log.Warning("Unable to Connect Agent Server, Retry After 1 Second");
				}
			}
			while (!socket.Connected);
			new AgentConnection(socket);
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
				Conf.TimedOutCheckTime = Convert.ToInt32(iniData["Server"]["TimedOutCheckTime"].Replace(" ", ""));
				Conf.EnableTimedOutCheck = Convert.ToBoolean(iniData["Server"]["EnableTimedOutCheck"].Replace(" ", ""));
				Conf.ProtocolDebug = iniData["Server"].ContainsKey("ProtocolDebug")
					&& Convert.ToBoolean(iniData["Server"]["ProtocolDebug"].Replace(" ", ""));
				Log.Information("Loading Settings.ini........Done");
				Log.Information("ProtocolDebug={0}", Conf.ProtocolDebug);
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message);
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
