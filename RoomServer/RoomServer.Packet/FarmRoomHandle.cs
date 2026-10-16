using System;
using System.Collections.Generic;
using System.Data;
using Akka.Actor;
using Ionic.Zlib;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.Room;
using RoomServer.Packet.RoomServer;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Farm;
using Serilog;

namespace RoomServer.Packet
{
	public class FarmRoomHandle
	{
		public static void Handle_FarmAction(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			long farmItemID = reader.ReadLEInt64();
			long unk = reader.ReadLEInt64();
			long unk2 = reader.ReadLEInt64();
			bool isOn = reader.ReadBoolean();
			if (User.InGame)
			{
				room?.BroadcastToAll(new FarmAction_Ack(User.RoomPos, farmItemID, unk, unk2, isOn, last));
			}
		}

		public static void Handle_ChangeFarmRoomName(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || User.MyFarmUniqueNum != room.FarmIndex)
			{
				return;
			}
			reader.ReadLEInt32();
			int num = reader.ReadLEInt16();
			string empty = string.Empty;
			if (num > 3 && num <= 20)
			{
				empty = reader.ReadBig5StringSafe(num);
				if (FarmHandle.ChangeFarmName(User, empty))
				{
					room.setName(empty);
					room.FarmRoomInfo.FarmName = empty;
					room.BroadcastToAll(new ChangeFarmName_Ack(User.MyFarmUniqueNum, empty, last));
					User.MyFarmInfo.FarmName = empty;
					if (room.FarmRoomInfo.isPublic)
					{
						room.BroadcastToAgent(new RM_To_AG_AddPublicFarmList(room));
					}
					return;
				}
			}
			User.SendAsync(new ChangeFarmName_FailAck(last));
		}

		public static void Handle_ChangeFarmRoomPassword(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || User.MyFarmUniqueNum != room.FarmIndex)
			{
				return;
			}
			reader.ReadLEInt32();
			int num = reader.ReadLEInt16();
			string text = string.Empty;
			if (num > 0 && num <= 50)
			{
				text = reader.ReadBig5StringSafe(num);
			}
			if (FarmHandle.ChangeFarmPassword(User, text))
			{
				room.setPassword(text);
				room.FarmRoomInfo.Password = text;
				room.BroadcastToAll(new ChangeFarmPassword_Ack(User.MyFarmUniqueNum, text, last));
				if (room.FarmRoomInfo.isPublic)
				{
					room.BroadcastToAgent(new RM_To_AG_AddPublicFarmList(room));
				}
			}
		}

		public static void Handle_PublicFarmRoom(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || User.MyFarmUniqueNum != room.FarmIndex)
			{
				return;
			}
			reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			byte type = reader.ReadByte();
			byte b = reader.ReadByte();
			byte unk = reader.ReadByte();
			int num = reader.ReadLEInt16();
			string empty = string.Empty;
			if (num > 3 && num <= 24)
			{
				empty = reader.ReadBig5StringSafe(num);
				reader.ReadBoolean();
				room.FarmRoomInfo.isPublic = flag;
				room.FarmRoomInfo.Type = type;
				room.FarmRoomInfo.MaxUserLimit = (byte)(flag ? b : 20);
				room.FarmRoomInfo.unk1 = unk;
				room.FarmRoomInfo.FarmName = empty;
				room.setName(empty);
				room.setSlotCount(room.FarmRoomInfo.MaxUserLimit);
				room.BroadcastToAll(new PublicFarmOpen_Ack(room.FarmRoomInfo, last));
				if (!flag)
				{
					Rooms.PublicFarmRoom.TryRemove(room.ID, out var _);
					{
						foreach (IActorRef value2 in AgentServer.AgentServerList.Values)
						{
							value2.Tell(new RemoveFarmRoom
							{
								RoomID = room.ID
							});
						}
						return;
					}
				}
				Rooms.PublicFarmRoom.TryAdd(room.ID, room);
				room.BroadcastToAgent(new RM_To_AG_AddPublicFarmList(room));
			}
			else
			{
				room.BroadcastToAll(new PublicFarmOpen_FailAck(last));
			}
		}

		public static void Handle_FarmCraft_ModifyFarmMapInfo(Account User, PacketReader reader, byte last)
		{
			reader.Offset += 4;
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (room == null)
			{
				return;
			}
			if (User.MyFarmUniqueNum <= 0 && User.CurrentFarmUniqueNum > 0)
			{
				User.MyFarmUniqueNum = User.CurrentFarmUniqueNum;
			}
			int mine = Utility.NormalizeFarmUnique(User.MyFarmUniqueNum);
			int idx = Utility.NormalizeFarmUnique(room.FarmIndex);
			int cur = Utility.NormalizeFarmUnique(User.CurrentFarmUniqueNum);
			if (mine != idx && cur != idx)
			{
				return;
			}
			if (User.MyFarmUniqueNum <= 0)
			{
				User.MyFarmUniqueNum = room.FarmIndex;
			}
			int num = reader.ReadLEInt32();
			reader.ReadByte();
			ushort num2 = 0;
			byte[] array = new byte[4];
			string text = string.Empty;
			string text2 = string.Empty;
			if (num > 0)
			{
				num2 = reader.ReadLEUInt16();
				array = reader.ReadByteArray(num2);
				num = reader.ReadLEInt32();
				reader.ReadByte();
				PacketReader packetReader = new PacketReader(ZlibStream.UncompressBuffer(array), 0);
				Dictionary<byte, int> dictionary = new Dictionary<byte, int>();
				for (int i = 0; i < num; i++)
				{
					packetReader.Offset += 3;
					byte key = packetReader.ReadByte();
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, 1);
					}
					else
					{
						dictionary[key]++;
					}
				}
				foreach (KeyValuePair<byte, int> item in dictionary)
				{
					text += $"{item.Key},";
					text2 += $"{item.Value},";
				}
			}
			if (!FarmCraft_ModifyFarmMapInfo(User.UserNum, room, text, text2, num, num2, array))
			{
				return;
			}
			if (num > 0)
			{
				room.FarmCraftMapCache.isFarmCraft = true;
				room.FarmCraftMapCache.TotalBlock = num;
				room.FarmCraftMapCache.CompressedData = array;
			}
			else
			{
				room.FarmCraftMapCache.isFarmCraft = false;
				room.FarmCraftMapCache.TotalBlock = 0;
				room.FarmCraftMapCache.CompressedData = null;
			}
			User.SendAsync(new FarmCraft_ModifyFarmMapOK(last));
			foreach (Account item2 in room.PlayerList())
			{
				if (item2.UserNum != User.UserNum)
				{
					item2.SendAsync(new GetFarmCraftMapData(room.FarmIndex, room.FarmCraftMapCache, last));
				}
			}
		}

		public static void Handle_ReloadMapInfo(Account User, PacketReader reader, byte last)
		{
			reader.Offset += 4;
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && room.RoomKindID == 75)
			{
				FarmHandle.GetFarmMapInfo(room.FarmIndex, 0, out var farmmapinfo, out var _);
				room.FarmRoomMapCache = farmmapinfo;
				room.BroadcastToAll(new GetFarmCraftMapData(room.FarmIndex, room.FarmCraftMapCache, last));
				room.BroadcastToAll(new Farm_FFD20133(last));
				if (farmmapinfo.Count > 0)
				{
					room.BroadcastToAll(new GetFarmItemPos_Ack(farmmapinfo, last));
					room.BroadcastToAll(new ReloadFarmTypeNum(room.FarmIndex, 0, last));
				}
			}
		}

		public static void Handle_SaveUserFarmSlotInfo(Account User, PacketReader reader, byte last)
		{
			reader.Offset += 4;
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && User.MyFarmUniqueNum == room.FarmIndex)
			{
				int num = reader.ReadLEInt32();
				int num2 = reader.ReadLEInt16();
				string title = string.Empty;
				string memo = string.Empty;
				long LatestDateTime = 0L;
				bool flag = true;
				bool flag2 = false;
				if (num2 > 3 && num2 <= 20)
				{
					title = reader.ReadBig5StringSafe(num2);
					int fixedLength = reader.ReadLEInt16();
					memo = reader.ReadBig5StringSafeNL(fixedLength);
					flag2 = SaveUserFarmSlotInfo(User.UserNum, room, num, title, memo, out LatestDateTime);
				}
				else
				{
					flag = false;
				}
				if (flag && flag2)
				{
					User.SendAsync(new SaveUserFarmSlotInfo_Ack(num, room.FarmRoomInfo.FarmTypeNum, title, memo, LatestDateTime, last));
				}
				else
				{
					User.SendAsync(new SaveUserFarmSlotInfoFail_Ack(last));
				}
			}
		}

		public static void Handle_ChangeFarmTypeByItem_New(Account User, PacketReader reader, byte last)
		{
			if (reader.Remaining >= 4)
			{
				reader.ReadLEInt32();
			}
			int itemDescNum = 0;
			if (reader.Remaining >= 4)
			{
				itemDescNum = reader.ReadLEInt32();
			}
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int farm = Utility.NormalizeFarmUnique(room != null ? room.FarmIndex : User.MyFarmUniqueNum);
			if (farm <= 0)
			{
				farm = Utility.NormalizeFarmUnique(User.MyFarmUniqueNum);
			}
			bool applied = false;
			if (room != null && itemDescNum > 0)
			{
				applied = FarmHandle.ChangeFarmMapTypeByItem(User, room, itemDescNum);
			}
			User.SendAsync(new FarmCraft_ModifyFarmMapOK(last, 8));
			// Prefer SP-updated FarmTypeNum; attr-70 alone can ACK type=1 with empty voxels
			// when map caches were not rebuilt for the new type.
			int terrainType = 0;
			if (room != null && room.FarmRoomInfo.FarmTypeNum > 0)
			{
				terrainType = room.FarmRoomInfo.FarmTypeNum;
			}
			if (terrainType <= 0)
			{
				terrainType = GetFarmTerrainType(itemDescNum);
			}
			if (terrainType <= 0 && farm > 0)
			{
				FarmHandle.GetFarmMapInfo(farm, 0, out _, out terrainType);
			}
			User.SendAsync(new ChangeFarmMapTypeByItem_Ack(farm, terrainType, last));
			// Do NOT push empty craft/voxel dumps — that zeros client voxels (size[0]).
			// Persist is already done by usp_Farm_ChangeFarmMapTypeByItem; ACK is enough
			// for onRecvFarmChangeMapTypeOK. Only re-push map items when present.
			if (applied && room != null)
			{
				FarmHandle.GetFarmMapInfo(room.FarmIndex, 0, out var farmmapinfo, out _);
				room.FarmRoomMapCache = farmmapinfo;
				if (farmmapinfo != null && farmmapinfo.Count > 0)
				{
					room.BroadcastToAll(new GetFarmItemPos_Ack(farmmapinfo, last));
				}
				Log.Information("opcode8 0x64 farm={0} item={1} terrain={2} applied items={3}",
					farm, itemDescNum, terrainType, farmmapinfo != null ? farmmapinfo.Count : 0);
			}
			else
			{
				Log.Information("opcode8 0x64 farm={0} item={1} terrain={2} applied={3}", farm, itemDescNum, terrainType, applied);
			}
		}

		private static int GetFarmTerrainType(int itemDescNum)
		{
			if (itemDescNum <= 0)
			{
				return 0;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT fdValue FROM tblAvatarItemDescAttr WHERE fdItemNum=@i AND fdAttr=70 LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@i", itemDescNum);
				object obj = mySqlCommand.ExecuteScalar();
				if (obj != null && obj != DBNull.Value)
				{
					return Convert.ToInt32(Convert.ToDouble(obj));
				}
			}
			catch (Exception ex)
			{
				Log.Error("GetFarmTerrainType error: {0}", ex.Message);
			}
			return 0;
		}

		public static void Handle_ChangeFarmMapTypeBySlot_New(Account User, PacketReader reader, byte last)
		{
			reader.Offset += 4;
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && User.MyFarmUniqueNum == room.FarmIndex)
			{
				int slotNum = reader.ReadLEInt32();
				if (ChangeFarmMapTypeBySlot(User, room, slotNum))
				{
					FarmHandle.GetFarmMapInfo(room.FarmIndex, 0, out var farmmapinfo, out var _);
					GetFarmCraftMapDataInfo(room.FarmIndex, out var farmcraftmapdata);
					room.FarmRoomMapCache = farmmapinfo;
					room.FarmCraftMapCache = farmcraftmapdata;
					User.SendAsync(new ChangeFarmMapTypeBySlot_Ack(room.FarmIndex, room, last));
					GetCurrentMapFarmCraftItemInfo(room.FarmIndex, -1, out var farmcraftmapitem);
					User.SendAsync(new GetCurrentMapFarmCraftItem(farmcraftmapitem, last));
					room.BroadcastToAll(new GetFarmCraftMapData(room.FarmIndex, farmcraftmapdata, last));
				}
			}
		}

		private static bool SaveUserFarmSlotInfo(int UserNum, NormalRoom room, int FarmSlotNum, string Title, string Memo, out long LatestDateTime)
		{
			LatestDateTime = 0L;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_SaveUserFarmSlotInfo";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = room.FarmIndex;
					mySqlCommand.Parameters.Add("pFarmSlotNum", MySqlDbType.Int32).Value = FarmSlotNum;
					mySqlCommand.Parameters.Add("pFarmTypeNum", MySqlDbType.Int32).Value = room.FarmRoomInfo.FarmTypeNum;
					mySqlCommand.Parameters.Add("sTitle", MySqlDbType.VarString).Value = Title;
					mySqlCommand.Parameters.Add("sMemo", MySqlDbType.VarString).Value = Memo;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						LatestDateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["fdLatestDateTime"]));
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_SaveUserFarmSlotInfo error: {0}", ex.Message);
				return false;
			}
		}

		private static bool ChangeFarmMapTypeBySlot(Account User, NormalRoom room, int SlotNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Farm_ChangeFarmMapTypeBySlot";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = User.MyFarmUniqueNum;
					mySqlCommand.Parameters.Add("pFarmSlotNum", MySqlDbType.Int32).Value = SlotNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						room.FarmRoomInfo.FarmTypeNum = Convert.ToInt32(mySqlDataReader["NewFarmType"]);
						room.FarmRoomInfo.FarmSkyTypeNum = Convert.ToInt32(mySqlDataReader["fdNewSkyBoxNum"]);
						room.FarmRoomInfo.FarmWeatherTypeNum = Convert.ToInt32(mySqlDataReader["fdNewWeatherNum"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_ChangeFarmMapTypeBySlot error: {0}", ex.Message);
				return false;
			}
		}

		public static void GetFarmCraftMapDataInfo(int FarmUniqueNum, out FarmCraftMapData farmcraftmapdata)
		{
			farmcraftmapdata = new FarmCraftMapData();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_FarmCraft_GetFarmMapDataInfo";
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					int dataSize = Convert.ToInt32(mySqlDataReader["DataSize"]);
					farmcraftmapdata.TotalBlock = Convert.ToInt32(mySqlDataReader["FarmCraftBlockCount"]);
					if (dataSize > 0)
					{
						byte[] array = new byte[dataSize];
						mySqlDataReader.GetBytes(mySqlDataReader.GetOrdinal("FarmCraftCompressedData"), 0L, array, 0, array.Length);
						farmcraftmapdata.CompressedData = array;
						farmcraftmapdata.isFarmCraft = true;
					}
					else
					{
						farmcraftmapdata.isFarmCraft = false;
						farmcraftmapdata.CompressedData = new byte[0];
					}
				}
				else
				{
					farmcraftmapdata.isFarmCraft = false;
					farmcraftmapdata.TotalBlock = 0;
					farmcraftmapdata.CompressedData = new byte[0];
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_FarmCraft_GetFarmMapDataInfo error: {0}", ex.Message);
			}
		}

		public static void GetCurrentMapFarmCraftItemInfo(int FarmUniqueNum, int ItemNum, out List<FarmCraftMapItem> farmcraftmapitem)
		{
			farmcraftmapitem = new List<FarmCraftMapItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_FarmCraft_GetCurrentFarmItemInfo";
				mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
				mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					FarmCraftMapItem item = new FarmCraftMapItem
					{
						ItemKind = Convert.ToByte(mySqlDataReader["FarmCraftItemKind"]),
						TotalCount = Convert.ToInt32(mySqlDataReader["FarmCraftItemTotalCount"]),
						UsedCount = Convert.ToInt32(mySqlDataReader["FarmCraftItemUsedCount"])
					};
					farmcraftmapitem.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_FarmCraft_GetCurrentFarmItemInfo error: {0}", ex.Message);
			}
		}

		private static bool FarmCraft_ModifyFarmMapInfo(int UserNum, NormalRoom room, string ItemKindStr, string UsedCountStr, int BlockCount, int DataSize, byte[] CompressedData)
		{
			try
			{
				string[] array = Conf.Connstr.Split(';');
				string text = string.Empty;
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (!text2.Contains("charset=") && !string.IsNullOrEmpty(text2))
					{
						text = text + text2 + ";";
					}
				}
				using (MySqlConnection mySqlConnection = new MySqlConnection(text))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_FarmCraft_ModifyFarmMapInfo";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("pFarmTypeNum", MySqlDbType.Int32).Value = room.FarmRoomInfo.FarmTypeNum;
					mySqlCommand.Parameters.Add("pFarmUniqueNum", MySqlDbType.Int32).Value = room.FarmIndex;
					mySqlCommand.Parameters.Add("ItemKindStr", MySqlDbType.VarString).Value = ItemKindStr;
					mySqlCommand.Parameters.Add("UsedCountStr", MySqlDbType.VarString).Value = UsedCountStr;
					mySqlCommand.Parameters.Add("BlockCount", MySqlDbType.Int32).Value = BlockCount;
					mySqlCommand.Parameters.Add("DataSize", MySqlDbType.Int32).Value = DataSize;
					mySqlCommand.Parameters.Add("CompressedData", MySqlDbType.Blob).Value = CompressedData;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (mySqlDataReader["ret"].ToString() == "0")
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_FarmCraft_ModifyFarmMapInfo error: UserNum:{1}, DataSize:{2}, {0}", ex.Message, UserNum, DataSize);
				return false;
			}
		}
	}
}
