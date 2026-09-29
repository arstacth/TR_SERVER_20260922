using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using RoomServer.Holders;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Item;
using RoomServer.Structuring.Room;
using Serilog;

namespace RoomServer.Packet
{
	public class FarmHandle
	{
		public static void Handle_EnterFarm(IActorRef Sender, PacketReader reader, byte last)
		{
			if (!GameRoomHandle.EnterRoomGetUserInfo(Sender, reader, out var User))
			{
				return;
			}
			int FarmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			int num = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength);
			int num2 = reader.ReadLEInt16();
			string text = string.Empty;
			if (num2 > 0)
			{
				text = reader.ReadBig5StringSafe(num2);
			}
			reader.ReadLEInt32();
			reader.ReadBoolean();
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			bool flag = Rooms.RoomList.Values.Any((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum);
			int pGuildFarm = ((num != 75) ? 1 : (-1));
			if (!RequestJoinFarmRoom(User, FarmUniqueNum, pGuildFarm, out var roominfo) || FarmUniqueNum <= 0)
			{
				return;
			}
			if (roominfo.IsMaster)
			{
				User.MyFarmUniqueNum = FarmUniqueNum;
			}
			User.CurrentFarmUniqueNum = FarmUniqueNum;
			if (!flag)
			{
				if (!(roominfo.Password != text) || User.Attribute != 0 || User.MyFarmUniqueNum == FarmUniqueNum)
				{
					RoomHolder.RoomKindInfos.TryGetValue(num, out var value);
					RoomSettings settings = new RoomSettings
					{
						Name = roominfo.FarmName,
						Password = roominfo.Password,
						IsTeamPlay = 0,
						ItemType = 0,
						IsStepOn = false,
						MapNum = SendFarmInfo.FarmMapNum(roominfo.FarmTypeNum),
						RoomKindID = num,
						roomkindinfo = value,
						FarmIndex = FarmUniqueNum,
						FarmRoomInfo = roominfo
					};
					lock (User.CreateRoomLock)
					{
						if (!User.InGame && User.CurrentRoomId == 0)
						{
							NormalRoom normalRoom = new NormalRoom(User, settings, last);
							User.SendAsync(new GameRoom_EnterRoomOK(last));
							User.SendAsync(new SendFarmItemPos(normalRoom.FarmRoomMapCache, last));
							User.SendAsync(new SendFarmInfo(User, normalRoom, FarmUniqueNum, last));
							User.SendAsync(new GameRoom_SendPlayerInfo(User, num, last));
							User.SendAsync(new GameRoom_GetRoomMaster(normalRoom.RoomMasterIndex, last));
							normalRoom.GetSortedHeroClassGameIndices();
							if (roominfo.IsMaster || User.MyFarmUniqueNum == User.CurrentFarmUniqueNum)
							{
								User.SendAsync(new GetCurrentMapFarmCraftItem(new List<FarmCraftMapItem>(), last));
							}
						}
						return;
					}
				}
				User.SendAsync(new GameRoom_EnterRoomError(7, (byte)num, last));
			}
			else
			{
				NormalRoom normalRoom = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum).FirstOrDefault();
				if (normalRoom != null && normalRoom.FarmRoomInfo != null)
				{
					normalRoom.FarmRoomInfo.farmExp = roominfo.farmExp;
				}
				normalRoom.EnterRoom(User, text, last);
				if (roominfo.IsMaster || User.MyFarmUniqueNum == User.CurrentFarmUniqueNum)
				{
					User.SendAsync(new GetCurrentMapFarmCraftItem(new List<FarmCraftMapItem>(), last));
				}
			}
		}

		public static void Handle_CreatePublicFarm(IActorRef Sender, PacketReader reader, byte last)
		{
			if (!GameRoomHandle.EnterRoomGetUserInfo(Sender, reader, out var User))
			{
				return;
			}
			int FarmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			_ = string.Empty;
			if (num > 0)
			{
				reader.ReadBig5StringSafe(num);
			}
			reader.ReadLEInt32();
			reader.ReadBoolean();
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			byte type = reader.ReadByte();
			byte b = reader.ReadByte();
			byte unk = reader.ReadByte();
			int num2 = reader.ReadLEInt16();
			_ = string.Empty;
			if (num2 > 0)
			{
				text = reader.ReadBig5StringSafe(num2);
			}
			bool flag = Rooms.RoomList.Values.Any((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum);
			if (!RequestJoinFarmRoom(User, FarmUniqueNum, -1, out var roominfo))
			{
				return;
			}
			if (roominfo.IsMaster)
			{
				User.MyFarmUniqueNum = FarmUniqueNum;
			}
			User.CurrentFarmUniqueNum = FarmUniqueNum;
			NormalRoom normalRoom;
			if (!flag)
			{
				RoomHolder.RoomKindInfos.TryGetValue(75, out var value);
				roominfo.isPublic = true;
				roominfo.Type = type;
				roominfo.MaxUserLimit = b;
				roominfo.unk1 = unk;
				roominfo.FarmName = text;
				RoomSettings settings = new RoomSettings
				{
					Name = roominfo.FarmName,
					Password = roominfo.Password,
					IsTeamPlay = 0,
					ItemType = 0,
					IsStepOn = false,
					MapNum = SendFarmInfo.FarmMapNum(roominfo.FarmTypeNum),
					RoomKindID = 75,
					roomkindinfo = value,
					FarmIndex = FarmUniqueNum,
					FarmRoomInfo = roominfo
				};
				lock (User.CreateRoomLock)
				{
					if (!User.InGame && User.CurrentRoomId == 0)
					{
						normalRoom = new NormalRoom(User, settings, last);
						normalRoom.setSlotCount(b);
						User.SendAsync(new GameRoom_EnterRoomOK(last));
						User.SendAsync(new SendFarmItemPos(normalRoom.FarmRoomMapCache, last));
						User.SendAsync(new SendFarmInfo(User, normalRoom, FarmUniqueNum, last));
						User.SendAsync(new GameRoom_SendPlayerInfo(User, 75, last));
						User.SendAsync(new GameRoom_GetRoomMaster(normalRoom.RoomMasterIndex, last));
						normalRoom.GetSortedHeroClassGameIndices();
						if (User.MyFarmUniqueNum == User.CurrentFarmUniqueNum)
						{
							User.SendAsync(new GetCurrentMapFarmCraftItem(new List<FarmCraftMapItem>(), last));
						}
						Rooms.PublicFarmRoom.TryAdd(normalRoom.ID, normalRoom);
					}
					return;
				}
			}
			normalRoom = Rooms.RoomList.Values.Where((NormalRoom rm) => rm.FarmIndex == FarmUniqueNum).FirstOrDefault();
			normalRoom.FarmRoomInfo.isPublic = true;
			normalRoom.FarmRoomInfo.Type = type;
			normalRoom.FarmRoomInfo.MaxUserLimit = b;
			normalRoom.FarmRoomInfo.unk1 = unk;
			normalRoom.FarmRoomInfo.FarmName = text;
			normalRoom.setName(text);
			normalRoom.setSlotCount(b);
			normalRoom.EnterRoom(User, string.Empty, last);
			if (User.MyFarmUniqueNum == User.CurrentFarmUniqueNum)
			{
				User.SendAsync(new GetCurrentMapFarmCraftItem(new List<FarmCraftMapItem>(), last));
			}
			Rooms.PublicFarmRoom.TryAdd(normalRoom.ID, normalRoom);
		}

		public static void Handle_ReloadFarmMapInfo(PacketReader reader, byte last)
		{
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			_ = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			_ = last;
		}

		public static void Handle_ClearUserFarmMapInfo(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				if (room != null)
				{
					room.BroadcastToAll(new Farm_ClearUserFarmMapInfo(farmUniqueNum, last));
				}
			}
		}

		public static void Handle_ModifyFarmMapInfo(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (room == null || !value.InGame)
			{
				return;
			}
			int mine = Utility.NormalizeFarmUnique(value.MyFarmUniqueNum);
			int idx = Utility.NormalizeFarmUnique(room.FarmIndex);
			int cur = Utility.NormalizeFarmUnique(value.CurrentFarmUniqueNum);
			if (mine != idx && cur != idx)
			{
				return;
			}
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			int farmTypeNum = ResolveFarmMapType(farmUniqueNum, reader.ReadLEInt32());
			int num = reader.ReadLEInt32();
			List<FarmMapInfo> list = new List<FarmMapInfo>();
			for (int i = 0; i < num; i++)
			{
				int modifyType = reader.ReadLEInt32();
				reader.ReadLEInt32();
				long farmItemID = reader.ReadLEInt64();
				byte cellPosX = reader.ReadByte();
				byte cellPosY = reader.ReadByte();
				byte cellPosZ = reader.ReadByte();
				byte cellWidth = reader.ReadByte();
				byte cellHeight = reader.ReadByte();
				byte blockPosX = reader.ReadByte();
				byte blockPosY = reader.ReadByte();
				byte blockWidth = reader.ReadByte();
				byte blockHeight = reader.ReadByte();
				byte rotateType = reader.ReadByte();
				reader.ReadLEInt16();
				float angle = reader.ReadLESingle();
				FarmMapInfo iteminfo = new FarmMapInfo
				{
					FarmItemID = farmItemID,
					CellPosX = cellPosX,
					CellPosY = cellPosY,
					CellPosZ = cellPosZ,
					CellWidth = cellWidth,
					CellHeight = cellHeight,
					BlockPosX = blockPosX,
					BlockPosY = blockPosY,
					BlockWidth = blockWidth,
					BlockHeight = blockHeight,
					RotateType = rotateType,
					Angle = angle,
					ModifyType = modifyType
				};
				if (ModifyFarmMap_Ex(value, farmTypeNum, farmUniqueNum, iteminfo, out var outiteminfo))
				{
					list.Add(outiteminfo);
					ApplyFarmMapCache(room, outiteminfo);
				}
			}
			value.SendAsync(new ModifyFarmMapInfo(farmUniqueNum, list, last));
		}

		public static void Handle_IncreaseAnimalSize(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (value.InGame && value.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemNum = reader.ReadLEInt32();
				long farmItemID = reader.ReadLEInt64();
				if (IncreaseAnimalSize(value.UserNum, farmUniqueNum, farmItemID, itemNum, out var currentGrowthLevel))
				{
					room.BroadcastToAll(new IncreaseAnimalSizeOK(farmItemID, currentGrowthLevel, last));
				}
			}
		}

		public static void Handle_RestoreAnimalDefaultSize(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			int farmUniqueNum = Utility.NormalizeFarmUnique(reader.ReadLEInt32());
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (value.InGame && value.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemNum = reader.ReadLEInt32();
				long farmItemID = reader.ReadLEInt64();
				if (RestoreAnimalDefaultSize(value.UserNum, farmUniqueNum, farmItemID, itemNum, out var currentGrowthLevel))
				{
					room.BroadcastToAll(new RestoreAnimalDefaultSizeOK(farmItemID, currentGrowthLevel, last));
				}
			}
		}

		public static void Handle_ChangeFarmSkybox(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (value.InGame && value.MyFarmUniqueNum == room.FarmIndex)
			{
				int itemDescNum = reader.ReadLEInt32();
				if (ChangeFarmSkybox(value, room, itemDescNum))
				{
					room.BroadcastToAll(new ChangeFarmSkybox_Ack(room.FarmIndex, room, last));
				}
			}
		}

		public static void Handle_ChangeFarmMapTypeBySlot(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				if (value.InGame && value.MyFarmUniqueNum == room.FarmIndex)
				{
					room.BroadcastToAll(new Farm_ChangeFarmMapTypeBySlot_UpdateItem(room.FarmRoomMapCache, last));
					room.BroadcastToAll(new Farm_ChangeFarmMapTypeBySlot_UpdateInfo(room.FarmIndex, room, last));
				}
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
							// IFNULL(...,0) returns STRING dates; Convert.ToDateTime under th-TH
							// treats year as Buddhist Era (CE-543) → huge negative ms → client "not owner".
							ExpireTime = FarmDateToMs(mySqlDataReader["ExpireTime"]),
							CreateTime = FarmDateToMs(mySqlDataReader["CreateTime"]),
							Password = mySqlDataReader["Password"].ToString(),
							TotalCount = Convert.ToInt32(mySqlDataReader["TotalCount"]),
							TodaysVisitorCount = Convert.ToInt32(mySqlDataReader["TodaysVisitorCount"]),
							PremiumFarmUsing = Convert.ToBoolean(mySqlDataReader["PremiumFarmUsing"]),
							PremiumFarmExpireDateTime = FarmDateToMs(mySqlDataReader["PremiumFarmExpireDateTime"], unsetMagic: true),
							farmExp = Convert.ToInt32(mySqlDataReader["farmExp"]),
							isGuildFarm = (pGuildFarm == 1)
						};
						roominfo.farmExp = EnsureFarmExp(User.UserNum, roominfo.farmExp);
						if (User.MyFarmInfo != null && User.MyFarmInfo.farmExp > roominfo.farmExp)
						{
							roominfo.farmExp = User.MyFarmInfo.farmExp;
						}
						if (roominfo.IsMaster)
						{
							User.MyFarmUniqueNum = FarmUniqueNum;
						}
						// Own-farm join must always bind unique even if IsMaster flag mis-reads.
						if (User.MyFarmUniqueNum <= 0)
						{
							try
							{
								using MySqlConnection ownConn = new MySqlConnection(Conf.Connstr);
								ownConn.Open();
								using MySqlCommand ownCmd = new MySqlCommand(
									"SELECT IFNULL(fdMyFarmUniqueNum,0) FROM tbluserfarminfo WHERE fdUserNum=@u LIMIT 1",
									ownConn);
								ownCmd.Parameters.AddWithValue("@u", User.UserNum);
								object own = ownCmd.ExecuteScalar();
								if (own != null && own != DBNull.Value)
								{
									User.MyFarmUniqueNum = Convert.ToInt32(own);
								}
							}
							catch
							{
							}
						}
						if (User.MyFarmUniqueNum == FarmUniqueNum)
						{
							roominfo.IsMaster = true;
						}
						if (User.MyFarmUniqueNum == FarmUniqueNum)
						{
							if (User.MyFarmInfo == null)
							{
								User.MyFarmInfo = new MyFarmInfo();
							}
							User.MyFarmInfo.FarmTypeNum = roominfo.FarmTypeNum;
							User.MyFarmInfo.FarmSkyTypeNum = roominfo.FarmSkyTypeNum;
							User.MyFarmInfo.FarmWeatherTypeNum = roominfo.FarmWeatherTypeNum;
							User.MyFarmInfo.FarmName = roominfo.FarmName;
							User.MyFarmInfo.MasterName = roominfo.MasterName;
							User.MyFarmInfo.ExpireTime = roominfo.ExpireTime;
							User.MyFarmInfo.CreateTime = roominfo.CreateTime;
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

		/// <summary>
		/// Join-farm SP returns IFNULL(datetime,0) as string. On th-TH culture,
		/// Convert.ToDateTime treats the year as Buddhist Era (CE−543), producing
		/// huge negative timestamps the client treats as expired / not-owner.
		/// </summary>
		private static long FarmDateToMs(object value, bool unsetMagic = false)
		{
			const long magic = 1842465389770955L;
			const long maxOk = 2145916800000L;
			if (value == null || value is DBNull)
			{
				return unsetMagic ? magic : maxOk;
			}
			DateTime dt;
			if (value is DateTime dateTime)
			{
				dt = dateTime;
			}
			else
			{
				string text = value.ToString();
				if (string.IsNullOrEmpty(text) || text == "0")
				{
					return unsetMagic ? magic : maxOk;
				}
				if (!DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
				{
					return unsetMagic ? magic : maxOk;
				}
			}
			if (dt.Year > 0 && dt.Year < 1900)
			{
				dt = dt.AddYears(543);
			}
			long num = Utility.ConvertToTimestamp(dt);
			if (num <= 0L || num < 946684800000L)
			{
				return unsetMagic ? magic : maxOk;
			}
			if (num > maxOk)
			{
				return maxOk;
			}
			return num;
		}

		private static void GetFarmPoint(int UserNum, out int farmpoint)
		{
			farmpoint = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_GetFamrPoint";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				farmpoint = Convert.ToInt32(mySqlDataReader["FarmPoint"]);
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_GetFamrPoint error: {0}", ex.Message);
			}
		}

		public static void GetFarmMapInfo(int FarmUniqueNum, int guildNum, out List<FarmMapInfo> farmmapinfo, out int farmMapType)
		{
			farmMapType = 0;
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
				Log.Error("usp_Farm_GetFarmMapInfo error: {0}, FarmUniqueNum:{1}", ex.Message, FarmUniqueNum);
			}
			if (farmmapinfo.Count == 0)
			{
				LoadFarmMapInfoFallback(FarmUniqueNum, farmmapinfo);
			}
			// Do not coerce farmMapType 1→0: shop/terrain mode 1 is valid and
			// forcing 0 leaves the farm shop UI unable to exit cleanly.
		}

		private static void LoadFarmMapInfoFallback(int FarmUniqueNum, List<FarmMapInfo> farmmapinfo)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT t1.fdFarmItemID AS FarmItemID, t2.fdItemDescNum AS ItemDescNum, " +
					"t1.fdCellPosX AS CellPosX, t1.fdCellPosY AS CellPosY, t1.fdCellPosZ AS CellPosZ, " +
					"t1.fdCellWidth AS CellWidth, t1.fdCellHeight AS CellHeight, " +
					"t1.fdBlockPosX AS BlockPosX, t1.fdBlockPosY AS BlockPosY, " +
					"t1.fdBlockWidth AS BlockWidth, t1.fdBlockHeight AS BlockHeight, " +
					"t1.fdRotateType AS RotateType, t1.fdAngle AS Angle, " +
					"t2.fdExp AS experience, t2.fdView AS ViewLevel " +
					"FROM tblUserFarmMapInfo t1 JOIN tblUserFarmItemInfo t2 ON t1.fdFarmItemID = t2.fdFarmItemID " +
					"WHERE t1.fdFarmUniqueNum=@u",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", FarmUniqueNum);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				long nowTime = Utility.CurrentTimeMilliseconds();
				while (mySqlDataReader.Read())
				{
					farmmapinfo.Add(new FarmMapInfo
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
					});
				}
			}
			catch (Exception ex)
			{
				Log.Error("FarmMapInfo fallback error: {0}, FarmUniqueNum:{1}", ex.Message, FarmUniqueNum);
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

		private static int ResolveFarmMapType(int farmUniqueNum, int clientType)
		{
			if (clientType > 1)
			{
				return clientType;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT fdFarmTypeNum FROM tblUserFarmList WHERE fdFarmUniqueNum=@u LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", farmUniqueNum);
				object obj = mySqlCommand.ExecuteScalar();
				if (obj != null && obj != DBNull.Value)
				{
					int listType = Convert.ToInt32(obj);
					if (listType > 0)
					{
						return listType;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("ResolveFarmMapType error: {0}, FarmUniqueNum:{1}", ex.Message, farmUniqueNum);
			}
			return clientType < 0 ? 0 : clientType;
		}

		private static void ApplyFarmMapCache(NormalRoom room, FarmMapInfo item)
		{
			if (room == null || item == null)
			{
				return;
			}
			if (room.FarmRoomMapCache == null)
			{
				room.FarmRoomMapCache = new List<FarmMapInfo>();
			}
			room.FarmRoomMapCache.RemoveAll((FarmMapInfo x) => x.FarmItemID == item.FarmItemID);
			if (item.ModifyType != 3)
			{
				room.FarmRoomMapCache.Add(item);
			}
		}

		private static void EnsureFarmModifyPermission(int userNum, int farmUniqueNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using (MySqlCommand insert = new MySqlCommand(
					"INSERT INTO tblUserFarmPermissionInfo (fdUserNum, fdFarmUniqueNum, fdView, fdModify, fdHarvest, fdMaster) " +
					"SELECT @u, @f, 1, 1, 1, 1 FROM DUAL WHERE NOT EXISTS (" +
					"SELECT 1 FROM tblUserFarmPermissionInfo WHERE fdUserNum=@u AND fdFarmUniqueNum=@f)",
					mySqlConnection))
				{
					insert.Parameters.AddWithValue("@u", userNum);
					insert.Parameters.AddWithValue("@f", farmUniqueNum);
					insert.ExecuteNonQuery();
				}
				using (MySqlCommand update = new MySqlCommand(
					"UPDATE tblUserFarmPermissionInfo SET fdModify=1, fdView=1, fdHarvest=1, fdMaster=1 " +
					"WHERE fdUserNum=@u AND fdFarmUniqueNum=@f",
					mySqlConnection))
				{
					update.Parameters.AddWithValue("@u", userNum);
					update.Parameters.AddWithValue("@f", farmUniqueNum);
					update.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				Log.Error("EnsureFarmModifyPermission error: {0}, UserNum:{1}, FarmUniqueNum:{2}", ex.Message, userNum, farmUniqueNum);
			}
		}

		private static bool ModifyFarmMapDirect(Account User, int FarmTypeNum, int FarmUniqueNum, FarmMapInfo iteminfo, out FarmMapInfo outiteminfo)
		{
			outiteminfo = new FarmMapInfo();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				int itemDescNum = 0;
				int exp = 0;
				int view = 0;
				using (MySqlCommand lookup = new MySqlCommand(
					"SELECT fdItemDescNum, IFNULL(fdExp,0), IFNULL(fdView,0) FROM tblUserFarmItemInfo WHERE fdFarmItemID=@id LIMIT 1",
					mySqlConnection))
				{
					lookup.Parameters.AddWithValue("@id", iteminfo.FarmItemID);
					using MySqlDataReader reader = lookup.ExecuteReader();
					if (!reader.Read())
					{
						return false;
					}
					itemDescNum = Convert.ToInt32(reader[0]);
					exp = Convert.ToInt32(reader[1]);
					view = Convert.ToInt32(reader[2]);
				}
				if (iteminfo.ModifyType == 3)
				{
					using MySqlCommand del = new MySqlCommand(
						"DELETE FROM tblUserFarmMapInfo WHERE fdFarmUniqueNum=@u AND fdFarmItemID=@id",
						mySqlConnection);
					del.Parameters.AddWithValue("@u", FarmUniqueNum);
					del.Parameters.AddWithValue("@id", iteminfo.FarmItemID);
					del.ExecuteNonQuery();
				}
				else
				{
					using MySqlCommand upd = new MySqlCommand(
						"UPDATE tblUserFarmMapInfo SET fdCellPosX=@x, fdCellPosY=@y, fdCellPosZ=@z, fdCellWidth=@w, fdCellHeight=@h, " +
						"fdBlockPosX=@bx, fdBlockPosY=@by, fdBlockWidth=@bw, fdBlockHeight=@bh, fdRotateType=@r, fdAngle=@a, fdFarmTypeNum=@t " +
						"WHERE fdFarmUniqueNum=@u AND fdFarmItemID=@id",
						mySqlConnection);
					upd.Parameters.AddWithValue("@x", iteminfo.CellPosX);
					upd.Parameters.AddWithValue("@y", iteminfo.CellPosY);
					upd.Parameters.AddWithValue("@z", iteminfo.CellPosZ);
					upd.Parameters.AddWithValue("@w", iteminfo.CellWidth);
					upd.Parameters.AddWithValue("@h", iteminfo.CellHeight);
					upd.Parameters.AddWithValue("@bx", iteminfo.BlockPosX);
					upd.Parameters.AddWithValue("@by", iteminfo.BlockPosY);
					upd.Parameters.AddWithValue("@bw", iteminfo.BlockWidth);
					upd.Parameters.AddWithValue("@bh", iteminfo.BlockHeight);
					upd.Parameters.AddWithValue("@r", iteminfo.RotateType);
					upd.Parameters.AddWithValue("@a", iteminfo.Angle);
					upd.Parameters.AddWithValue("@t", FarmTypeNum);
					upd.Parameters.AddWithValue("@u", FarmUniqueNum);
					upd.Parameters.AddWithValue("@id", iteminfo.FarmItemID);
					if (upd.ExecuteNonQuery() == 0)
					{
						using MySqlCommand ins = new MySqlCommand(
							"INSERT INTO tblUserFarmMapInfo (fdFarmUniqueNum, fdFarmTypeNum, fdFarmItemID, fdCellPosX, fdCellPosY, fdCellPosZ, " +
							"fdCellWidth, fdCellHeight, fdBlockPosX, fdBlockPosY, fdBlockWidth, fdBlockHeight, fdRotateType, fdAngle) VALUES " +
							"(@u, @t, @id, @x, @y, @z, @w, @h, @bx, @by, @bw, @bh, @r, @a)",
							mySqlConnection);
						ins.Parameters.AddWithValue("@u", FarmUniqueNum);
						ins.Parameters.AddWithValue("@t", FarmTypeNum);
						ins.Parameters.AddWithValue("@id", iteminfo.FarmItemID);
						ins.Parameters.AddWithValue("@x", iteminfo.CellPosX);
						ins.Parameters.AddWithValue("@y", iteminfo.CellPosY);
						ins.Parameters.AddWithValue("@z", iteminfo.CellPosZ);
						ins.Parameters.AddWithValue("@w", iteminfo.CellWidth);
						ins.Parameters.AddWithValue("@h", iteminfo.CellHeight);
						ins.Parameters.AddWithValue("@bx", iteminfo.BlockPosX);
						ins.Parameters.AddWithValue("@by", iteminfo.BlockPosY);
						ins.Parameters.AddWithValue("@bw", iteminfo.BlockWidth);
						ins.Parameters.AddWithValue("@bh", iteminfo.BlockHeight);
						ins.Parameters.AddWithValue("@r", iteminfo.RotateType);
						ins.Parameters.AddWithValue("@a", iteminfo.Angle);
						ins.ExecuteNonQuery();
					}
				}
				outiteminfo = new FarmMapInfo
				{
					FarmItemID = iteminfo.FarmItemID,
					ItemDescNum = itemDescNum,
					CellPosX = iteminfo.CellPosX,
					CellPosY = iteminfo.CellPosY,
					CellPosZ = iteminfo.CellPosZ,
					CellWidth = iteminfo.CellWidth,
					CellHeight = iteminfo.CellHeight,
					BlockPosX = iteminfo.BlockPosX,
					BlockPosY = iteminfo.BlockPosY,
					BlockWidth = iteminfo.BlockWidth,
					BlockHeight = iteminfo.BlockHeight,
					RotateType = iteminfo.RotateType,
					Angle = iteminfo.Angle,
					Exp = exp,
					View = view,
					ModifyType = iteminfo.ModifyType,
					NowTime = Utility.CurrentTimeMilliseconds()
				};
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("ModifyFarmMapDirect error: {0}, UserNum:{1}, FarmUniqueNum:{2}, FarmItemID:{3}", ex.Message, User.UserNum, FarmUniqueNum, iteminfo.FarmItemID);
				return false;
			}
		}

		private static bool ModifyFarmMap_Ex(Account User, int FarmTypeNum, int FarmUniqueNum, FarmMapInfo iteminfo, out FarmMapInfo outiteminfo)
		{
			outiteminfo = new FarmMapInfo();
			EnsureFarmModifyPermission(User.UserNum, FarmUniqueNum);
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ModifyFarmMapInfo_Ex";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmTypeNum", MySqlDbType.Int32).Value = FarmTypeNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					mySqlCommand.Parameters.Add("pFarmItemID", MySqlDbType.Int64).Value = iteminfo.FarmItemID;
					mySqlCommand.Parameters.Add("pCellPosX", MySqlDbType.Int16).Value = iteminfo.CellPosX;
					mySqlCommand.Parameters.Add("pCellPosY", MySqlDbType.Int16).Value = iteminfo.CellPosY;
					mySqlCommand.Parameters.Add("pCellWidth", MySqlDbType.Int16).Value = iteminfo.CellWidth;
					mySqlCommand.Parameters.Add("pCellHeight", MySqlDbType.Int16).Value = iteminfo.CellHeight;
					mySqlCommand.Parameters.Add("pBlockPosX", MySqlDbType.Int16).Value = iteminfo.BlockPosX;
					mySqlCommand.Parameters.Add("pBlockPosY", MySqlDbType.Int16).Value = iteminfo.BlockPosY;
					mySqlCommand.Parameters.Add("pBlockWidth", MySqlDbType.Int16).Value = iteminfo.BlockWidth;
					mySqlCommand.Parameters.Add("pBlockHeight", MySqlDbType.Int16).Value = iteminfo.BlockHeight;
					mySqlCommand.Parameters.Add("pRotateType", MySqlDbType.Int16).Value = iteminfo.RotateType;
					mySqlCommand.Parameters.Add("pAngle", MySqlDbType.Float).Value = iteminfo.Angle;
					mySqlCommand.Parameters.Add("pRunLevel", MySqlDbType.Int32).Value = User.Level;
					mySqlCommand.Parameters.Add("pFarmLevel", MySqlDbType.Int32).Value = 1;
					mySqlCommand.Parameters.Add("pModifyType", MySqlDbType.Int32).Value = iteminfo.ModifyType;
					mySqlCommand.Parameters.Add("pCellPosZ", MySqlDbType.Int16).Value = iteminfo.CellPosZ;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						long nowTime = Utility.CurrentTimeMilliseconds();
						while (mySqlDataReader.Read())
						{
							FarmMapInfo farmMapInfo = (outiteminfo = new FarmMapInfo
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
								Exp = Convert.ToInt32(mySqlDataReader["Exp"]),
								View = Convert.ToInt32(mySqlDataReader["ViewLevel"]),
								ModifyType = Convert.ToInt32(mySqlDataReader["ModifyType"]),
								NowTime = nowTime
							});
						}
						return true;
					}
				}
				return ModifyFarmMapDirect(User, FarmTypeNum, FarmUniqueNum, iteminfo, out outiteminfo);
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ModifyFarmMapInfo_Ex error: {0}, UserNum:{1}, FarmUniqueNum:{2}, FarmItemID:{3}, ModifyType:{4}", ex.Message, User.UserNum, FarmUniqueNum, iteminfo.FarmItemID, iteminfo.ModifyType);
				return ModifyFarmMapDirect(User, FarmTypeNum, FarmUniqueNum, iteminfo, out outiteminfo);
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

		private static bool IncreaseAnimalSize(int UserNum, int FarmUniqueNum, long FarmItemID, int ItemNum, out int currentGrowthLevel)
		{
			currentGrowthLevel = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_IncreaseAnimalSize";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					mySqlCommand.Parameters.Add("pDestObjectID", MySqlDbType.Int64).Value = FarmItemID;
					mySqlCommand.Parameters.Add("pUsedItemDescNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("pDefaultSize", MySqlDbType.Int32).Value = 0;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						currentGrowthLevel = Convert.ToInt32(mySqlDataReader["currentGrowthLevel"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_IncreaseAnimalSize error: {0}", ex.Message);
				return false;
			}
		}

		private static bool RestoreAnimalDefaultSize(int UserNum, int FarmUniqueNum, long FarmItemID, int ItemNum, out int currentGrowthLevel)
		{
			currentGrowthLevel = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_RestoreAnimalDefaultSize";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					mySqlCommand.Parameters.Add("pDestObjectID", MySqlDbType.Int64).Value = FarmItemID;
					mySqlCommand.Parameters.Add("pUsedItemDescNum", MySqlDbType.Int32).Value = ItemNum;
					mySqlCommand.Parameters.Add("pDefaultSize", MySqlDbType.Int32).Value = 0;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						currentGrowthLevel = Convert.ToInt32(mySqlDataReader["currentGrowthLevel"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_RestoreAnimalDefaultSize error: {0}", ex.Message);
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

		public static bool ChangeFarmMapTypeByItem(Account User, NormalRoom room, int ItemDescNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ChangeFarmMapTypeByItem";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = Utility.NormalizeFarmUnique(room.FarmIndex);
					mySqlCommand.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = ItemDescNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						room.FarmRoomInfo.FarmTypeNum = Convert.ToInt32(mySqlDataReader["NewFarmType"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmMapTypeByItem error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ChangeFarmMapType(Account User, NormalRoom room, int FarmTypeNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ChangeFarmMapType";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.CurrentFarmUniqueNum;
					mySqlCommand.Parameters.Add("pNewFarmTypeNum", MySqlDbType.Int32).Value = FarmTypeNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						room.FarmRoomInfo.FarmTypeNum = Convert.ToInt32(mySqlDataReader["NewFarmType"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmMapTypeByItem error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ChangeFarmSkybox(Account User, NormalRoom room, int ItemDescNum)
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
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.CurrentFarmUniqueNum;
					mySqlCommand.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = ItemDescNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						room.FarmRoomInfo.FarmSkyTypeNum = Convert.ToInt32(mySqlDataReader["NewSkyboxType"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmSkyboxByItem error: {0}", ex.Message);
				return false;
			}
		}

		public static bool ChangeFarmName(Account User, string name)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_SetFarmName";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pGMUser", MySqlDbType.Int32).Value = User.Attribute;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.CurrentFarmUniqueNum;
					mySqlCommand.Parameters.Add("pFarmName", MySqlDbType.VarString).Value = name;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_SetFarmName error: {0}", ex.Message);
				return false;
			}
		}

		public static bool ChangeFarmPassword(Account User, string pw)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ModifyFarmPassword";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pGMUser", MySqlDbType.Int32).Value = User.Attribute;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.CurrentFarmUniqueNum;
					mySqlCommand.Parameters.Add("pNewPassword", MySqlDbType.VarString).Value = pw;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ModifyFarmPassword error: {0}", ex.Message);
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

		private static bool FarmExchangeItem(Account User, int Itemnum, int count, int rewardCondition, out List<ExchangeItemInfo> exinfo)
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
	}
}
