using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RelayServer.Network.Connections;
using Serilog;

namespace RelayServer
{
	/// <summary>
	/// Client Sent=2 never hits :9155. Listen on common mis-parsed ports
	/// (e.g. 9155 with swapped endian = 49955) to learn the real destination.
	/// </summary>
	public static class UdpHoneypot
	{
		// 9155 BE misread as LE -> 0xC323 = 49955
		private static readonly int[] Ports = { 49955, 9153, 9500, 2345, 9000 };

		public static void Start()
		{
			foreach (int port in Ports)
			{
				int p = port;
				Thread thread = new Thread(() => Listen(p))
				{
					IsBackground = true,
					Name = "UdpHoney" + p
				};
				thread.Start();
			}
			Log.Information("UDP honeypot listening on {0}", string.Join(",", Ports));
		}

		private static void Listen(int port)
		{
			UdpClient client;
			try
			{
				client = new UdpClient(AddressFamily.InterNetwork);
				client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
			}
			catch (Exception ex)
			{
				Log.Warning("Honeypot bind {0} failed: {1}", port, ex.Message);
				return;
			}
			while (true)
			{
				try
				{
					IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = client.Receive(ref from);
					string hex = BitConverter.ToString(data, 0, Math.Min(data.Length, 48));
					string line = DateTime.Now.ToString("HH:mm:ss.fff") + " HONEY:" + port + " " + data.Length + " from " + from + " " + hex + Environment.NewLine;
					Directory.CreateDirectory("Logs\\Relay");
					File.AppendAllText("Logs\\Relay\\udp.txt", line);
					Log.Warning("HONEYPOT hit :{0} {1}B from {2} {3}", port, data.Length, from, hex);
					if (data.Length >= 10)
					{
						ClientConnection.HandleReceived(data, from);
					}
				}
				catch (Exception ex)
				{
					Log.Warning("Honeypot {0}: {1}", port, ex.Message);
					Thread.Sleep(200);
				}
			}
		}
	}
}
