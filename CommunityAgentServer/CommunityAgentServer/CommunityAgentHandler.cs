using System;
using System.IO;
using System.Net;
using Akka.Actor;
using Akka.IO;
using CommunityAgentServer.Network.Connections;
using Serilog;

namespace CommunityAgentServer
{
	public class CommunityAgentHandler : ReceiveActor
	{
		public CommunityAgentHandler(EndPoint remote, ClientConnection connection)
		{
			CommunityAgentHandler communityAgentHandler = this;
			UntypedActor.Context.Watch(base.Self);
			MemoryStream memoryStream = new MemoryStream();
			Receive(delegate(Tcp.Received received)
			{
				int count = received.Data.Count;
				memoryStream.Write(received.Data.ToArray(), 0, count);
				byte[] array = memoryStream.ToArray();
				int num = 0;
				if (count > 0)
				{
					while (true)
					{
						int num2 = 0;
						num2 = ((array.Length - num >= 2) ? BitConverter.ToUInt16(array, num) : (-1));
						if (array.Length - num < num2 || num2 == -1)
						{
							break;
						}
						int num3 = num2 - 2;
						byte[] array2 = new byte[num3];
						Buffer.BlockCopy(array, num + 2, array2, 0, num3);
						connection.HandleReceived(array2);
						num += num2;
					}
					memoryStream.Close();
					memoryStream.Dispose();
					memoryStream = new MemoryStream();
					memoryStream.Write(array, num, array.Length - num);
				}
			});
			Receive<Tcp.ConnectionClosed>(delegate
			{
				try
				{
					connection.CurrentAccount.isDisconnected = true;
					ClientConnection.CurrentAccounts.TryRemove(connection.CurrentAccount.NickName, out var _);
				}
				catch
				{
					Log.Warning("Client: {0} disconnected,But the remove fail", remote);
				}
				Log.Information("Client: {0} disconnected", remote);
				UntypedActor.Context.Stop(communityAgentHandler.Self);
			});
			Receive<Terminated>(delegate
			{
				try
				{
					connection.CurrentAccount.isDisconnected = true;
					ClientConnection.CurrentAccounts.TryRemove(connection.CurrentAccount.NickName, out var _);
				}
				catch
				{
					Log.Warning("Client: {0} died,But the remove fail", remote);
				}
				Log.Information("Client: {0} died", remote);
				UntypedActor.Context.Stop(communityAgentHandler.Self);
			});
		}
	}
}
