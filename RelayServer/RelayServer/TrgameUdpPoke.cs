using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using RelayServer.Network.Connections;
using RelayServer.Network.Packet.Send;
using RelayServer.Structuring;
using LocalCommons.Utilities;
using Serilog;

namespace RelayServer
{
	/// <summary>
	/// Poke one REGIST_ACK then LIVE_ACK keepalives at trgame's local UDP port.
	/// Dual-poke caused RemainSize=16 (two REGIST_ACKs). Only loopback.
	/// </summary>
	public static class TrgameUdpPoke
	{
		private const int UdpTableOwnerPid = 1;
		private const int AfInet = 2;
		private const int AfInet6 = 23;
		private const string FlagPath = "Logs\\Relay\\login_ok.flag";
		private const int LiveIntervalMs = 2000;
		private const double MinFlagAgeSec = 0.85;
		private const double MaxRegistAgeSec = 6.0;

		[DllImport("iphlpapi.dll", SetLastError = true)]
		private static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int pdwSize, bool bOrder, int ulAf, int tableClass, uint reserved);

		private static readonly HashSet<int> LivePorts = new HashSet<int>();
		private static DateTime _flagWriteUtc = DateTime.MinValue;
		private static int _flagSession = 0;
		private static int _liveLogCounter = 0;

		public static void Start()
		{
			try
			{
				if (File.Exists(FlagPath))
				{
					File.Delete(FlagPath);
				}
			}
			catch
			{
			}
			Thread thread = new Thread(Loop)
			{
				IsBackground = true,
				Name = "TrgameUdpPoke"
			};
			thread.Start();
			Log.Information("Trgame UDP poke started (REGIST after {0:0.00}s, LIVE every {1}ms)", MinFlagAgeSec, LiveIntervalMs);
		}

		private static bool TryReadFlag(out double ageSec)
		{
			ageSec = -1;
			try
			{
				if (!File.Exists(FlagPath))
				{
					return false;
				}
				DateTime writeUtc = File.GetLastWriteTimeUtc(FlagPath);
				ageSec = (DateTime.UtcNow - writeUtc).TotalSeconds;
				if (writeUtc != _flagWriteUtc)
				{
					_flagWriteUtc = writeUtc;
					int session;
					if (!int.TryParse(File.ReadAllText(FlagPath).Trim(), out session))
					{
						session = 1;
					}
					_flagSession = session;
					lock (LivePorts)
					{
						LivePorts.Clear();
					}
					Log.Information("login_ok.flag refreshed session={0} age={1:0.000}s", session, ageSec);
				}
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void Loop()
		{
			HashSet<int> poked = new HashSet<int>();
			DateTime registDoneForFlag = DateTime.MinValue;
			long liveTick = 0;
			while (true)
			{
				try
				{
					HashSet<int> pids = TrgamePids();
					if (pids.Count == 0)
					{
						poked.Clear();
						registDoneForFlag = DateTime.MinValue;
						lock (LivePorts)
						{
							LivePorts.Clear();
						}
					}
					else
					{
						double age;
						bool hasFlag = TryReadFlag(out age);
						if (hasFlag && registDoneForFlag != _flagWriteUtc
							&& age >= MinFlagAgeSec && age <= MaxRegistAgeSec)
						{
							foreach (int port in GetOwnedUdpPorts(pids))
							{
								if (port == Conf.RelayPort || port < 5000 || port > 65000)
								{
									continue;
								}
								if (!poked.Add(port))
								{
									continue;
								}
								PokeRegist(port);
								lock (LivePorts)
								{
									LivePorts.Add(port);
								}
							}
							if (poked.Count > 0)
							{
								registDoneForFlag = _flagWriteUtc;
								poked.Clear();
							}
						}
					}

					if (Environment.TickCount - liveTick > LiveIntervalMs)
					{
						liveTick = Environment.TickCount;
						int[] snapshot;
						lock (LivePorts)
						{
							snapshot = new int[LivePorts.Count];
							LivePorts.CopyTo(snapshot);
						}
						foreach (int port in snapshot)
						{
							PokeLive(port);
						}
						if (snapshot.Length > 0 && (++_liveLogCounter % 5) == 1)
						{
							Append(DateTime.Now.ToString("HH:mm:ss.fff") + " POKE LIVE ports=" + string.Join(",", snapshot) + Environment.NewLine);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Warning("TrgameUdpPoke: {0}", ex.Message);
				}
				Thread.Sleep(100);
			}
		}

		private static HashSet<int> TrgamePids()
		{
			HashSet<int> pids = new HashSet<int>();
			foreach (Process process in Process.GetProcessesByName("trgame"))
			{
				pids.Add(process.Id);
			}
			return pids;
		}

		private static int LoginSession()
		{
			return _flagSession != 0 ? _flagSession : 1;
		}

		private static void PokeRegist(int port)
		{
			try
			{
				// One poke only — dual REGIST_ACK caused RemainSize=16.
				IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, port);
				int session = LoginSession();
				AccountInfo accountInfo = new AccountInfo
				{
					Session = session,
					UDPPort = (ushort)port,
					IP = "127.0.0.1",
					LastPingTime = Utility.CurrentTimeMilliseconds(),
					remoteIpEndPoint = ep
				};
				ClientConnection.CurrentAccounts.AddOrUpdate(session, accountInfo, (int _, AccountInfo __) => accountInfo);
				byte[] compiled = new Connect_04FF5604(ep).Compile();
				RawUdpHost.Send(compiled, ep);
				Append(DateTime.Now.ToString("HH:mm:ss.fff") + " POKE REGIST 127.0.0.1:" + port + " bytes=" + compiled.Length + " session=" + session + Environment.NewLine);
				Log.Information("Poked REGIST_ACK 127.0.0.1:{0} bytes={1} session={2}", port, compiled.Length, session);
			}
			catch (Exception ex)
			{
				Log.Warning("PokeRegist :{0} failed: {1}", port, ex.Message);
			}
		}

		private static void PokeLive(int port)
		{
			try
			{
				RawUdpHost.Send(new Connect_04FF5A04().Compile(), new IPEndPoint(IPAddress.Loopback, port));
			}
			catch (Exception ex)
			{
				Log.Warning("PokeLive :{0} failed: {1}", port, ex.Message);
			}
		}

		private static void Append(string line)
		{
			if (!Conf.ProtocolDebug)
			{
				return;
			}
			try
			{
				Directory.CreateDirectory("Logs\\Relay");
				File.AppendAllText("Logs\\Relay\\udp.txt", line);
			}
			catch
			{
			}
		}

		private static List<int> GetOwnedUdpPorts(HashSet<int> pids)
		{
			List<int> ports = new List<int>();
			AddTable(pids, ports, AfInet, 12);
			AddTable(pids, ports, AfInet6, 28);
			return ports;
		}

		private static void AddTable(HashSet<int> pids, List<int> ports, int family, int rowSize)
		{
			int size = 0;
			GetExtendedUdpTable(IntPtr.Zero, ref size, true, family, UdpTableOwnerPid, 0);
			if (size <= 0)
			{
				return;
			}
			IntPtr buf = Marshal.AllocHGlobal(size);
			try
			{
				if (GetExtendedUdpTable(buf, ref size, true, family, UdpTableOwnerPid, 0) != 0)
				{
					return;
				}
				int count = Marshal.ReadInt32(buf);
				IntPtr row = IntPtr.Add(buf, 4);
				for (int i = 0; i < count; i++)
				{
					int pid;
					int netPort;
					if (family == AfInet)
					{
						netPort = Marshal.ReadInt32(row, 4);
						pid = Marshal.ReadInt32(row, 8);
					}
					else
					{
						netPort = Marshal.ReadInt32(row, 20);
						pid = Marshal.ReadInt32(row, 24);
					}
					if (pids.Contains(pid))
					{
						int port = (ushort)((netPort & 0xFF) << 8 | ((netPort >> 8) & 0xFF));
						if (port > 0 && !ports.Contains(port))
						{
							ports.Add(port);
						}
					}
					row = IntPtr.Add(row, rowSize);
				}
			}
			finally
			{
				Marshal.FreeHGlobal(buf);
			}
		}
	}
}
