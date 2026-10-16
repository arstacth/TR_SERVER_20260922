using System.Collections.Generic;
using System.Linq;
using AgentServer.Packet.Send;
using LocalCommons.Network;

namespace AgentServer.Structuring
{
	public class Party
	{
		public int ID;

		public int Type = 11;

		public int LevelLimit;

		public string Title = string.Empty;

		public int MaxPlayersCount = 8;

		public List<Account> Players = new List<Account>();

		public Dictionary<string, int> JoinRequestList = new Dictionary<string, int>();

		public string LeaderNickName = string.Empty;

		public int LeaderUserNum;

		public int LeaderLevel;

		public int Status;

		private readonly object playerLock = new object();

		private readonly object joinLock = new object();

		public int PlayerCount()
		{
			return Players.Count;
		}

		public List<Account> PlayerList()
		{
			lock (playerLock)
			{
				return new List<Account>(Players);
			}
		}

		public void setID(int id)
		{
			ID = id;
		}

		public void SetLeader(Account newLeader)
		{
			LeaderNickName = newLeader.NickName;
			LeaderUserNum = newLeader.UserNum;
			LeaderLevel = newLeader.Level;
			JoinRequestList.Clear();
		}

		public void BroadcastToAll(NetPacket np)
		{
			foreach (Account item in PlayerList())
			{
				item.Connection.SendAsync(np);
			}
		}

		public void JoinParty(Account User)
		{
			lock (joinLock)
			{
				if (Players.Count < MaxPlayersCount)
				{
					User.CurrentPartyID = ID;
					User.Connection.SendAsync(new PartyUserList_Ack(this, 1));
					Players.Add(User);
					BroadcastToAll(new PartyNewUser_Ack(User, 1));
					if (Players.Count >= MaxPlayersCount)
					{
						Status = 3;
						JoinRequestList.Clear();
					}
				}
			}
		}

		public void LeaveParty(Account User, int type, byte last)
		{
			lock (joinLock)
			{
				if (User.CurrentPartyID == 0)
				{
					return;
				}
				User.CurrentPartyID = 0;
				User.Connection.SendAsync(new PartyLeaveOK_Ack(type, last));
				Players.Remove(User);
				BroadcastToAll(new PartyLeaveUser_Ack(User.NickName, type, last));
				if (Players.Count < 2)
				{
					BroadcastToAll(new PartyLeaveOK_Ack(2, last));
					foreach (Account player in Players)
					{
						player.CurrentPartyID = 0;
					}
					Partys.RemoveParty(ID);
				}
				else if (LeaderUserNum == User.UserNum)
				{
					Status = 3;
					Account account = Players.FirstOrDefault();
					SetLeader(account);
					BroadcastToAll(new PartyChangeLeader_Ack(account.NickName, last));
				}
			}
		}
	}
}
