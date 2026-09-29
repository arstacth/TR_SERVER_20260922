using System;
using System.Collections.Generic;
using System.Data;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using RoomServer.Holders;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Map;
using RoomServer.Structuring.Room;
using RoomServer.Structuring.SiegeMode;
using Serilog;

namespace RoomServer.Packet
{
	public class AssaultModeHandle
	{
		public static void Handle_GetAssaultModeLimitAttackInfo(Account User, byte last)
		{
			User.SendAsync(new AssaultModeAttackLimitInfo(last));
		}

		public static void Handle_GetAnubisPoint(Account User, byte last)
		{
			User.SendAsync(new AnubisUserPoint(0, last));
		}

		public static void Handle_GetAnubisOpenTime(Account User, byte last)
		{
			User.SendAsync(new AnubisOpenTime(last));
		}

		public static void Handle_GetDungeonRaidPoint(Account User, byte last)
		{
			DungeonRaidGetUserPoint(User.UserNum, out var point);
			User.SendAsync(new AssaultRaidPoint(point, last));
		}

		public static void Handle_GetAssaultRaidOpenTime(Account User, byte last)
		{
			User.SendAsync(new AssaultRaidOpenTime(last));
		}

		public static void AssaultMode_SetObjectInfo(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			for (int i = 1; i <= num; i++)
			{
				reader.ReadLEInt32();
				int key = reader.ReadLEInt32();
				int num2 = reader.ReadLEInt32();
				reader.ReadLEInt32();
				reader.ReadLEInt32();
				ObjectBoss objectBoss = new ObjectBoss();
				objectBoss.HP = num2;
				objectBoss.MaxHP = num2;
				room.AnubisObjectBoss.TryAdd(key, objectBoss);
			}
			room.BroadcastToAll(new SetObjectInfo_Ack(room, last));
		}

		public static void AssaultMode_SetCharacterEnergy(Account User, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			User.SendAsync(new SetCharacterEnergy_Ack(room, last));
		}

		public static void AssaultMode_DecreaseCharacterEnergy(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int num2 = User.HP - num;
			User.HP = ((num2 >= 0) ? num2 : 0);
			room.BroadcastToAll(new DecreaseCharacterEnergy_Ack(User, last));
		}

		public static void AssaultMode_ChargeCharacterEnergy(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (num < 0)
			{
				num = 100;
			}
			int num2 = ((room.RuleType == 45568 || room.RuleType == 111104) ? 100 : User.MaxHP);
			bool flag = User.HP == num2 && User.GameEndType == 0;
			if (!flag)
			{
				int num3 = User.HP + num;
				User.HP = ((num3 > num2) ? num2 : num3);
				User.GameEndType = 0;
				User.GameOver = false;
				room.Survival++;
			}
			if (num < 0)
			{
				room.BroadcastToAll(new AssaultModeRebirth_Ack(User, isRealRebirth: false, last));
			}
			room.BroadcastToAll(new ChargeCharacterEnergy_Ack(User, flag, last));
		}

		public static void AssaultMode_DecreaseObjectEnergy(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int unk = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			reader.ReadByte();
			int num2 = reader.ReadLEInt32();
			if (room.AnubisObjectBoss.ContainsKey(num))
			{
				int num3 = room.AnubisObjectBoss[num].HP - num2;
				room.AnubisObjectBoss[num].HP = ((num3 >= 0) ? num3 : 0);
				User.TotalDamage += num2;
				User.MaxDamage = ((num2 > User.MaxDamage) ? num2 : User.MaxDamage);
				room.BroadcastToAll(new DecreaseObjectEnergy_Ack(User, room, unk, num, num2, last));
			}
		}

		public static void AssaultMode_BounsItemMake(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (room.AnubisObjectBoss.TryGetValue(num2, out var value) && value.HP <= 0 && !User.BonusItemMade.Contains(num2))
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				int value2 = 0;
				int value3 = 0;
				if (AssaultModeMakeBonusItem(room.PlayingMapNum, num, 1, out value2))
				{
					dictionary.Add(1, value2);
				}
				else
				{
					Log.Warning("BounsItemMake Not Exists Group groupNum:{0}, objectid:{2} MapNum:{1}", num, room.PlayingMapNum, num2);
				}
				if (AssaultModeMakeBonusItem(room.PlayingMapNum, num, 2, out value3))
				{
					dictionary.Add(2, value3);
				}
				User.BonusItemMade.Add(num2);
				User.SendAsync(new BounsItemMake_Ack(num2, dictionary, last));
			}
		}

		public static void AssaultMode_BounsItemEat(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			int unk = reader.ReadLEInt32();
			int unk2 = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			lock (room.bounsLock)
			{
				if (num2 < 0)
				{
					Log.Warning("AssaultMode_BounsItemEat value < 0 v:{0} type:{1}", num2, num);
					num2 = 0;
				}
				if (num2 > 1000)
				{
					Log.Warning("AssaultMode_BounsItemEat value > 1000 v:{0} type:{1}", num2, num);
					num2 = 0;
				}
				if (!room.UserBounsItemInfos.ContainsKey(User.RoomPos, num))
				{
					room.UserBounsItemInfos[User.RoomPos][num] = num2;
				}
				else
				{
					room.UserBounsItemInfos[User.RoomPos][num] += num2;
				}
				User.SendAsync(new BounsItemEat_Ack(num, unk, unk2, room.UserBounsItemInfos[User.RoomPos][num], last));
			}
		}

		public static void AssaultMode_Rebirth(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			byte b = reader.ReadByte();
			bool flag = SiegeModeHolder.SiegeModeInfos.Exists((SiegeModeInfo e) => e.MapNum == room.PlayingMapNum);
			if (!(room.Survival > 0 || flag) || (b == 0 && (User.RebirthTime > 2 || room.PlayingMapNum == 9118)))
			{
				return;
			}
			if (b == 1)
			{
				if ((room.PlayingMapNum == 9118 && User.RebirthTime > 1) || !AssaultModeUseItem(User.UserNum, 810, out var itemnum, out var itemcount))
				{
					return;
				}
				User.SendAsync(new AssaultModeUseItem_Ack(itemnum, itemcount, last));
			}
			User.RebirthTime++;
			User.HP = User.MaxHP;
			User.GameEndType = 0;
			User.GameOver = false;
			room.Survival++;
			room.BroadcastToAll(new AssaultModeRebirth_Ack(User, isRealRebirth: true, last));
		}

		public static void Handle_InitMapBonusItem(Account User, PacketReader reader, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				int key = reader.ReadLEInt32();
				int bonusType = reader.ReadLEInt32();
				reader.Offset += 4;
				int num2 = reader.ReadLEInt32();
				reader.Offset += 4;
				BonusItemInfo value = new BonusItemInfo
				{
					BonusType = bonusType,
					BonusValue = ((num2 > 20) ? 20 : num2)
				};
				room.MapBonusItems.Add(key, value);
			}
			room.BroadcastToAll(new InitMapBounsItem_Ack(last));
		}

		public static void Handle_MapBonusItemEat(Account User, PacketReader reader, byte last)
		{
			if (!User.InGame)
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			int num = reader.ReadLEInt32();
			lock (room.bounsLock)
			{
				if (room.MapBonusItems.TryGetValue(num, out var value))
				{
					int num2 = 0;
					if (value.BonusType == 200)
					{
						num2 = 1;
					}
					else if (value.BonusType == 500)
					{
						num2 = 2;
					}
					else if (value.BonusType == 2700 || value.BonusType == 2701 || value.BonusType == 2702)
					{
						num2 = 3;
					}
					if (!room.UserBounsItemInfos.ContainsKey(User.RoomPos, num2))
					{
						room.UserBounsItemInfos[User.RoomPos][num2] = value.BonusValue;
					}
					else
					{
						room.UserBounsItemInfos[User.RoomPos][num2] += value.BonusValue;
					}
					if (num2 == 3)
					{
						User.BonusStagePoint += value.BonusValue;
					}
					User.SendAsync(new MapBounsItemEat_Ack(num, value, last));
				}
			}
		}

		private static bool DungeonRaidGetUserPoint(int UserNum, out int point)
		{
			point = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_DungeonRaidGetUserPoint";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						point = Convert.ToInt32(mySqlDataReader["point"]);
						return true;
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_DungeonRaidGetUserPoint Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool AssaultModeMakeBonusItem(int mapNum, int groupNum, int rewardType, out int value)
		{
			value = 0;
			AssaultModeRewardInfo value2;
			bool num = MapItemHolder.AssaultModeRewardInfos.TryGetValue(mapNum, groupNum, rewardType, out value2);
			if (num)
			{
				Random random = new Random(Guid.NewGuid().GetHashCode());
				value = random.Next(value2.minValue, value2.maxValue + 1);
			}
			return num;
		}

		public static bool AssaultModeUseItem(int UserNum, int position, out int itemnum, out int itemcount)
		{
			itemnum = 0;
			itemcount = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_assaultModeUseItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("position", MySqlDbType.Int32).Value = position;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						itemnum = Convert.ToInt32(mySqlDataReader["itemDescNum"]);
						itemcount = Convert.ToInt32(mySqlDataReader["itemCount"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_assaultModeUseItem Error:{0}", ex.Message);
				return false;
			}
		}

		public static void DungeonRaidAddPoint(int UserNum, int gotpoint)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_DungeonRaidAddPoint";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("gotPoint", MySqlDbType.Int32).Value = gotpoint;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_DungeonRaidAddPoint Error:{0}", ex.Message);
			}
		}
	}
}
