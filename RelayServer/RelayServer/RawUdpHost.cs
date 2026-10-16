using System;

using System.IO;

using System.Net;

using System.Net.Sockets;

using System.Threading;

using RelayServer.Network.Connections;
using RelayServer.Network;

using Serilog;



namespace RelayServer

{

	public static class RawUdpHost

	{

		public static UdpClient Socket { get; private set; }



		public static UdpClient LoopSocket { get; private set; }



		public static UdpClient AnySocket { get; private set; }



		public static IPEndPoint BoundEndPoint { get; private set; }

		[ThreadStatic]
		private static UdpClient ReplySocket;


		public static void Start(int port)

		{

			// Catch-all IPv4 first so REGIST/LIVE hit even if LOGIN_OK IP differs slightly.

			try

			{

				AnySocket = BindV4(IPAddress.Any, port);

				BoundEndPoint = new IPEndPoint(IPAddress.Any, port);

				StartRecv(AnySocket, port, handle: true);

				Log.Information("Raw UDP ANY bind 0.0.0.0:{0}", port);

			}

			catch (Exception ex)

			{

				Log.Warning("ANY bind failed: {0}", ex.Message);

			}



			IPAddress lan = TryLanIp();

			if (lan != null && AnySocket == null)

			{

				try

				{

					Socket = BindV4(lan, port);

					BoundEndPoint = new IPEndPoint(lan, port);

					StartRecv(Socket, port, handle: true);

					Log.Information("Raw UDP LAN bind {0}", BoundEndPoint);

				}

				catch (Exception ex)

				{

					Log.Warning("LAN bind failed: {0}", ex.Message);

				}

			}



			// Do not bind 127.0.0.1:9155 when 0.0.0.0:9155 already owns the port.
			// ReuseAddress dual-bind can duplicate loopback datagrams and reply from
			// a different local IP than LOGIN_OK advertised.
			if (AnySocket == null)

			{

				try

				{

					LoopSocket = BindV4(IPAddress.Loopback, port);

					StartRecv(LoopSocket, port, handle: true);

					Log.Information("Raw UDP loopback bind 127.0.0.1:{0}", port);

				}

				catch (Exception ex)

				{

					Log.Warning("Loopback bind failed: {0}", ex.Message);

				}

			}



			if (Socket == null)

			{

				Socket = AnySocket ?? LoopSocket;

			}



			if (Socket == null && LoopSocket == null && AnySocket == null)

			{

				Socket = BindDual(port);

				BoundEndPoint = new IPEndPoint(IPAddress.IPv6Any, port);

				StartRecv(Socket, port, handle: true);

				Log.Information("Raw UDP fallback dual-stack [::]:{0}", port);

			}



			SelfTest(port);

			if (Conf.ProtocolDebug)
			{
				UdpHoneypot.Start();
			}

			// Do not poke unsolicited REGIST_ACK. Client CRC-fails those
			// ("CRC Failed/22/26/26") and a real REGIST never arrives if LOGIN_OK
			// sockaddr is wrong. ACK only in response to inbound opcode 21/25.

			Log.Information("Relay UDP ready any={0} lan={1} loop={2}", AnySocket != null, BoundEndPoint, LoopSocket != null);

		}



		public static void Send(byte[] data, EndPoint endPoint)

		{

			if (data == null || endPoint == null)

			{

				return;

			}

			IPEndPoint dest = (IPEndPoint)endPoint;

			IPAddress destIp = dest.Address.AddressFamily == AddressFamily.InterNetworkV6

				? (dest.Address.IsIPv4MappedToIPv6 ? dest.Address.MapToIPv4() : dest.Address)

				: dest.Address;



			UdpClient client = ReplySocket ?? PickSocketForDest(destIp);

			if (client == null)

			{

				Log.Warning("No UDP socket to send to {0}", dest);

				return;

			}

			try

			{

				if (client.Client.AddressFamily == AddressFamily.InterNetworkV6 && dest.AddressFamily == AddressFamily.InterNetwork)

				{

					dest = new IPEndPoint(dest.Address.MapToIPv6(), dest.Port);

				}

				else if (client.Client.AddressFamily == AddressFamily.InterNetwork && dest.Address.IsIPv4MappedToIPv6)

				{

					dest = new IPEndPoint(dest.Address.MapToIPv4(), dest.Port);

				}

				client.Send(data, data.Length, dest);
				if (data.Length >= 10)
				{
					int op = data[8] | (data[9] << 8);
					UdpIntercept.Server(data, dest, op, "");
				}

			}

			catch (Exception ex)

			{

				Log.Warning("UDP send to {0} failed: {1}", dest, ex.Message);

			}

		}



		private static UdpClient PickSocketForDest(IPAddress destIp)

		{

			if (IPAddress.IsLoopback(destIp))

			{

				return LoopSocket ?? AnySocket ?? Socket;

			}

			return AnySocket ?? Socket ?? LoopSocket;

		}



		private static IPAddress TryLanIp()

		{

			IPAddress parsed;

			if (!string.IsNullOrWhiteSpace(Conf.ServerIP) && IPAddress.TryParse(Conf.ServerIP, out parsed))

			{

				parsed = parsed.MapToIPv4();

				if (parsed.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(parsed))

				{

					return parsed;

				}

			}

			return null;

		}



		private static UdpClient BindV4(IPAddress ip, int port)

		{

			UdpClient client = new UdpClient(AddressFamily.InterNetwork);

			client.Client.ReceiveBufferSize = 1024 * 1024;

			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			client.Client.Bind(new IPEndPoint(ip, port));

			return client;

		}



		private static UdpClient BindDual(int port)

		{

			UdpClient client = new UdpClient(AddressFamily.InterNetworkV6);

			client.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

			client.Client.DualMode = true;

			client.Client.ReceiveBufferSize = 1024 * 1024;

			client.Client.Bind(new IPEndPoint(IPAddress.IPv6Any, port));

			return client;

		}



		private static void SelfTest(int port)

		{

			try

			{

				byte[] ping = new byte[] { 9, 9, 9, 9 };

				IPAddress lan = TryLanIp();

				if (AnySocket != null)

				{

					IPAddress dest = lan ?? IPAddress.Loopback;

					AnySocket.Send(ping, 4, new IPEndPoint(dest, port));

				}

				if (LoopSocket != null)

				{

					LoopSocket.Send(ping, 4, new IPEndPoint(IPAddress.Loopback, port));

				}

			}

			catch (Exception ex)

			{

				Log.Error("UDP self-test send failed: {0}", ex.Message);

			}

		}



		private static void StartRecv(UdpClient client, int port, bool handle)

		{

			Thread thread = new Thread(() => RecvLoop(client, port, handle))

			{

				IsBackground = true,

				Name = "RelayUdp" + port

			};

			thread.Start();

		}



		private static void RecvLoop(UdpClient client, int port, bool handle)

		{

			while (true)

			{

				try

				{

					IPEndPoint from = new IPEndPoint(IPAddress.Any, 0);

					byte[] data = client.Receive(ref from);

					// Ignore short junk (e.g. 09-09-09-09 loopback noise). Real
					// relay frames are >= 10 bytes; HandleReceived also drops <10.
					if (data == null || data.Length < 10)
					{
						continue;
					}

					if (Conf.ProtocolDebug)
					{
						string hex = BitConverter.ToString(data, 0, Math.Min(data.Length, 32));
						string line = DateTime.Now.ToString("HH:mm:ss.fff") + " UDP:" + port + " " + data.Length + " from " + from + " " + hex + Environment.NewLine;
						try
						{
							Directory.CreateDirectory("Logs\\Relay");
							File.AppendAllText("Logs\\Relay\\udp.txt", line);
						}
						catch
						{
						}
						Log.Information("UDP:{0} {1} bytes from {2} {3}", port, data.Length, from, hex);
					}

					if (handle && data.Length >= 2)

					{

						ReplySocket = client;

						try

						{

							ClientConnection.HandleReceived(data, from);

						}

						finally

						{

							ReplySocket = null;

						}

					}

				}

				catch (Exception ex)

				{

					Log.Error("UDP recv {0}: {1}", port, ex.Message);

					Thread.Sleep(100);

				}

			}

		}

	}

}


