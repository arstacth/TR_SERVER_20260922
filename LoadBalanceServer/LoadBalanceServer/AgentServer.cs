using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using NetMsg.LBS;
using Serilog;

namespace LoadBalanceServer
{
	public class AgentServer : ReceiveActor
	{
		private readonly HashSet<IActorRef> _agentClients = new HashSet<IActorRef>();

		public AgentServer()
		{
			Receive(delegate(AgentToLBSRequest cr)
			{
				if (Conf.AgentPortList.TryAdd(base.Sender, cr.Port))
				{
					Conf.AgentClientBalance.TryAdd(base.Sender, 0);
					_agentClients.Add(base.Sender);
					Log.Information("{0} AgentServer Connected!", _agentClients.Count);
					base.Sender.Tell(new AgentToLBSResponse
					{
						Port = cr.Port
					});
				}
			});
			Receive(delegate(AccountCheckRequest np)
			{
				foreach (IActorRef agentClient in _agentClients)
				{
					agentClient.Tell(np);
				}
			});
			Receive(delegate(AccountCheckResponse np)
			{
				foreach (IActorRef agentClient2 in _agentClients)
				{
					agentClient2.Tell(np);
				}
			});
			Receive(delegate(ReloadSetting re)
			{
				foreach (IActorRef agentClient3 in _agentClients)
				{
					agentClient3.Tell(re);
				}
			});
			Receive(delegate(CapsuleMachineItemUpdate re)
			{
				foreach (IActorRef agentClient4 in _agentClients)
				{
					agentClient4.Tell(re);
				}
			});
			Receive(delegate(HuMongPickBoardUpdate re)
			{
				foreach (IActorRef agentClient5 in _agentClients)
				{
					agentClient5.Tell(re);
				}
			});
			Receive(delegate(byte[] np)
			{
				foreach (IActorRef agentClient6 in _agentClients)
				{
					agentClient6.Tell(np);
				}
			});
			Receive(delegate(SetServerReady re)
			{
				foreach (IActorRef agentClient7 in _agentClients)
				{
					agentClient7.Tell(re);
				}
			});
			Receive(delegate(CanShopOperation re)
			{
				foreach (IActorRef agentClient8 in _agentClients)
				{
					agentClient8.Tell(re);
				}
			});
			Receive(delegate(ReloadHash re)
			{
				foreach (IActorRef agentClient9 in _agentClients)
				{
					agentClient9.Tell(re);
				}
				Server.HashList.Clear();
				Server.HashList = re.Hash.ToList();
				Log.Information("Reload Hash Count: {0}", Server.HashList.Count);
			});
			Receive(delegate(OnlineUserUpdate re)
			{
				Conf.AgentClientBalance.AddOrUpdate(base.Sender, re.OnlineCount, delegate(IActorRef k, int v)
				{
					v = re.OnlineCount;
					return v;
				});
				Conf.AgentClientTotalLoginCount.AddOrUpdate(base.Sender, re.LoginedCount, delegate(IActorRef k, int v)
				{
					v = re.LoginedCount;
					return v;
				});
				foreach (IActorRef agentClient10 in _agentClients)
				{
					agentClient10.Tell(new TotalLoginUser
					{
						Count = Conf.AgentClientTotalLoginCount.Sum((KeyValuePair<IActorRef, int> s) => s.Value)
					});
				}
			});
			Receive(delegate(WaitLoginUserInfo re)
			{
				foreach (IActorRef agentClient11 in _agentClients)
				{
					agentClient11.Tell(re);
				}
			});
			Receive(delegate(DelWaitLoginUserInfo re)
			{
				foreach (IActorRef agentClient12 in _agentClients)
				{
					agentClient12.Tell(re);
				}
			});
			Receive(delegate(Guild_ProcessJoinRequest re)
			{
				foreach (IActorRef agentClient13 in _agentClients)
				{
					agentClient13.Tell(re);
				}
			});
			Receive(delegate(Guild_ProcessLeave re)
			{
				foreach (IActorRef agentClient14 in _agentClients)
				{
					agentClient14.Tell(re);
				}
			});
			Receive(delegate(Guild_LVUP re)
			{
				foreach (IActorRef agentClient15 in _agentClients)
				{
					agentClient15.Tell(re);
				}
			});
			Receive(delegate(Guild_Skill re)
			{
				foreach (IActorRef agentClient16 in _agentClients)
				{
					agentClient16.Tell(re);
				}
			});
			Receive(delegate(DisconnectUser re)
			{
				foreach (IActorRef agentClient17 in _agentClients)
				{
					agentClient17.Tell(re);
				}
			});
			Receive(delegate(HTLoad re)
			{
				foreach (IActorRef agentClient18 in _agentClients)
				{
					agentClient18.Tell(re);
				}
			});
			Receive(delegate(ParkDivinationCouple re)
			{
				foreach (IActorRef agentClient19 in _agentClients)
				{
					agentClient19.Tell(re);
				}
			});
			Receive(delegate(ThankOfferingScheduleReload re)
			{
				foreach (IActorRef agentClient20 in _agentClients)
				{
					agentClient20.Tell(re);
				}
			});
			Receive(delegate(ThankOfferingScheduleNext re)
			{
				foreach (IActorRef agentClient21 in _agentClients)
				{
					agentClient21.Tell(re);
				}
			});
			Receive(delegate(CombinationShopExchange re)
			{
				foreach (IActorRef agentClient22 in _agentClients)
				{
					agentClient22.Tell(re);
				}
			});
			Receive(delegate(WeddingDivorce re)
			{
				foreach (IActorRef agentClient23 in _agentClients)
				{
					agentClient23.Tell(re);
				}
			});
		}
	}
}
