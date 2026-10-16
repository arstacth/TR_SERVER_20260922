using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RelayServer
{
	public static class Conf
	{
		public static string Connstr = "server=127.0.0.1;port=3306;user id=root;password=;database=tr_game_db;charset=utf8mb4";

		public static string ServerIP = "";

		public static int AgentPort = 0;

		public static int AgentPort2 = 0;

		public static int RelayPort = 0;

		public static int CommunityAgentServerPort = 0;

		public static int LoadBalanceServerPort = 0;

		public static bool HashCheck = false;

		public static int MaxUserCount = 0;

		public static int TimedOutCheckTime = 30000;

		public static bool EnableTimedOutCheck = true;

		/// <summary>
		/// When true: UDP honeypot, udp.txt, intercept, per-packet UDP logs.
		/// settings.ini [Server] ProtocolDebug=
		/// </summary>
		public static bool ProtocolDebug = false;

		/// <summary>
		/// Official REGIST_ACK echoes the client's public mapped UDP endpoint,
		/// which is not the socket's local address. Loopback echo == local, so
		/// NAT compare never takes the "mapped" path on this client.
		/// Keep the real source for sending; only the sockaddr body is rewritten.
		/// </summary>
		public static IPEndPoint MappedRegistEcho(IPEndPoint source)
		{
			if (source == null)
			{
				return source;
			}
			IPAddress src = source.Address.MapToIPv4();
			IPAddress mapped = PickDifferentLocal(src);
			return new IPEndPoint(mapped, source.Port);
		}

		public static IPAddress PickDifferentLocal(IPAddress src)
		{
			IPAddress[] locals = EnumLocalIPv4().Where(IsAdvertisable).ToArray();
			IPAddress testNet = locals.FirstOrDefault(IsTestNet);
			if (testNet != null && !Same(testNet, src))
			{
				return testNet;
			}
			IPAddress lan = locals.FirstOrDefault(IsPreferredLan);
			if (lan != null && !Same(lan, src))
			{
				return lan;
			}
			IPAddress other = locals.FirstOrDefault(a => !Same(a, src));
			if (other != null)
			{
				return other;
			}
			if (IsTestNet(src))
			{
				return IPAddress.Parse("192.168.1.101");
			}
			return IPAddress.Parse("203.0.113.10");
		}

		private static IEnumerable<IPAddress> EnumLocalIPv4()
		{
			var found = new List<IPAddress>();
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
			return found.Distinct();
		}

		private static bool Same(IPAddress a, IPAddress b)
		{
			if (a == null || b == null)
			{
				return false;
			}
			return a.MapToIPv4().Equals(b.MapToIPv4());
		}

		private static bool IsTestNet(IPAddress a)
		{
			byte[] b = a.GetAddressBytes();
			return b.Length == 4 && b[0] == 203 && b[1] == 0 && b[2] == 113;
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

		private static bool IsAdvertisable(IPAddress a)
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
	}
}
