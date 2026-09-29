using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;

namespace AgentServer.Packet
{
	public static class PartyHandle
	{
		public static void Handle_PartyInvite(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string invitedname = reader.ReadBig5StringSafe(fixedLength);
			Party party;
			bool party2 = Partys.GetParty(currentAccount.CurrentPartyID, out party);
			if (party2 && party.Players.Count >= party.MaxPlayersCount)
			{
				Client.SendAsync(new PartyInviteFail_Ack(7, invitedname, last));
				return;
			}
			Account account = ClientConnection.CurrentAccounts.Values.FirstOrDefault((Account a) => a.NickName == invitedname && a.isLogin && !a.isDisconnected);
			if (account != null)
			{
				if ((account.GameOption & 4) == 4 && (!party2 || !party.JoinRequestList.ContainsKey(invitedname)))
				{
					Client.SendAsync(new PartyInviteFail_Ack(6, invitedname, last));
					return;
				}
				Party party3;
				bool party4 = Partys.GetParty(account.CurrentPartyID, out party3);
				if (account.InGame && Rooms.GetRoom(account.CurrentRoomId).isPlaying)
				{
					Client.SendAsync(new PartyInviteFail_Ack(4, invitedname, last));
				}
				else if (!party4 || party3.Status == 1)
				{
					account.Connection.SendAsync(new PartyInvite_Ack(currentAccount.NickName, last));
				}
				else if (party4 && party3.Status > 1)
				{
					if (party3.JoinRequestList.ContainsKey(invitedname))
					{
						party3.JoinRequestList.Remove(invitedname);
					}
					Client.SendAsync(new PartyInviteFail_Ack(5, invitedname, last));
				}
			}
			else
			{
				Client.SendAsync(new PartyInviteFail_Ack(3, invitedname, last));
			}
		}

		public static void Handle_AcceptPartyInvite(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string invitename = reader.ReadBig5StringSafe(fixedLength);
			Party party;
			bool party2 = Partys.GetParty(currentAccount.CurrentPartyID, out party);
			if (party2 && party.Status == 1)
			{
				Partys.RemoveParty(currentAccount.CurrentPartyID);
				currentAccount.CurrentPartyID = 0;
				party = null;
			}
			else if (party2 && party.Status >= 2)
			{
				return;
			}
			Account account = ClientConnection.CurrentAccounts.Values.FirstOrDefault((Account a) => a.NickName == invitename && a.isLogin && !a.isDisconnected);
			if (account != null)
			{
				Party party3;
				bool party4 = Partys.GetParty(account.CurrentPartyID, out party3);
				if (!party4)
				{
					Random random = new Random();
					int num = random.Next(255);
					num += random.Next(255) << 8;
					num += random.Next(255) << 16;
					num += random.Next(255) << 24;
					Party party5 = new Party();
					party5.setID(num);
					party5.Status = 3;
					party5.Players.Add(account);
					party5.SetLeader(account);
					account.CurrentPartyID = num;
					account.Connection.SendAsync(new AcceptPartyInvite_Ack1(account.NickName, last));
					account.Connection.SendAsync(new PartyNewUser_Ack(account, last));
					account.Connection.SendAsync(new PartyAccept_Ack4(last));
					party5.JoinParty(currentAccount);
					Partys.AddParty(num, party5);
				}
				else if (party4 && party3.Status == 1)
				{
					if (party3.JoinRequestList.ContainsKey(currentAccount.NickName))
					{
						party3.JoinRequestList.Remove(currentAccount.NickName);
					}
					party3.Status = 2;
					account.CurrentPartyID = party3.ID;
					account.Connection.SendAsync(new AcceptPartyInvite_Ack1(account.NickName, last));
					account.Connection.SendAsync(new PartyNewUser_Ack(account, last));
					account.Connection.SendAsync(new PartyAccept_Ack4(last));
					party3.JoinParty(currentAccount);
				}
				else if (party4 && party3.Players.Count < party3.MaxPlayersCount && (party3.Status == 2 || party3.Status == 3))
				{
					if (party3.JoinRequestList.ContainsKey(currentAccount.NickName))
					{
						party3.JoinRequestList.Remove(currentAccount.NickName);
					}
					account.Connection.SendAsync(new PartyAccept_Ack4(last));
					party3.JoinParty(currentAccount);
				}
				else if (party4 && party3.Players.Count >= party3.MaxPlayersCount)
				{
					Client.SendAsync(new AcceptPartyInviteFail_Ack(7, last));
				}
			}
			else
			{
				Client.SendAsync(new AcceptPartyInviteFail_Ack(20, last));
			}
		}

		public static void Handle_PartyLeave(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party))
			{
				party.LeaveParty(currentAccount, 0, last);
			}
		}

		public static void Handle_PartyKickUser(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party) && party.LeaderUserNum == currentAccount.UserNum)
			{
				Account user = party.Players.FirstOrDefault((Account p) => p.NickName == nickname);
				party.LeaveParty(user, 1, last);
			}
		}

		public static void Handle_PartyChangeLeader(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party) && party.LeaderUserNum == currentAccount.UserNum)
			{
				Account account = party.Players.FirstOrDefault((Account p) => p.NickName == nickname);
				party.Status = 3;
				party.SetLeader(account);
				party.BroadcastToAll(new PartyChangeLeader_Ack(account.NickName, last));
			}
		}

		public static void Handle_PartyRecruit(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int type = reader.ReadLEInt32();
			int levelLimit = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			int fixedLength2 = reader.ReadLEInt16();
			_ = string.Empty;
			reader.ReadBig5StringSafe(fixedLength2);
			int code = 0;
			Party party;
			bool party2 = Partys.GetParty(currentAccount.CurrentPartyID, out party);
			if (!party2)
			{
				Random random = new Random();
				int num = random.Next(255);
				num += random.Next(255) << 8;
				num += random.Next(255) << 16;
				num += random.Next(255) << 24;
				Party party3 = new Party();
				party3.setID(num);
				party3.Status = 1;
				party3.Type = type;
				party3.LevelLimit = levelLimit;
				party3.Title = empty;
				party3.Players.Add(currentAccount);
				party3.SetLeader(currentAccount);
				currentAccount.CurrentPartyID = num;
				Partys.AddParty(num, party3);
			}
			else if (party2 && party.Players.Count < party.MaxPlayersCount && party.Status > 2)
			{
				party.Status = 2;
				party.Type = type;
				party.LevelLimit = levelLimit;
				party.Title = empty;
			}
			else if (party2 && party.Players.Count >= party.MaxPlayersCount)
			{
				code = 9;
			}
			else if (party2 && party.Status <= 2)
			{
				code = 26;
			}
			Client.SendAsync(new PartyRecruit_Ack(code, last));
		}

		public static void Handle_PartyRecruitCancel(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			Party party;
			bool party2 = Partys.GetParty(currentAccount.CurrentPartyID, out party);
			if (party2 && party.Status == 1)
			{
				Partys.RemoveParty(currentAccount.CurrentPartyID);
				currentAccount.CurrentPartyID = 0;
				party = null;
			}
			else if (party2 && party.Status == 2)
			{
				party.JoinRequestList.Clear();
				party.Status = 3;
			}
			Client.SendAsync(new PartyRecruitCancelOK_Ack(last));
		}

		public static void Handle_GetPartyIndex(ClientConnection Client, PacketReader reader, byte last)
		{
			reader.ReadLEInt32();
			int type = reader.ReadLEInt32();
			int isAllLevel = reader.ReadLEInt32();
			int levelilmit = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string leadername = string.Empty;
			leadername = reader.ReadBig5StringSafe(fixedLength);
			List<Party> list = Partys.PartyList.Values.Where((Party w) => w.Type == type && ((isAllLevel == 0) ? (w.LevelLimit == levelilmit) : (w.LevelLimit >= levelilmit)) && w.LeaderNickName.Contains(leadername) && w.Status <= 2).Skip((num2 - 1) * num).Take(num)
				.ToList();
			int num3 = Convert.ToInt32(Math.Ceiling((double)list.Count / Convert.ToDouble(num)));
			if (num3 == 0)
			{
				num3 = 1;
				num2 = 1;
			}
			Client.SendAsync(new PartyList_Ack(list, num3, num2, last));
		}

		public static void Handle_GetPartyUserList(ClientConnection Client, PacketReader reader, byte last)
		{
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string leadername = string.Empty;
			leadername = reader.ReadBig5StringSafe(fixedLength);
			Party party = Partys.PartyList.Values.FirstOrDefault((Party w) => w.LeaderNickName == leadername && w.Status <= 2);
			if (party != null)
			{
				Client.SendAsync(new GetPartyUserList_Ack(party, last));
			}
		}

		public static void Handle_PartyJoinRequest(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string leadername = string.Empty;
			leadername = reader.ReadBig5StringSafe(fixedLength);
			if (leadername == currentAccount.NickName)
			{
				Client.SendAsync(new PartyJoinRequestFail_Ack(22, last));
				return;
			}
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party) && party.Status >= 2)
			{
				Client.SendAsync(new PartyJoinRequestFail_Ack(21, last));
				return;
			}
			Account account = ClientConnection.CurrentAccounts.Values.FirstOrDefault((Account a) => a.NickName == leadername && a.isLogin && !a.isDisconnected);
			if (account != null)
			{
				Party party2;
				bool party3 = Partys.GetParty(account.CurrentPartyID, out party2);
				if (party3 && party2.Players.Count < party2.MaxPlayersCount && party2.Status <= 2)
				{
					if (!party2.JoinRequestList.ContainsKey(currentAccount.NickName))
					{
						party2.JoinRequestList.Add(currentAccount.NickName, currentAccount.Level);
						Client.SendAsync(new PartyJoinRequest_Ack(leadername, last));
					}
					else
					{
						Client.SendAsync(new PartyJoinRequestFail_Ack(19, last));
					}
				}
				else if (party3 && party2.Players.Count >= party2.MaxPlayersCount && party2.Status <= 2)
				{
					Client.SendAsync(new PartyJoinRequestFail_Ack(16, last));
				}
				else if (party3 && party2.Status > 2)
				{
					Client.SendAsync(new PartyJoinRequestFail_Ack(18, last));
				}
			}
			else
			{
				Client.SendAsync(new PartyJoinRequestFail_Ack(14, last));
			}
		}

		public static void Handle_PartyJoinRequestReject(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party) && party.JoinRequestList.ContainsKey(empty))
			{
				party.JoinRequestList.Remove(empty);
				Client.SendAsync(new PartyJoinRequestReject_Ack(empty, last));
			}
		}

		public static void Handle_GetPartyJoinRequestList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int page = reader.ReadLEInt32();
			int getnum = reader.ReadLEInt32();
			if (Partys.GetParty(currentAccount.CurrentPartyID, out var party))
			{
				Client.SendAsync(new GetPartyJoinRequestList_Ack(party, page, getnum, last));
			}
		}
	}
}
