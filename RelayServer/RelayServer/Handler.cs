using System;
using Akka.Actor;
using Akka.IO;
using RelayServer.Network.Connections;
using Serilog;

namespace RelayServer
{
	public class Handler : ReceiveActor
	{
		public Handler()
		{
			Receive(delegate(Udp.Received x)
			{
				try
				{
					int count = x.Data.Count;
					byte[] array = new byte[count];
					Buffer.BlockCopy(x.Data.ToArray(), 0, array, 0, count);
					if (Conf.ProtocolDebug)
					{
						Log.Information("UDP datagram {0} bytes from {1}", count, x.Sender);
					}
					ClientConnection.HandleReceived(array, x.Sender);
				}
				catch (Exception ex)
				{
					Log.Error("Udp.Received Error:{0}", ex.ToString());
				}
			});
		}
	}
}
