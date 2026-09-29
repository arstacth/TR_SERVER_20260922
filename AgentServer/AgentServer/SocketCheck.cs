using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Network.Connections;
using AgentServer.Structuring;
using Akka.Actor;
using LocalCommons.Utilities;
using Serilog;

namespace AgentServer
{
	public class SocketCheck : ActorBase
	{
		protected override bool Receive(object message)
		{
			long CurrentTime = Utility.CurrentTimeMilliseconds();
			List<Account> list = ClientConnection.CurrentAccounts.Values.Where((Account w) => !w.isDisconnected && CurrentTime > w.LastPingTime + Conf.TimedOutCheckTime).ToList();
			foreach (Account item in list)
			{
				try
				{
					item.Connection.Disconnect();
					ClientConnection.CurrentAccounts.TryRemove(item.Session, out var value);
					value?.Connection.ClientConnection_DisconnectedEvent();
				}
				catch (Exception ex)
				{
					ClientConnection.CurrentAccounts.TryRemove(item.Session, out var _);
					Log.Error("Error on Disconnect Timed-Out Connection:\r\n{0}", ex.Message);
				}
			}
			if (list.Count > 0)
			{
				Log.Information("Removed ({0}) Timed-Out Connection.", list.Count);
			}
			return true;
		}
	}
}
