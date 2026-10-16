using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LocalCommons.Utilities;
using RelayServer.Network.Connections;
using RelayServer.Structuring;
using Serilog;

namespace RelayServer
{
	public class SocketCheck : ActorBase
	{
		protected override bool Receive(object message)
		{
			long CurrentTime = Utility.CurrentTimeMilliseconds();
			List<AccountInfo> list = ClientConnection.CurrentAccounts.Values.Where((AccountInfo w) => CurrentTime > w.LastPingTime + Conf.TimedOutCheckTime).ToList();
			foreach (AccountInfo item in list)
			{
				AccountInfo value;
				try
				{
					ClientConnection.CurrentAccounts.TryRemove(item.Session, out value);
				}
				catch (Exception ex)
				{
					ClientConnection.CurrentAccounts.TryRemove(item.Session, out value);
					Log.Error("Error on remove expired connection:\r\n{0}", ex.Message);
				}
			}
			if (list.Count > 0)
			{
				Log.Information("Removed ({0}) Expired Connection.", list.Count);
			}
			return true;
		}
	}
}
