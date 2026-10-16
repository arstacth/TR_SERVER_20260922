using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Function;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Room;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class FarmHandle
	{
		public static void Handle_EnterFarm(ClientConnection Client, PacketReader packetReader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int FarmUniqueNum = Utility.NormalizeFarmUnique(packetReader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
			int num = packetReader.ReadLEInt32();
			string text = string.Empty;
			if (num > 0)
			{
				text = packetReader.ReadBig5StringSafe(num);
			}
			try
			{
				if (currentAccount.MyFarmInfo != null)
				{
					currentAccount.MyFarmInfo.farmExp = EnsureFarmExp(currentAccount.UserNum, currentAccount.MyFarmInfo.farmExp);
				}
				if (!Rooms.RoomList.Values.Any((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum))
				{
					RoomSettings roomsetting = new RoomSettings
					{
						Name = "FarmName",
						Password = text,
						IsTeamPlay = 0,
						ItemType = 0,
						IsStepOn = false,
						MapNum = FarmMapNum(currentAccount.MyFarmInfo),
						RoomKindID = 75,
						FarmIndex = FarmUniqueNum
					};
					ServerStatus.ToRoomServer(new RM_CreateFarmRoom_PlayerInfo(currentAccount, roomsetting, FarmUniqueNum, last), 0);
				}
				else
				{
					NormalRoom normalRoom = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum).FirstOrDefault();
					ServerStatus.ToRoomServer(new RM_PlayerEnterRoom(currentAccount, text, normalRoom.ID, normalRoom.RoomKindID, FarmUniqueNum, 0, last), normalRoom.RoomServerID);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Handle_EnterFarm: {0}", ex);
			}
		}

		public static void Handle_CreatePublicFarm(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int FarmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
			if (FarmUniqueNum == currentAccount.MyFarmUniqueNum)
			{
				byte type = reader.ReadByte();
				byte maxUserLimit = reader.ReadByte();
				byte unk = reader.ReadByte();
				int num = reader.ReadLEInt16();
				string empty = string.Empty;
				if (num > 3 && num <= 32)
				{
					empty = reader.ReadBig5StringSafe(num);
					if (!Rooms.RoomList.Values.Any((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum))
					{
						FarmRoomInfo farmRoomInfo = new FarmRoomInfo
						{
							isPublic = true,
							Type = type,
							MaxUserLimit = maxUserLimit,
							unk1 = unk,
							FarmName = empty
						};
						RoomSettings roomsetting = new RoomSettings
						{
							Name = "FarmName",
							Password = string.Empty,
							IsTeamPlay = 0,
							ItemType = 0,
							IsStepOn = false,
							MapNum = FarmMapNum(currentAccount.MyFarmInfo),
							RoomKindID = 75,
							FarmIndex = FarmUniqueNum,
							FarmRoomInfo = farmRoomInfo
						};
						ServerStatus.ToRoomServer(new RM_CreatePublicFarmRoom_PlayerInfo(currentAccount, roomsetting, FarmUniqueNum, last), 0);
					}
					else
					{
						NormalRoom normalRoom = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum).FirstOrDefault();
						ServerStatus.ToRoomServer(new RM_PlayerEnterRoom(currentAccount, string.Empty, normalRoom.ID, normalRoom.RoomKindID, FarmUniqueNum, 0, last), normalRoom.RoomServerID);
					}
					return;
				}
			}
			Client.SendAsync(new CreatePublicFarmFail(last));
		}

		public static void Handle_GetPublicFarmList(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int page = reader.ReadLEInt32();
			byte getcount = reader.ReadByte();
			byte GetType = 7;
			if (reader.Remaining > 0)
			{
				GetType = reader.ReadByte();
			}
			if (getcount == 0)
			{
				getcount = 14;
			}
			// Thai All-tab is 8 (KR 7). Piero 0x0E often has no type byte.
			bool allTypes = GetType == 7 || GetType == 8 || GetType == 16;
			List<NormalRoom> publicfarmlist = (allTypes ? Rooms.PublicFarmRoom.Values.Where((NormalRoom w) => !w.HasPassword).ToList() : Rooms.PublicFarmRoom.Values.Where((NormalRoom w) => !w.HasPassword && w.ChatFarmType == GetType).ToList());
			Client.SendAsync(new PublicFarmList(publicfarmlist, page, getcount, last));
		}

		public static void Handle_GetFarmPoint(ClientConnection Client, byte last)
		{
			GetFarmPoint(Client.CurrentAccount.UserNum, out var farmpoint);
			Client.SendAsync(new FarmPoint(farmpoint, last));
		}

		public static void Handle_GetMyFarmItem(ClientConnection Client, PacketReader reader, byte last)
		{
			GetFarmItemList(Client.CurrentAccount.UserNum, null, out var farmitemlist);
			Client.SendAsync(new GetMyFarmItemResponse(last));
			foreach (List<FarmItem> item in farmitemlist.Split(2041))
			{
				Client.SendAsync(new MyFarmItemList(item, last));
			}
			Client.SendAsync(new MyFarmItemCount(farmitemlist.Count, last));
		}

		public static void Handle_GetFarmItemAttr(ClientConnection Client, PacketReader reader, byte last)
		{
			int wireUnique = reader.ReadLEInt32();
			byte getOnlySetUpObjectInfo = reader.ReadByte();
			int itemNum = reader.ReadLEInt32();
			long oID = reader.ReadLEInt64();
			int farmUniqueNum = Utility.NormalizeFarmUnique(wireUnique, Client.CurrentAccount != null ? Client.CurrentAccount.MyFarmUniqueNum : 0);
			GetFarmItemAttr(farmUniqueNum, getOnlySetUpObjectInfo, itemNum, oID, out var farmitemattr);
			int ackUnique = Utility.PackFarmUnique(wireUnique > 0 ? wireUnique : farmUniqueNum);
			Client.SendAsync(new FarmItemAttr_Ack(ackUnique, farmitemattr, last));
			Log.Information("FarmItemAttr farm={0} wire={1} attrs={2}", farmUniqueNum, wireUnique, farmitemattr.Count);
		}

		public static void Handle_GetFarmItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			string text = string.Empty;
			if (num > 0 && num < 20000)
			{
				for (int i = 0; i < num; i++)
				{
					if (reader.Remaining < 4)
					{
						break;
					}
					int num2 = reader.ReadLEInt32();
					text += $"{num2},";
				}
			}
			GetFarmItemList(currentAccount.UserNum, string.IsNullOrEmpty(text) ? null : text, out var farmitemlist);
			if (farmitemlist.Count == 0)
			{
				Client.SendAsync(new FarmItemList(farmitemlist, last));
				return;
			}
			foreach (List<FarmItem> item in farmitemlist.Split(2041))
			{
				Client.SendAsync(new FarmItemList(item, last));
			}
		}

		public static void Handle_ReloadFarmMapInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (!currentAccount.InGame)
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (room == null)
			{
				return;
			}
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
			ServerStatus.ToRoomServer(new RM_ReloadFarmMapInfo(currentAccount, farmUniqueNum, last), room.RoomServerID);
		}

		public static void Handle_ClearUserFarmMapInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
				ServerStatus.ToRoomServer(new RM_ClearUserFarmMapInfo(currentAccount, farmUniqueNum, last), room.RoomServerID);
			}
		}

		public static void Handle_ModifyFarmMapInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (room == null || !currentAccount.InGame)
			{
				return;
			}
			int mine = Utility.NormalizeFarmUnique(currentAccount.MyFarmUniqueNum);
			int idx = Utility.NormalizeFarmUnique(room.FarmIndex);
			if (mine != idx && mine != 0)
			{
				return;
			}
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
			int farmTypeNum = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			byte[] data = reader.ReadByteArray(32 * num);
			ServerStatus.ToRoomServer(new RM_ModifyFarmMapInfo(currentAccount, farmUniqueNum, farmTypeNum, num, data, last), room.RoomServerID);
		}

		public static void Handle_SearchFarm(ClientConnection Client, PacketReader reader, byte last)
		{
			int fixedLength = reader.ReadLEInt16();
			_ = string.Empty;
			if (SearchFarm(reader.ReadBig5StringSafe(fixedLength), out var FoundFarmUnqueNum, out var PW))
			{
				if (FoundFarmUnqueNum != -1)
				{
					Client.SendAsync(new SearchFarmOK(FoundFarmUnqueNum, PW == string.Empty, last));
				}
				else
				{
					Client.SendAsync(new SearchFarmFail(last));
				}
			}
			else
			{
				Client.SendAsync(new SearchFarmFail(last));
			}
		}

		public static void Handle_SearchFarmByUserNum(ClientConnection Client, PacketReader reader, byte last)
		{
			int userNum = reader.ReadLEInt32();
			if (SearchFarmByUserNum("1", userNum, out var FoundFarmUnqueNum, out var PW))
			{
				if (FoundFarmUnqueNum != -1)
				{
					Client.SendAsync(new SearchFarmByUserNumOK(FoundFarmUnqueNum, PW == string.Empty, last));
				}
				else
				{
					Client.SendAsync(new SearchFarmFail(last));
				}
			}
			else
			{
				Client.SendAsync(new SearchFarmFail(last));
			}
		}

		public static void Handle_JoinFarmRoom(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			int roomsession = reader.ReadLEInt32();
			int num = reader.ReadLEInt16();
			string pw = string.Empty;
			if (num > 0)
			{
				pw = reader.ReadBig5StringSafe(num);
			}
			if (!Rooms.ExistRoom(roomsession))
			{
				Client.SendAsync(new GameRoom_EnterRoomError(1, 75, last));
				return;
			}
			NormalRoom room = Rooms.GetRoom(roomsession);
			ServerStatus.ToRoomServer(new RM_PlayerEnterRoom(currentAccount, pw, room.ID, room.RoomKindID, room.FarmIndex, 0, last), room.RoomServerID);
		}

		public static void Handle_IncreaseAnimalSize(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemNum = reader.ReadLEInt32();
				int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
				long farmItemID = reader.ReadLEInt64();
				ServerStatus.ToRoomServer(new RM_IncreaseAnimalSize(currentAccount, farmUniqueNum, itemNum, farmItemID, last), room.RoomServerID);
			}
		}

		public static void Handle_RestoreAnimalDefaultSize(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemNum = reader.ReadLEInt32();
				int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32(), currentAccount.MyFarmUniqueNum);
				long farmItemID = reader.ReadLEInt64();
				ServerStatus.ToRoomServer(new RM_RestoreAnimalDefaultSize(currentAccount, farmUniqueNum, itemNum, farmItemID, last), room.RoomServerID);
			}
		}

		public static void Handle_ExpiredFarmItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int farm = 0;
			int sky = 0;
			if (currentAccount != null)
			{
				farm = Utility.NormalizeFarmUnique(currentAccount.MyFarmUniqueNum);
				sky = ChangeFarmSkyboxByItem(currentAccount.UserNum, farm, itemNum);
				if (currentAccount.MyFarmInfo != null)
				{
					currentAccount.MyFarmInfo.FarmSkyTypeNum = sky;
				}
			}
			Client.SendAsync(new ExpiredFarmItem_Ack(farm, sky, last));
		}

		public static void Handle_ChangeFarmSkybox(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemNum = reader.ReadLEInt32();
				ServerStatus.ToRoomServer(new RM_ChangeFarmSkybox(currentAccount, itemNum, last), room.RoomServerID);
			}
		}

		public static void Handle_ChangeFarmWeather(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int farm = 0;
			int weather = 0;
			if (currentAccount != null)
			{
				farm = Utility.NormalizeFarmUnique(currentAccount.MyFarmUniqueNum);
				weather = ChangeFarmWeatherByItem(currentAccount.UserNum, farm, itemNum);
				if (currentAccount.MyFarmInfo != null)
				{
					currentAccount.MyFarmInfo.FarmWeatherTypeNum = weather;
				}
			}
			Client.SendAsync(new ChangeFarmWeather_Ack(farm, weather, last));
		}

		private static int ChangeFarmWeatherByItem(int userNum, int farmUniqueNum, int itemDescNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ChangeFarmWeatherByItem";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = farmUniqueNum;
					mySqlCommand.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = itemDescNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						return Convert.ToInt32(mySqlDataReader["NewWeatherType"]);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmWeatherByItem error: {0}", ex.Message);
			}
			return 0;
		}

		private static int ChangeFarmSkyboxByItem(int userNum, int farmUniqueNum, int itemDescNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ChangeFarmSkyboxByItem";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = farmUniqueNum;
					mySqlCommand.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = itemDescNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						return Convert.ToInt32(mySqlDataReader["NewSkyboxType"]);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmSkyboxByItem error: {0}", ex.Message);
			}
			return 0;
		}

		public static void Handle_ChangeFarmType(ClientConnection Client, PacketReader reader, byte last)
		{
			if (Client.CurrentAccount.InGame)
			{
				reader.ReadLEInt32();
			}
		}

		public static void Handle_SetFarmPortalInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				long num = reader.ReadLEInt64();
				int num2 = reader.ReadLEInt16();
				string nickname = string.Empty;
				if (num2 > 4)
				{
					nickname = reader.ReadBig5StringSafe(num2);
				}
				int fixedLength = reader.ReadLEInt16();
				string memo = string.Empty;
				if (num2 > 0)
				{
					memo = reader.ReadBig5StringSafe(fixedLength);
				}
				if (SetFarmPortalInfo(currentAccount.UserNum, num, nickname, memo, out var portalFarmUniqueNum))
				{
					byte[] packet = new SetFarmPortalInfoOK_Ack(num, portalFarmUniqueNum, memo, last).ToArray();
					room.BroadcastToAll(currentAccount.Session, packet);
				}
			}
		}

		public static void Handle_GetFarmPortalInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			long num = reader.ReadLEInt64();
			if (GetFarmPortalInfo(num, out var nickname, out var memo, out var portalFarmUniqueNum))
			{
				Client.SendAsync(new GetFarmPortalInfoOK_Ack(num, portalFarmUniqueNum, nickname, memo, last));
			}
		}

		public static void Handle_ModifyObjectValueInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				long num = reader.ReadLEInt64();
				int value = reader.ReadLEInt32();
				if (ModifyObjectValueInfo(num, value))
				{
					byte[] packet = new ModifyObjectValueInfoOK(num, value, last).ToArray();
					room.BroadcastToAll(currentAccount.Session, packet);
				}
			}
		}

		public static void Handle_GetFarmSlotListInfo(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Client.SendAsync(new GetFarmSlot_Ack(currentAccount, last));
		}

		public static void Handle_ClearFarmSlotInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int slotNum = reader.ReadLEInt32();
			if (currentAccount.InGame && ClearUserFarmSlotInfo(currentAccount, slotNum))
			{
				Client.SendAsync(new ClearFarmSlot_Ack(slotNum, last));
			}
		}

		public static void Handle_ChangeFarmMapTypeBySlot(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && currentAccount.MyFarmUniqueNum == room.FarmIndex)
			{
				reader.ReadLEInt32();
				reader.ReadLEInt32();
				reader.ReadLEInt32();
				ServerStatus.ToRoomServer(new RM_ChangeFarmMapTypeBySlot(currentAccount, last), room.RoomServerID);
			}
		}

		public static void Handle_GetMasterUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			int farmnum = 0;
			if (reader != null && reader.Remaining >= 4)
			{
				farmnum = reader.ReadLEInt32();
			}
			int fallback = 0;
			if (Client.CurrentAccount != null)
			{
				fallback = Client.CurrentAccount.MyFarmUniqueNum;
			}
			farmnum = Utility.NormalizeFarmUnique(farmnum, fallback);
			bool ownFarm = fallback > 0 && fallback == farmnum;
			Log.Information("GetMasterUserInfo farm={0} own={1} user={2}", farmnum, ownFarm, Client.CurrentAccount != null ? Client.CurrentAccount.UserID : "?");
			Client.SendAsync(new FarmGetUserInfo(farmnum, last, ownFarm));
		}

		public static void Handle_FFD10136(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Rooms.GetRoom(currentAccount.CurrentRoomId).BroadcastToAll(packet: new UnknownFarmPacket1(last).ToArray(), Session: currentAccount.Session);
		}

		public static void Handle_FFD1017E(ClientConnection Client, byte last)
		{
			Client.SendAsync(new UnknownFarmPacket2(last));
		}

		public static void Handle_FarmExchangeItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemnum = reader.ReadLEInt32();
			int count = reader.ReadLEInt32();
			int level = currentAccount.Level;
			int rewardCondition = reader.ReadLEInt32();
			short num = reader.ReadLEInt16();
			string itemNums = string.Empty;
			if (num > 0)
			{
				itemNums = reader.ReadBig5StringSafe(num);
				itemNums += ",";
			}
			if (FarmExchangeItem(currentAccount, itemnum, count, rewardCondition, itemNums, out var exinfo))
			{
				Client.SendAsync(new FarmExchangeItem_Ack(itemnum, count, exinfo, last));
				if (LobbyHandle.LevelUPCheck(currentAccount, level))
				{
					Client.SendAsync(new UserLevelUPEXPInfo(1, currentAccount.Level, currentAccount.Exp, last));
				}
			}
			else
			{
				Client.SendAsync(new FarmExchangeItemFail_Ack(last));
			}
		}

		public static bool RequestJoinFarmRoom(Account User, int FarmUniqueNum, int pGuildFarm, out FarmRoomInfo roominfo)
		{
			roominfo = new FarmRoomInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_RequestJoinFarmRoom";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					mySqlCommand.Parameters.Add("pLucky", MySqlDbType.Float).Value = (float)User.Luck;
					mySqlCommand.Parameters.Add("pCheckFarmPeriod", MySqlDbType.Int32).Value = 1;
					mySqlCommand.Parameters.Add("pGuildFarm", MySqlDbType.Int32).Value = pGuildFarm;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						roominfo = new FarmRoomInfo
						{
							IsMaster = Convert.ToBoolean(mySqlDataReader["IsMaster"]),
							FarmTypeNum = Convert.ToInt32(mySqlDataReader["FarmTypeNum"]),
							FarmSkyTypeNum = 0,
							FarmWeatherTypeNum = 0,
							MaxPlayerNum = Convert.ToByte(mySqlDataReader["MaxPlayerNum"]),
							FarmName = mySqlDataReader["FarmName"].ToString(),
							MasterName = mySqlDataReader["MasterName"].ToString(),
							ExpireTime = ((mySqlDataReader["ExpireTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ExpireTime"]))),
							CreateTime = ((mySqlDataReader["CreateTime"].ToString() == "0") ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["CreateTime"]))),
							Password = mySqlDataReader["Password"].ToString(),
							TotalCount = Convert.ToInt32(mySqlDataReader["TotalCount"]),
							TodaysVisitorCount = Convert.ToInt32(mySqlDataReader["TodaysVisitorCount"]),
							PremiumFarmUsing = Convert.ToBoolean(mySqlDataReader["PremiumFarmUsing"]),
							PremiumFarmExpireDateTime = ((mySqlDataReader["PremiumFarmExpireDateTime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["PremiumFarmExpireDateTime"]))),
							farmExp = Convert.ToInt32(mySqlDataReader["farmExp"]),
							isGuildFarm = (pGuildFarm == 1)
						};
						roominfo.farmExp = EnsureFarmExp(User.UserNum, roominfo.farmExp);
						if (User.MyFarmInfo != null && User.MyFarmInfo.farmExp > roominfo.farmExp)
						{
							roominfo.farmExp = User.MyFarmInfo.farmExp;
						}
						if (User.MyFarmUniqueNum == FarmUniqueNum)
						{
							User.MyFarmInfo.FarmTypeNum = roominfo.FarmTypeNum;
							User.MyFarmInfo.FarmSkyTypeNum = roominfo.FarmSkyTypeNum;
							User.MyFarmInfo.FarmWeatherTypeNum = roominfo.FarmWeatherTypeNum;
							User.MyFarmInfo.TotalCount = roominfo.TotalCount;
							User.MyFarmInfo.TodaysVisitorCount = roominfo.TodaysVisitorCount;
							User.MyFarmInfo.PremiumFarmUsing = roominfo.PremiumFarmUsing;
							User.MyFarmInfo.PremiumFarmExpireDateTime = roominfo.PremiumFarmExpireDateTime;
							User.MyFarmInfo.farmExp = roominfo.farmExp;
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_RequestJoinFarmRoom error: {0}", ex.Message);
				return false;
			}
		}

		private static void GetFarmPoint(int UserNum, out int farmpoint)
		{
			farmpoint = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT IFNULL(fdFarmPoint,0) AS FarmPoint FROM tblUserFarmInfo WHERE fdUserNum=@u LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", UserNum);
				object obj = mySqlCommand.ExecuteScalar();
				if (obj != null && obj != DBNull.Value)
				{
					farmpoint = Convert.ToInt32(obj);
				}
			}
			catch (Exception ex)
			{
				Log.Error("GetFarmPoint error: {0}", ex.Message);
			}
		}

		public static void GetFarmMapInfo(int FarmUniqueNum, int guildNum, out List<FarmMapInfo> farmmapinfo, out int farmMapType)
		{
			farmMapType = 1;
			farmmapinfo = new List<FarmMapInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_GetFarmMapInfo";
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = guildNum;
				mySqlCommand.Parameters.Add("pFarmChattingSuject", MySqlDbType.VarString).Value = string.Empty;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				long nowTime = Utility.CurrentTimeMilliseconds();
				mySqlDataReader.Read();
				farmMapType = Convert.ToInt32(mySqlDataReader["farmMapType"]);
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					FarmMapInfo item = new FarmMapInfo
					{
						FarmItemID = Convert.ToInt64(mySqlDataReader["FarmItemID"]),
						ItemDescNum = Convert.ToInt32(mySqlDataReader["ItemDescNum"]),
						CellPosX = Convert.ToByte(mySqlDataReader["CellPosX"]),
						CellPosY = Convert.ToByte(mySqlDataReader["CellPosY"]),
						CellPosZ = Convert.ToByte(mySqlDataReader["CellPosZ"]),
						CellWidth = Convert.ToByte(mySqlDataReader["CellWidth"]),
						CellHeight = Convert.ToByte(mySqlDataReader["CellHeight"]),
						BlockPosX = Convert.ToByte(mySqlDataReader["BlockPosX"]),
						BlockPosY = Convert.ToByte(mySqlDataReader["BlockPosY"]),
						BlockWidth = Convert.ToByte(mySqlDataReader["BlockWidth"]),
						BlockHeight = Convert.ToByte(mySqlDataReader["BlockHeight"]),
						RotateType = Convert.ToByte(mySqlDataReader["RotateType"]),
						Angle = Convert.ToSingle(mySqlDataReader["Angle"]),
						Exp = Convert.ToInt32(mySqlDataReader["experience"]),
						View = Convert.ToInt32(mySqlDataReader["ViewLevel"]),
						NowTime = nowTime
					};
					farmmapinfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetFarmMapInfo error: {0}", ex.Message);
			}
		}

		private static void GetFarmItemList(int UserNum, string itemnumstr, out List<FarmItem> farmitemlist)
		{
			farmitemlist = new List<FarmItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_GetFarmItemList";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("requestFarmItemNums", MySqlDbType.VarString).Value = itemnumstr;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					FarmItem item = new FarmItem
					{
						FarmItemID = Convert.ToInt64(mySqlDataReader["FarmItemID"]),
						ItemDescNum = Convert.ToInt32(mySqlDataReader["ItemDescNum"]),
						Exp = Convert.ToInt32(mySqlDataReader["Exp"]),
						View = Convert.ToInt32(mySqlDataReader["View"]),
						ExpireDateTime = (Convert.IsDBNull(mySqlDataReader["ExpireDateTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ExpireDateTime"])))
					};
					farmitemlist.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetFarmItemList error: {0}", ex.Message);
			}
		}

		private static bool GetFarmItemAttr(int FarmUniqueNum, byte GetOnlySetUpObjectInfo, int ItemNum, long OID, out ConcurrentDictionary<long, List<FarmItemAttr>> farmitemattr)
		{
			farmitemattr = new ConcurrentDictionary<long, List<FarmItemAttr>>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_GetFarmItemAttr";
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					mySqlCommand.Parameters.Add("pGetOnlySetUpObjectInfo", MySqlDbType.Int16).Value = GetOnlySetUpObjectInfo;
					mySqlCommand.Parameters.Add("pItemNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("pOID", MySqlDbType.Int64).Value = OID;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							long key = Convert.ToInt64(mySqlDataReader["FarmItemID"]);
							FarmItemAttr attr = new FarmItemAttr
							{
								AttrType = Convert.ToInt32(mySqlDataReader["AttrType"]),
								AttrValueNumber = Convert.ToSingle(mySqlDataReader["AttrValueNumber"]),
								AttrValueString = mySqlDataReader["AttrValueString"].ToString()
							};
							farmitemattr.AddOrUpdate(key, new List<FarmItemAttr> { attr }, delegate(long k, List<FarmItemAttr> v)
							{
								v.Add(attr);
								return v;
							});
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetFarmItemAttr error: {0}", ex.Message);
				return false;
			}
		}

		private static bool SearchFarm(string Nickname, out int FoundFarmUnqueNum, out string PW)
		{
			FoundFarmUnqueNum = 0;
			PW = string.Empty;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_SearchFarm";
					mySqlCommand.Parameters.Add("pNickname", MySqlDbType.VarString).Value = Nickname;
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = null;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						FoundFarmUnqueNum = Convert.ToInt32(mySqlDataReader["FoundFarmUnqueNum"]);
						PW = mySqlDataReader["FoundFarmPassword"].ToString();
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_SearchFarm error: {0}", ex.Message);
				return false;
			}
		}

		private static bool SearchFarmByUserNum(string Nickname, int UserNum, out int FoundFarmUnqueNum, out string PW)
		{
			FoundFarmUnqueNum = 0;
			PW = string.Empty;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_SearchFarm";
					mySqlCommand.Parameters.Add("pNickname", MySqlDbType.VarString).Value = Nickname;
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						FoundFarmUnqueNum = Convert.ToInt32(mySqlDataReader["FoundFarmUnqueNum"]);
						PW = mySqlDataReader["FoundFarmPassword"].ToString();
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_SearchFarm error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ModifyObjectValueInfo(long OID, int value)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ModifyObjectValueInfo";
					mySqlCommand.Parameters.Add("pOID", MySqlDbType.Int64).Value = OID;
					mySqlCommand.Parameters.Add("pValue", MySqlDbType.Int32).Value = value;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
						{
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ModifyObjectValueInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ClearUserFarmSlotInfo(Account User, int SlotNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ClearUserFarmSlotInfo";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.MyFarmUniqueNum;
					mySqlCommand.Parameters.Add("pFarmSlotNum", MySqlDbType.Int32).Value = SlotNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ClearUserFarmSlotInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool SetFarmPortalInfo(int UserNum, long FarmItemID, string nickname, string memo, out int portalFarmUniqueNum)
		{
			portalFarmUniqueNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_SetFarmPortalInfo";
					mySqlCommand.Parameters.Add("portalMasterUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("objectID", MySqlDbType.Int64).Value = FarmItemID;
					mySqlCommand.Parameters.Add("farmMasterNickname", MySqlDbType.VarString).Value = nickname;
					mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
						{
							portalFarmUniqueNum = Convert.ToInt32(mySqlDataReader["portalFarmUniqueNum"]);
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_SetFarmPortalInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool GetFarmPortalInfo(long FarmItemID, out string nickname, out string memo, out int portalFarmUniqueNum)
		{
			portalFarmUniqueNum = 0;
			nickname = string.Empty;
			memo = string.Empty;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_GetFarmPortalInfo";
					mySqlCommand.Parameters.Add("objectID", MySqlDbType.Int64).Value = FarmItemID;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						portalFarmUniqueNum = Convert.ToInt32(mySqlDataReader["portalFarmUniqueNum"]);
						nickname = mySqlDataReader["farmMasterNickname"].ToString();
						memo = mySqlDataReader["memo"].ToString();
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetFarmPortalInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool FarmExchangeItem(Account User, int Itemnum, int count, int rewardCondition, string itemNums, out List<ExchangeItemInfo> exinfo)
		{
			exinfo = new List<ExchangeItemInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_exchangeItemGiveReward";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pItemNum", MySqlDbType.Int32).Value = Itemnum;
					mySqlCommand.Parameters.Add("pCount", MySqlDbType.Int32).Value = count;
					mySqlCommand.Parameters.Add("rewardCondition", MySqlDbType.Int32).Value = rewardCondition;
					mySqlCommand.Parameters.Add("itemNums", MySqlDbType.VarString).Value = itemNums;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							ExchangeItemInfo exchangeItemInfo = new ExchangeItemInfo
							{
								type = Convert.ToInt32(mySqlDataReader["rewardType"]),
								id = Convert.ToInt32(mySqlDataReader["rewardID"]),
								count = Convert.ToInt32(mySqlDataReader["amount"])
							};
							if (exchangeItemInfo.type == 200)
							{
								User.TR += exchangeItemInfo.count;
							}
							else if (exchangeItemInfo.type == 500)
							{
								User.Exp += exchangeItemInfo.count;
							}
							exinfo.Add(exchangeItemInfo);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_exchangeItemGiveReward error: {0}", ex.Message);
				return false;
			}
		}

		public static int EnsureFarmExp(int userNum, int farmExp)
		{
			if (userNum <= 0)
			{
				return farmExp < 1 ? 1 : farmExp;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				int dbExp = 0;
				int accExp = 0;
				using (MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT IFNULL(fdFarmExp,0), IFNULL(fdAccExp,0) FROM tblUserFarmInfo WHERE fdUserNum=@u LIMIT 1",
					mySqlConnection))
				{
					mySqlCommand.Parameters.AddWithValue("@u", userNum);
					using MySqlDataReader reader = mySqlCommand.ExecuteReader();
					if (reader.Read())
					{
						dbExp = Convert.ToInt32(reader[0]);
						accExp = Convert.ToInt32(reader[1]);
					}
				}
				int best = farmExp;
				if (dbExp > best)
				{
					best = dbExp;
				}
				if (accExp > best)
				{
					best = accExp;
				}
				if (best < 1)
				{
					best = 1;
				}
				if (best != dbExp)
				{
					using MySqlCommand mySqlCommand = new MySqlCommand(
						"UPDATE tblUserFarmInfo SET fdFarmExp=@e WHERE fdUserNum=@u",
						mySqlConnection);
					mySqlCommand.Parameters.AddWithValue("@e", best);
					mySqlCommand.Parameters.AddWithValue("@u", userNum);
					mySqlCommand.ExecuteNonQuery();
				}
				return best;
			}
			catch (Exception ex)
			{
				Log.Error("EnsureFarmExp error: {0}", ex.Message);
				return farmExp < 1 ? 1 : farmExp;
			}
		}

		private static bool ClearUserFarmMapInfo(int UserNum, int FarmUniqueNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ClearUserFarmMapInfo";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
					Log.Error("usp_Farm_ClearUserFarmMapInfo error: {0}", ex.Message);
				return false;
			}
		}

		internal static int PackedFarmTypeNum()
		{
			return 1;
		}

		internal static int FarmMapNum(MyFarmInfo info)
		{
			_ = info;
			return 1;
		}
	}
}
