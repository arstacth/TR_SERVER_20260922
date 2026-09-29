using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AgentServer;

namespace AgentServer.Network
{
	/// <summary>
	/// Intercepts client TCP: decrypted RX and TX.
	/// Readable log: Logs\Agent\intercept.txt
	/// Table log:     Logs\Agent\protocol_dump.tsv
	/// </summary>
	public static class ProtocolDump
	{
		private static readonly object Gate = new object();
		private static readonly Dictionary<ushort, string> WireNames = LoadNames();
		private static readonly HashSet<ushort> SeenWires = new HashSet<ushort>();
		private static bool _headerWritten;
		private static bool _interceptStarted;
		private static bool _seenHeaderWritten;

		public static void Rx(ushort wire, ushort server, byte[] body, bool isLogin, string ip)
		{
			Write("C->S", wire, server, body, isLogin, ip, unhandled: false);
		}

		public static void Unhandled(ushort wire, ushort server, byte[] body, string ip)
		{
			Write("C->S", wire, server, body, isLogin: true, ip, unhandled: true);
		}

		public static void Tx(ushort server, ushort wire, byte[] body, bool isLogin)
		{
			Write("S->C", wire, server, body, isLogin, "", unhandled: false);
		}

		private static bool VerboseDump()
		{
			return File.Exists("Logs\\Agent\\protocol_dump_verbose");
		}

		private static void Write(string dir, ushort wire, ushort server, byte[] body, bool isLogin, string ip, bool unhandled)
		{
			if (!Conf.ProtocolDebug)
			{
				return;
			}
			bool live = wire == 322 || server == 699 || server == 648;
			bool verbose = VerboseDump();
			// LIVE TX is ~60pps of the same opcode; still logged once in opcodes_seen via C->S.
			if (live && isLogin && dir == "S->C" && !unhandled)
			{
				return;
			}
			try
			{
				string name = NameOf(wire, server);
				int len = body == null ? 0 : body.Length;
				int hexMax;
				if (live && !unhandled)
				{
					hexMax = 16;
				}
				else if (verbose)
				{
					hexMax = int.MaxValue;
				}
				else
				{
					hexMax = 256;
				}
				string hex = Hex(body, hexMax);
				ushort inner = 0;
				if (body != null && body.Length >= 4)
				{
					inner = (ushort)(body[2] | (body[3] << 8));
				}
				string phase = isLogin ? "login" : "prelogin";
				string flag = unhandled ? "UNHANDLED" : (wire != server ? "remap" : "");
				string tsv = string.Format(
					"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
					DateTime.Now.ToString("HH:mm:ss.fff"),
					dir,
					phase,
					wire,
					server,
					name,
					len,
					ip,
					flag,
					inner,
					hex);
				string human = string.Format(
					"{0}  {1}  TCP  wire={2} (0x{2:X})  map={3}  {4}  len={5}{6}{7}\r\n             {8}\r\n",
					DateTime.Now.ToString("HH:mm:ss.fff"),
					dir,
					wire,
					server,
					name,
					len,
					string.IsNullOrEmpty(ip) ? "" : "  ip=" + ip,
					unhandled ? "  ** UNHANDLED — no Agent switch case **" : "",
					string.IsNullOrEmpty(hex) ? "-" : hex);
				lock (Gate)
				{
					Directory.CreateDirectory("Logs\\Agent");
					string tsvPath = "Logs\\Agent\\protocol_dump.tsv";
					if (!_headerWritten && !File.Exists(tsvPath))
					{
						File.AppendAllText(tsvPath, "time\tdir\tphase\twire\tserver\tname\tlen\tip\tflag\tinner\thex\r\n", Encoding.UTF8);
					}
					_headerWritten = true;
					File.AppendAllText(tsvPath, tsv + "\r\n", Encoding.UTF8);
					string seenPath = "Logs\\Agent\\opcodes_seen.tsv";
					if (!_seenHeaderWritten && !File.Exists(seenPath))
					{
						File.AppendAllText(seenPath, "first\tdir\twire\tserver\tname\tinner\tlen\tflag\r\n", Encoding.UTF8);
						_seenHeaderWritten = true;
					}
					if (SeenWires.Add(wire))
					{
						File.AppendAllText(
							seenPath,
							string.Format(
								"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\r\n",
								DateTime.Now.ToString("HH:mm:ss.fff"),
								dir,
								wire,
								server,
								name,
								inner,
								len,
								flag),
							Encoding.UTF8);
					}
					string intercept = "Logs\\Agent\\intercept.txt";
					if (!_interceptStarted)
					{
						File.AppendAllText(intercept, "\r\n===== intercept " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " =====\r\n", Encoding.UTF8);
						_interceptStarted = true;
					}
					File.AppendAllText(intercept, human, Encoding.UTF8);
				}
			}
			catch
			{
			}
		}

		private static string NameOf(ushort wire, ushort server)
		{
			if (wire == 1077 || server == 1077)
			{
				return "CLIENT_LOGIN_TIMEOUT_REPORT";
			}
			string name;
			if (WireNames.TryGetValue(wire, out name))
			{
				return name;
			}
			if (WireNames.TryGetValue(server, out name))
			{
				return name;
			}
			return "?";
		}

		private static string Hex(byte[] body, int max)
		{
			if (body == null || body.Length == 0)
			{
				return "";
			}
			int n = Math.Min(body.Length, max);
			string hex = BitConverter.ToString(body, 0, n);
			if (body.Length > n)
			{
				hex += "...";
			}
			return hex;
		}

		private static Dictionary<ushort, string> LoadNames()
		{
			Dictionary<ushort, string> map = new Dictionary<ushort, string>();
			string[] paths =
			{
				"tools\\trgame_client_opcodes.tsv",
				"..\\tools\\trgame_client_opcodes.tsv",
				"trgame_client_opcodes.tsv"
			};
			foreach (string path in paths)
			{
				try
				{
					if (!File.Exists(path))
					{
						continue;
					}
					foreach (string line in File.ReadAllLines(path))
					{
						if (string.IsNullOrWhiteSpace(line) || line.StartsWith("index"))
						{
							continue;
						}
						string[] parts = line.Split('\t');
						ushort op;
						if (parts.Length >= 2 && ushort.TryParse(parts[0], out op))
						{
							map[op] = parts[1].Trim();
						}
					}
					break;
				}
				catch
				{
				}
			}
			return map;
		}
	}
}
