using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace AgentServer
{
	public static class Conf
	{
		public static string Connstr = "server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=utf8mb4;";

		public static string ServerIP = "";

		/// <summary>
		/// IP written into LOGIN_OK relay sockaddr + settings RelayServerIP.
		/// Must match the Agent TCP address the client used (trgame -zone:), or the
		/// Lobby watchdog fires "firewall/sharing" at 20s if relay UDP fails.
		/// Honor settings even when that IP is 127.0.0.1.
		/// </summary>
		public static string RelayAdvertiseIP = "";

		public static int AgentPort = 0;

		public static int AgentPort2 = 0;

		public static int RelayPort = 0;

		public static string GetRelayAdvertiseIP()
		{
			IPAddress parsed;
			if (!string.IsNullOrWhiteSpace(RelayAdvertiseIP) && IPAddress.TryParse(RelayAdvertiseIP.Trim(), out parsed)
				&& parsed.AddressFamily == AddressFamily.InterNetwork)
			{
				// Local default is 127.0.0.1, but public deployments set AgentServerIP to the
				// reachable address. Remote clients cannot UDP-register against loopback relay.
				if (IPAddress.IsLoopback(parsed) && !string.IsNullOrWhiteSpace(ServerIP)
					&& IPAddress.TryParse(ServerIP, out IPAddress serverParsed)
					&& !IPAddress.IsLoopback(serverParsed))
				{
					return serverParsed.MapToIPv4().ToString();
				}
				return parsed.MapToIPv4().ToString();
			}
			if (!string.IsNullOrWhiteSpace(ServerIP) && IPAddress.TryParse(ServerIP, out parsed)
				&& parsed.AddressFamily == AddressFamily.InterNetwork)
			{
				return parsed.MapToIPv4().ToString();
			}
			string best = PickLocalIPv4(preferTestNet: false);
			if (!string.IsNullOrEmpty(best))
			{
				return best;
			}
			return "127.0.0.1";
		}

		public static string PickLocalIPv4(bool preferTestNet)
		{
			var found = new System.Collections.Generic.List<IPAddress>();
			try
			{
				found.AddRange(Dns.GetHostAddresses(Dns.GetHostName()).Where(a => a.AddressFamily == AddressFamily.InterNetwork));
			}
			catch
			{
			}
			try
			{
				found.AddRange(System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
					.Where(n => n.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
					.SelectMany(n => n.GetIPProperties().UnicastAddresses)
					.Select(u => u.Address)
					.Where(a => a.AddressFamily == AddressFamily.InterNetwork));
			}
			catch
			{
			}
			IPAddress[] uniq = found.Distinct().ToArray();
			if (preferTestNet)
			{
				IPAddress testNet = uniq.FirstOrDefault(a =>
				{
					byte[] b = a.GetAddressBytes();
					return b.Length == 4 && b[0] == 203 && b[1] == 0 && b[2] == 113;
				});
				if (testNet != null)
				{
					return testNet.ToString();
				}
			}
			IPAddress ethernet = uniq.FirstOrDefault(IsPreferredLan);
			if (ethernet != null)
			{
				return ethernet.ToString();
			}
			IPAddress any = uniq.FirstOrDefault(IsAdvertisable);
			return any == null ? null : any.ToString();
		}

		private static bool IsApipa(IPAddress a)
		{
			byte[] b = a.GetAddressBytes();
			return b.Length == 4 && b[0] == 169 && b[1] == 254;
		}

		private static bool IsVmnet(IPAddress a)
		{
			byte[] b = a.GetAddressBytes();
			return b.Length == 4 && b[0] == 192 && b[1] == 168 && (b[2] == 70 || b[2] == 19);
		}

		private static bool IsPreferredLan(IPAddress a)
		{
			if (!IsAdvertisable(a))
			{
				return false;
			}
			byte[] b = a.GetAddressBytes();
			return b[0] == 192 && b[1] == 168 && b[2] == 1;
		}

		/// <summary>
		/// LOGIN_OK / settings may only advertise a local TEST-NET alias or RFC1918.
		/// Never bind a public advertise IP on the loopback adapter.
		/// </summary>
		public static bool IsAdvertisable(IPAddress a)
		{
			if (a == null || a.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(a) || IsApipa(a) || IsVmnet(a))
			{
				return false;
			}
			byte[] b = a.GetAddressBytes();
			if (b.Length != 4)
			{
				return false;
			}
			if (b[0] == 203 && b[1] == 0 && b[2] == 113)
			{
				return true;
			}
			if (b[0] == 10)
			{
				return true;
			}
			if (b[0] == 192 && b[1] == 168)
			{
				return true;
			}
			if (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
			{
				return true;
			}
			return false;
		}

		public static int CommunityAgentServerPort = 0;

		public static int LoadBalanceServerPort = 0;

		public static int LBSLocalPort = 0;

		public static string RMServerIP = "";

		public static int RMLocalPort = 0;

		public static bool HashCheck = false;

		public static int MaxUserCount = 150;

		public static int MaxTotalAgentUserCount = 100;

		public static bool useLoginTrafficManager = false;

		public static int TimedOutCheckTime = 30000;

		public static bool EnableTimedOutCheck = true;

		/// <summary>
		/// When true: ProtocolDump / intercept / RX wire spam / verbose protocol logs.
		/// When false: quiet. Set ProtocolDebug=true in settings.ini [Server].
		/// </summary>
		public static bool ProtocolDebug = true;

		/// R186259 1.3.3.1 MyRoom GET/SAVE/slots: 13×10 dye bytes (176-byte half).
		/// GET_AVATAR memcpy 353 = 2×(46+130)+1 costume bool.
		public const int AvatarDyePadBytes = 130;

		public static bool BlockDDOS = false;

		public static int JudgeTime = 3;

		public static int MaxConnectTime = 30;

		/// <summary>Optional XignCode SDK load (off by default; needs bin\xigncode package).</summary>
		public static bool EnableXignCode = false;
	}
}
