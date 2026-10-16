using System.Collections.Generic;
using System.Linq;
using AgentServer.Network.Connections;
using AgentServer.Structuring;
using Akka.Actor;
using LocalCommons.Utilities;

namespace AgentServer.Holders
{
	public class LoginTrafficActor : ActorBase
	{
		private long tLastNoticeLoginWaitTime;

		protected override bool Receive(object message)
		{
			long num = Utility.CurrentTimeMilliseconds();
			if (LoginTrafficManager.canAcceptLogin() && LoginTrafficManager.mapWaitLoginUserList.Count > 0)
			{
				KeyValuePair<int, ReLoginUserInfo> keyValuePair = LoginTrafficManager.mapWaitLoginUserList.FirstOrDefault();
				if (keyValuePair.Value.iServerNum == ServerStatus.MyAgentID && ClientConnection.CurrentAccounts.TryGetValue(keyValuePair.Key, out var value))
				{
					LoginTrafficManager.LoginProcess(value.Connection, 1);
				}
				LoginTrafficManager.mapWaitLoginUserList.TryRemove(keyValuePair.Key, out var _);
				LoginTrafficManager.mapWaitLoginUserIDList.TryRemove(keyValuePair.Key, out var _);
			}
			if (num - tLastNoticeLoginWaitTime >= 3000)
			{
				tLastNoticeLoginWaitTime = num;
				int num2 = 1;
				foreach (KeyValuePair<int, ReLoginUserInfo> mapWaitLoginUser in LoginTrafficManager.mapWaitLoginUserList)
				{
					if (mapWaitLoginUser.Value.iServerNum == ServerStatus.MyAgentID && ClientConnection.CurrentAccounts.TryGetValue(mapWaitLoginUser.Key, out var value4))
					{
						if (value4.isLogin && !value4.isWaitLogin)
						{
							continue;
						}
						int loginWaitUserNum = LoginTrafficManager.getLoginWaitUserNum();
						value4.Connection.SendAsync(new ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK(num2, loginWaitUserNum, 1));
					}
					num2++;
				}
			}
			long result;
			while (LoginTrafficManager.queueLoginTimeList.Count > 0 && LoginTrafficManager.queueLoginTimeList.First() < num - 60000)
			{
				LoginTrafficManager.queueLoginTimeList.TryDequeue(out result);
			}
			while (LoginTrafficManager.queueLogoutTimeList.Count > 0 && LoginTrafficManager.queueLogoutTimeList.First() < num - 60000)
			{
				LoginTrafficManager.queueLogoutTimeList.TryDequeue(out result);
			}
			return true;
		}
	}
}
