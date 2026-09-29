using System;
using System.IO;
using System.Text;
using CommunityAgentServer;

namespace CommunityAgentServer.Network
{
	/// <summary>
	/// Intercepts community TCP on 9000. Logs\CMA\intercept.txt
	/// Official first packet after connect is opcode 2 (version+nick), ACK opcode 3.
	/// </summary>
	public static class CmIntercept
	{
		private static readonly object Gate = new object();
		private static bool _started;

		public static void Client(byte[] body, int opcode, string note)
		{
			Write("C->S", body, opcode, note);
		}

		public static void Server(byte[] wire, int opcode, string note)
		{
			Write("S->C", wire, opcode, note);
		}

		public static void Raw(byte[] buffer, int offset, int claimedLen, string note)
		{
			int n = 0;
			if (buffer != null)
			{
				n = Math.Min(64, Math.Max(0, buffer.Length - offset));
			}
			string hex = n > 0 ? BitConverter.ToString(buffer, offset, n) : "";
			WriteLine(string.Format(
				"{0}  RAW  claimedLen={1}  {2}\r\n             {3}\r\n",
				DateTime.Now.ToString("HH:mm:ss.fff"),
				claimedLen,
				note,
				hex));
		}

		private static void Write(string dir, byte[] data, int opcode, string note)
		{
			int len = data == null ? 0 : data.Length;
			string hex = "";
			if (data != null && len > 0)
			{
				int n = Math.Min(len, 96);
				hex = BitConverter.ToString(data, 0, n);
				if (len > n)
				{
					hex += "...";
				}
			}
			string extra = string.IsNullOrEmpty(note) ? "" : "  " + note;
			WriteLine(string.Format(
				"{0}  {1}  CMA  op={2}  len={3}{4}\r\n             {5}\r\n",
				DateTime.Now.ToString("HH:mm:ss.fff"),
				dir,
				opcode,
				len,
				extra,
				string.IsNullOrEmpty(hex) ? "-" : hex));
		}

		private static void WriteLine(string line)
		{
			if (!Conf.ProtocolDebug)
			{
				return;
			}
			try
			{
				lock (Gate)
				{
					Directory.CreateDirectory("Logs\\CMA");
					string path = "Logs\\CMA\\intercept.txt";
					if (!_started)
					{
						File.AppendAllText(path, "\r\n===== intercept " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " =====\r\n", Encoding.UTF8);
						_started = true;
					}
					File.AppendAllText(path, line, Encoding.UTF8);
				}
			}
			catch
			{
			}
		}
	}
}
