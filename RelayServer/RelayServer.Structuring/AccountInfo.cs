using System;
using System.Net;
using LocalCommons.Network;
using Serilog;

namespace RelayServer.Structuring
{
	public class AccountInfo
	{
		public int Session { get; set; }

		public ushort UDPPort { get; set; }

		public string IP { get; set; }

		public long LastPingTime { get; set; }

		public IPEndPoint remoteIpEndPoint { get; set; }

		public bool ConfirmPoked { get; set; }

		public void SendAsync(NetPacket packet, EndPoint EndPoint)
		{
			try
			{
				RawUdpHost.Send(packet.Compile(), EndPoint);
			}
			catch (Exception ex)
			{
				Log.Error("SendAsync Error:{0}", ex.ToString());
			}
		}
	}
}
