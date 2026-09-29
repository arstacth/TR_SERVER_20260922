using System;
using System.Collections.Generic;
using System.Data;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using RoomServer.Packet.RoomServer;
using RoomServer.Structuring;
using RoomServer.Structuring.User;
using Serilog;

namespace RoomServer.Packet
{
	public class FishingHandle
	{
		public static void Handle_UpdateFarmFishingReward(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (room != null)
			{
				bool hasFishingReward = (room.hasFishingReward = reader.ReadBoolean());
				if (Rooms.PublicFarmRoom.TryGetValue(value.CurrentRoomId, out var value2))
				{
					value2.hasFishingReward = hasFishingReward;
				}
				room.BroadcastToAgent(new RM_To_AG_UpdateFarmFishingReward(room));
			}
		}

		public static bool storage_GetFarmFishingReward(int FarmUniqueNum, out List<UserStorageItemInfo> farmRewardList)
		{
			farmRewardList = new List<UserStorageItemInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_storage_GetFarmFishing";
					mySqlCommand.Parameters.Add("FarmUniqueNum", MySqlDbType.Int32).Value = FarmUniqueNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					while (mySqlDataReader.Read())
					{
						UserStorageItemInfo item = new UserStorageItemInfo
						{
							uniqueNum = mySqlDataReader.GetInt64("uniqueNum"),
							itemNum = mySqlDataReader.GetInt32("itemNum")
						};
						farmRewardList.Add(item);
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_storage_setAttr Error:{0}", ex.Message);
				return false;
			}
		}
	}
}
