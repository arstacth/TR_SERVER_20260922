using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RelayServer;

namespace RelayServer.Network
{
	/// <summary>
	/// Intercepts client UDP on 9155. Logs\Relay\intercept.txt
	/// </summary>
	public static class UdpIntercept
	{
		private static readonly object Gate = new object();
		private static bool _started;
		private static readonly Dictionary<int, string> Names = new Dictionary<int, string>
		{
			{ 21, "eRelayServer_REGIST_REQ" },
			{ 22, "eRelayServer_REGIST_ACK" },
			{ 23, "eRelayServer_P2P_WRAP_REQ" },
			{ 24, "eRelayServer_P2P_WRAP_ACK" },
			{ 25, "eRelayServer_LIVE_MSG_REQ" },
			{ 26, "eRelayServer_LIVE_MSG_ACK" },
			{ 28, "eUDPProtocol_PING_REQ" },
			{ 29, "eUDPProtocol_PING_ACK" },
			{ 30, "eUDPProtocol_NEED_CONFIRM_REQ" },
			{ 31, "eUDPProtocol_NEED_CONFIRM_ACK" },
			{ 38, "eUDPProtocol_FOR_CONNECT_CONFIRM_REQ" },
			{ 39, "eUDPProtocol_FOR_CONNECT_CONFIRM_ACK" }
		};

		public static void Client(byte[] data, EndPoint from, int opcode, string note)
		{
			Write("C->S", data, from, opcode, note);
		}

		public static void Server(byte[] data, EndPoint to, int opcode, string note)
		{
			Write("S->C", data, to, opcode, note);
		}

		private static void Write(string dir, byte[] data, EndPoint ep, int opcode, string note)
		{
			if (!Conf.ProtocolDebug)
			{
				return;
			}
			try
			{
				string name;
				if (!Names.TryGetValue(opcode, out name))
				{
					name = opcode > 0 ? "?" : "raw";
				}
				int len = data == null ? 0 : data.Length;
				string hex = "";
				if (data != null && len > 0)
				{
					int n = Math.Min(len, 64);
					hex = BitConverter.ToString(data, 0, n);
					if (len > n)
					{
						hex += "...";
					}
				}
				string extra = string.IsNullOrEmpty(note) ? "" : "  " + note;
				string line = string.Format(
					"{0}  {1}  UDP  op={2}  {3}  len={4}  {5}{6}\r\n             {7}\r\n",
					DateTime.Now.ToString("HH:mm:ss.fff"),
					dir,
					opcode,
					name,
					len,
					ep,
					extra,
					hex);
				lock (Gate)
				{
					Directory.CreateDirectory("Logs\\Relay");
					if (!_started)
					{
						File.AppendAllText("Logs\\Relay\\intercept.txt", "\r\n===== intercept " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " =====\r\n", Encoding.UTF8);
						_started = true;
					}
					File.AppendAllText("Logs\\Relay\\intercept.txt", line, Encoding.UTF8);
				}
			}
			catch
			{
			}
		}
	}
}
