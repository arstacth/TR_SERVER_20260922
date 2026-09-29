using System;
using System.Net;
using Akka.Actor;
using Akka.IO;
using LoadBalanceServer.Network.Connections;
using LocalCommons.Network;
using Serilog;

namespace LoadBalanceServer
{
	public class LoadBalanceHandler : ReceiveActor
	{
		public LoadBalanceHandler(EndPoint remote, ClientConnection connection)
		{
			LoadBalanceHandler loadBalanceHandler = this;
			UntypedActor.Context.Watch(base.Self);
			Receive(delegate(Tcp.Received received)
			{
				PacketReader packetReader = new PacketReader(received.Data.ToArray(), 0);
				_ = packetReader.Size;
				int num = packetReader.ReadLEUInt16() - 2;
				ushort srcOffset = 2;
				byte[] array = new byte[num];
				Buffer.BlockCopy(packetReader.Buffer, srcOffset, array, 0, num);
				connection.HandleReceived(array);
				packetReader.Clear();
			});
			Receive<Tcp.ConnectionClosed>(delegate
			{
				Log.Information("Client: {0} closed", remote);
				UntypedActor.Context.Stop(loadBalanceHandler.Self);
			});
			Receive<Terminated>(delegate
			{
				Log.Information("Client: {0} died", remote);
				UntypedActor.Context.Stop(loadBalanceHandler.Self);
			});
		}
	}
}
