using System;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using Akka.Actor;

namespace AgentServer.Holders
{
	public class ShopManager : ActorBase
	{
		protected override bool Receive(object message)
		{
			switch (Convert.ToByte(message))
			{
			case 1:
				ServerStatus.ServerActor.Tell(new ShopBuyCountResetNotify_ACK());
				ServerStatus.ServerActor.Tell(new GuildMissionUserMissionDeleteNotify_ACK(0, 1));
				if (ServerStatus.MyAgentID == 1)
				{
					MissionHolder.Mission_guildMission_Init();
				}
				break;
			case 2:
				if (DateTime.Now.Day == 1)
				{
					ServerStatus.ServerActor.Tell(new ShopVIPLevelResetNotify());
				}
				break;
			}
			return true;
		}
	}
}
