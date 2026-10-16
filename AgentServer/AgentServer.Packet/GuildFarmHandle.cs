using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Room;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class GuildFarmHandle
	{
		public static void Handle_EnterGuildFarm(ClientConnection Client, PacketReader packetReader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int FarmUniqueNum = packetReader.ReadLEInt32();
			if (!Rooms.RoomList.Values.Any((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum))
			{
				RoomHolder.RoomKindInfos.TryGetValue(76, out var _);
				RoomSettings roomsetting = new RoomSettings
				{
					Name = "GuildFarmName",
					Password = string.Empty,
					IsTeamPlay = 0,
					ItemType = 0,
					IsStepOn = false,
					MapNum = FarmHandle.FarmMapNum(currentAccount.MyFarmInfo),
					RoomKindID = 76,
					FarmIndex = FarmUniqueNum
				};
				ServerStatus.ToRoomServer(new RM_CreateFarmRoom_PlayerInfo(currentAccount, roomsetting, FarmUniqueNum, last), 0);
			}
			else
			{
				NormalRoom normalRoom = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum).FirstOrDefault();
				ServerStatus.ToRoomServer(new RM_PlayerEnterRoom(currentAccount, string.Empty, normalRoom.ID, normalRoom.RoomKindID, FarmUniqueNum, 0, last), normalRoom.RoomServerID);
			}
		}

		public static void Handle_GetGuildFarmInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (GuildHandle.GetGuildFarmInfo(reader.ReadLEInt32(), currentAccount.UserNum, out var farminfo))
			{
				Client.SendAsync(new GuildFarmInfoACK(farminfo, last));
			}
		}

		public static void Handle_GetGuildFarmObjectAttr(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int guildNum = reader.ReadLEInt32();
			int attr = reader.ReadLEInt32();
			if (GetGuildFarmObjectAttr(guildNum, attr, out var farmitemattr))
			{
				Client.SendAsync(new GuildFarmItemAttr_Ack(guildNum, farmitemattr, last));
			}
		}

		public static void Handle_ModifyGuildFarmNoticeBoardInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int guildNum = reader.ReadLEInt32();
			int num = reader.ReadLEInt16();
			string notice = reader.ReadBig5StringSafe(num);
			bool flag = true;
			if (num > 100)
			{
				flag = false;
			}
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (flag && ModifyGuildFarmNoticeBoardInfo(guildNum, notice))
			{
				Client.SendAsync(new GuildFarmNoticeBoard_Ack(guildNum, notice, last));
			}
			if (GetGuildFarmObjectAttr(guildNum, -1, out var farmitemattr))
			{
				byte[] packet = new GuildFarmItemAttr_Ack(guildNum, farmitemattr, last).ToArray();
				ServerStatus.ToRoomServer(new AG_TO_RM_TO_User(currentAccount.Session, room.ID, packet), room.RoomServerID);
			}
		}

		public static void Handle_GetGuildFarmItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			reader.ReadLEInt32();
			reader.ReadLEInt64();
		}

		private static bool GetGuildFarmObjectAttr(int GuildNum, int Attr, out List<FarmItemAttr> farmitemattr)
		{
			farmitemattr = new List<FarmItemAttr>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_GetGuildFarmObjectAttr";
					mySqlCommand.Parameters.Add("pGuildNum", MySqlDbType.Int32).Value = GuildNum;
					mySqlCommand.Parameters.Add("pAttr", MySqlDbType.Int32).Value = Attr;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							FarmItemAttr item = new FarmItemAttr
							{
								AttrType = Convert.ToInt32(mySqlDataReader["AttrType"]),
								AttrValueNumber = Convert.ToSingle(mySqlDataReader["AttrValueNumber"]),
								AttrValueString = mySqlDataReader["AttrValueString"].ToString()
							};
							farmitemattr.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetGuildFarmObjectAttr error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ModifyGuildFarmNoticeBoardInfo(int GuildNum, string Notice)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ModifyGuildFarmNoticeBoardInfo";
					mySqlCommand.Parameters.Add("pGuildNum", MySqlDbType.Int32).Value = GuildNum;
					mySqlCommand.Parameters.Add("pBoardInfo", MySqlDbType.VarString).Value = Notice;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ModifyGuildFarmNoticeBoardInfo error: {0}", ex.Message);
				return false;
			}
		}
	}
}
