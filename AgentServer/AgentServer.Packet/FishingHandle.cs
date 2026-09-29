using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class FishingHandle
	{
		public static void Handle_GetFishRecordInfo(ClientConnection Client, byte last)
		{
			GetFishRecord(Client.CurrentAccount.NickName, out var fishrecord);
			Client.SendAsync(new FishRecordInfo(fishrecord, last));
		}

		public static void Handle_GetFishNetInfo(ClientConnection Client, byte last)
		{
			GetFishNetInfo(Client.CurrentAccount.UserNum, out var fishnetitems);
			Client.SendAsync(new FishNetInfo(fishnetitems, last));
		}

		public static void Handle_MyRanking(ClientConnection Client, byte last)
		{
			int rank = 0;
			int point = 0;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdRank, fdPoint FROM GameDataFishingRank WHERE fdUserNum=@u LIMIT 1",
					conn);
				cmd.Parameters.AddWithValue("@u", Client.CurrentAccount.UserNum);
				using MySqlDataReader r = cmd.ExecuteReader();
				if (r.Read())
				{
					rank = r.GetInt32(0);
					point = r.GetInt32(1);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Fish MyRanking: {0}", ex.Message);
			}
			Client.SendAsync(new FishMyRankingAck(rank, point, last));
		}

		public static void Handle_AsyncFishingPoint(ClientConnection Client, byte last)
		{
			int point = 0;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdAsyncPoint FROM UserFishingInfo WHERE fdUserNum=@u LIMIT 1",
					conn);
				cmd.Parameters.AddWithValue("@u", Client.CurrentAccount.UserNum);
				object o = cmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					point = Convert.ToInt32(o);
				}
			}
			catch (Exception ex)
			{
				Log.Warning("Fish AsyncPoint: {0}", ex.Message);
			}
			Client.SendAsync(new FishAsyncPointAck(point, last));
		}

		public static void Handle_Fishing(ClientConnection Client, PacketReader reader, byte last)
		{
			byte b = reader.ReadByte();
			Account currentAccount = Client.CurrentAccount;
			// Start REQ includes rod/bait/map; stop is only action (+ optional last).
			int rod = 0;
			int bait = 0;
			if (reader.Remaining >= 12)
			{
				rod = reader.ReadLEInt32();
				bait = reader.ReadLEInt32();
				reader.ReadLEInt32();
			}
			lock (currentAccount.fishingLock)
			{
				if (b == 1)
				{
					NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
					if (!currentAccount.InGame || room == null)
					{
						int err = 295;
						Client.SendAsync(new FishingPacketFail(err, last));
					}
					else if (!currentAccount.isFishing && (FishingCheck(currentAccount.UserNum, rod, bait) || currentAccount.activeItem.getOnItemList(185).Count > 0 || rod > 0 || FishingHolder.FishingEquips.Count > 0))
					{
						if (rod <= 0 && FishingHolder.FishingEquips.Count > 0)
						{
							rod = FishingHolder.FishingEquips.Keys.First();
						}
						if (bait <= 0 && FishingHolder.FishingBaitsDrawItem.Count > 0)
						{
							bait = FishingHolder.FishingBaitsDrawItem.Keys.First();
						}
						currentAccount.isFishing = true;
						currentAccount.FishingRodItemNum = rod;
						currentAccount.FishingBaitItemNum = bait;
						currentAccount.FishingCancelSource = new CancellationTokenSource();
						currentAccount.FishingCancelToken = currentAccount.FishingCancelSource.Token;
						currentAccount.StartFishingThread();
						Client.SendAsync(new FishingPacket(b, last));
					}
					else if (currentAccount.isFishing)
					{
						int err2 = 287;
						Client.SendAsync(new FishingPacketFail(err2, last));
					}
					else
					{
						int err3 = 295;
						Client.SendAsync(new FishingPacketFail(err3, last));
					}
				}
				else
				{
					currentAccount.isFishing = false;
					if (currentAccount.FishingCancelSource != null)
					{
						currentAccount.FishingCancelSource.Cancel();
					}
					Client.SendAsync(new FishingPacket(b, last));
				}
			}
		}

		public static void Handle_CollectFishedItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte b = reader.ReadByte();
			byte b2 = reader.ReadByte();
			if (currentAccount.isFishing)
			{
				int err = 297;
				Client.SendAsync(new CollectFishedItemFail(err, last));
			}
			else if (b == 0 && b2 == 0)
			{
				byte b3 = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(1, 9);
				byte b4 = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(1, 9);
				currentAccount.FishAns1 = b3;
				currentAccount.FishAns2 = b4;
				Client.SendAsync(new CollectFishedItemAns(b3, b4, last));
			}
			else if (b != currentAccount.FishAns1 || b2 != currentAccount.FishAns2)
			{
				int err2 = 294;
				Client.SendAsync(new CollectFishedItemFail(err2, last));
			}
			else
			{
				GiveFishNetItemToMyRoom(currentAccount.UserNum, out var fishnetitems);
				Client.SendAsync(new CollectFishedItem(fishnetitems, last));
			}
		}

		public static void Handle_GetFarmFishingReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (currentAccount.InGame && room != null)
			{
				storage_GetFarmFishingReward(room.FarmIndex, out var farmRewardList);
				if (currentAccount.MyFarmUniqueNum == room.FarmIndex)
				{
					ServerStatus.ToRoomServer(new RM_UpdateFarmFishingReward(currentAccount, farmRewardList.Count > 0, last), room.RoomServerID);
				}
				Client.SendAsync(new GetFarmFishingReward_ACK(currentAccount.MyFarmUniqueNum == room.FarmIndex, farmRewardList, last));
			}
		}

		public static void Handle_SetFarmFishingReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			long uniqueNum = Convert.ToInt64(reader.ReadBig5StringSafe(fixedLength));
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (!currentAccount.InGame || room == null || currentAccount.MyFarmUniqueNum != room.FarmIndex)
			{
				Client.SendAsync(new SetFarmFishingReward_ACK(null, last, 298));
			}
			else if (storage_setAttr(currentAccount.UserNum, uniqueNum, 0, 2, 1f))
			{
				storage_GetFarmFishingReward(currentAccount.MyFarmUniqueNum, out var farmRewardList);
				Client.SendAsync(new SetFarmFishingReward_ACK(farmRewardList, last));
				ServerStatus.ToRoomServer(new RM_UpdateFarmFishingReward(currentAccount, farmRewardList.Count > 0, last), room.RoomServerID);
				byte[] packet = new SetFarmFishingReward_RoomUpdate_ACK(farmRewardList, last).ToArray();
				room.BroadcastToAll(currentAccount.Session, packet);
			}
			else
			{
				Client.SendAsync(new SetFarmFishingReward_ACK(null, last, 300));
			}
		}

		public static void Handle_RemoveFarmFishingReward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			long uniqueNum = Convert.ToInt64(reader.ReadBig5StringSafe(fixedLength));
			NormalRoom room = Rooms.GetRoom(currentAccount.CurrentRoomId);
			if (!currentAccount.InGame || room == null || currentAccount.MyFarmUniqueNum != room.FarmIndex)
			{
				Client.SendAsync(new RemoveFarmFishingReward_ACK(null, last, 298));
			}
			else if (storage_setAttr(currentAccount.UserNum, uniqueNum, 0, 2, 0f))
			{
				storage_GetFarmFishingReward(currentAccount.MyFarmUniqueNum, out var farmRewardList);
				Client.SendAsync(new RemoveFarmFishingReward_ACK(farmRewardList, last));
				ServerStatus.ToRoomServer(new RM_UpdateFarmFishingReward(currentAccount, farmRewardList.Count > 0, last), room.RoomServerID);
				byte[] packet = new RemoveFarmFishingReward_RoomUpdate_ACK(farmRewardList, last).ToArray();
				room.BroadcastToAll(currentAccount.Session, packet);
			}
			else
			{
				Client.SendAsync(new RemoveFarmFishingReward_ACK(null, last, 299));
			}
		}

		public static void Handle_MiniGameFishing(ClientConnection Client, PacketReader reader, byte last)
		{
			Account User = Client.CurrentAccount;
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (!User.InGame || room == null)
			{
				return;
			}
			byte b = reader.ReadByte();
			bool flag = reader.ReadBoolean();
			List<int> onItemList = User.activeItem.getOnItemList(185);
			List<int> onItemList2 = User.activeItem.getOnItemList(186);
			if (onItemList.Count <= 0)
			{
				onItemList = User.activeItem.getPositions(185);
			}
			if (onItemList2.Count <= 0)
			{
				onItemList2 = User.activeItem.getPositions(186);
			}
			if (onItemList.Count <= 0)
			{
				Log.Warning("Fishing mini-game no rod user={0}", User.NickName);
				return;
			}
			int rodNum = onItemList.FirstOrDefault();
			int num = onItemList2.FirstOrDefault();
			Random rand = new Random(Guid.NewGuid().GetHashCode());
			int fisheditemnum = User.FishedItemNum;
			int num2 = User.FishedSize;
			int num3 = User.FishedItemNum;
			int num4 = 0;
			bool flag2 = User.FishedKeepNum != 0;
			if (!flag)
			{
				if (FishingHolder.Decoy_Ex.TryGetValue(num, out var value) && value != null && value.FailureReward
					&& FishingHolder.FishingFailureDrawItem.TryGetValue(num, out var value2) && value2 != null)
				{
					Decoy decoy = value2.NextWithReplacement();
					fisheditemnum = decoy.FishNum;
					num2 = 0;
					num3 = decoy.FishNum;
				}
			}
			else
			{
				if (b != User.RandomFish)
				{
					num4 = 2;
					Log.Warning("Player [{0}] minigame fishing success but different fish!", User.NickName);
					goto IL_034a;
				}
				if (flag2)
				{
					storage_GetFarmFishingReward(room.FarmIndex, out var farmRewardList);
					if (farmRewardList.Count > 0)
					{
						if (!farmRewardList.Exists((UserStorageItemInfo a) => a.uniqueNum == User.FishedKeepNum))
						{
							UserStorageItemInfo userStorageItemInfo = farmRewardList.OrderBy((UserStorageItemInfo _) => Guid.NewGuid()).FirstOrDefault();
							fisheditemnum = userStorageItemInfo.itemNum;
							User.FishedKeepNum = userStorageItemInfo.uniqueNum;
						}
						num4 = FishingSuccess(User.UserNum, fisheditemnum, 0, isFish: false, fisheditemnum, User.FishedKeepNum, rodNum);
						if (num4 == 0)
						{
							ServerStatus.ToRoomServer(new RM_UpdateFarmFishingReward(User, farmRewardList.Count - 1 > 0, 1), room.RoomServerID);
						}
						goto IL_034a;
					}
					FishingHolder.FishingBaitsDrawItem.TryGetValue(num, out var value3);
					Decoy decoy2 = value3.NextWithReplacement();
					fisheditemnum = decoy2.FishNum;
					num3 = decoy2.FishNum;
				}
			}
			bool flag3 = FishingHolder.Fishes.ContainsKey(fisheditemnum);
			if (flag3)
			{
				FishingHolder.Fishes.TryGetValue(num3, out var value4);
				num2 = ((User.FishedSize != 0) ? User.FishedSize : FishSizeCalc(rand, value4.MinSize, value4.MaxSize));
				fisheditemnum = ItemHolder.ItemShopInfos.Where((KeyValuePair<int, ItemShopInfo> w) => w.Value.supplyItemDescNum == fisheditemnum).FirstOrDefault().Key;
			}
			num4 = FishingSuccess(User.UserNum, fisheditemnum, num2, flag3, num3, 0L, rodNum);
			goto IL_034a;
			IL_034a:
			if (num4 == 0)
			{
				Client.SendAsync(new MiniGameFishing_Result(flag, fisheditemnum, num2, num, last));
				room.BroadcastToAll(User.Session, new MiniGameFishing_ResultNotify(User.RoomPos, flag, fisheditemnum, num2, flag2, last).ToArray());
			}
			else
			{
				int err = 287;
				switch (num4)
				{
				case 1:
					err = 296;
					break;
				case 2:
					err = 292;
					break;
				case 3:
					err = 289;
					break;
				case 4:
					err = 299;
					break;
				}
				Client.SendAsync(new FishedItemFail(err, 1));
				User.isFishing = false;
				if (User.FishingCancelSource != null)
				{
					User.FishingCancelSource.Cancel();
				}
			}
			User.FishedItemNum = 0;
			User.FishedSize = 0;
			User.FishedKeepNum = 0L;
		}

		private static void GetFishNetInfo(int usernum, out List<UserFishedItem> fishnetitems)
		{
			fishnetitems = new List<UserFishedItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Fishing_GetFishNetInfo";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int num = Convert.ToInt32(mySqlDataReader["fdFishNum"]);
					int itemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]);
					if (num > 0)
					{
						itemNum = num;
					}
					UserFishedItem item = new UserFishedItem
					{
						ItemNum = itemNum,
						Size = Convert.ToInt32(mySqlDataReader["fdMaxSize"]),
						Count = Convert.ToInt32(mySqlDataReader["fdCount"])
					};
					fishnetitems.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Fishing_GetFishNetInfo Error:{0}", ex.Message);
			}
			if (fishnetitems.Count == 0)
			{
				try
				{
					using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
					conn.Open();
					using MySqlCommand cmd = new MySqlCommand(
						"SELECT fdItemNum, fdMaxSize, fdCount, fdFishNum FROM userfishnetinfo WHERE fdUserNum=@u",
						conn);
					cmd.Parameters.AddWithValue("@u", usernum);
					using MySqlDataReader r = cmd.ExecuteReader();
					while (r.Read())
					{
						int fishNum = Convert.ToInt32(r["fdFishNum"]);
						int itemNum = Convert.ToInt32(r["fdItemNum"]);
						if (fishNum > 0)
						{
							itemNum = fishNum;
						}
						fishnetitems.Add(new UserFishedItem
						{
							ItemNum = itemNum,
							Size = Convert.ToInt32(r["fdMaxSize"]),
							Count = Convert.ToInt32(r["fdCount"])
						});
					}
				}
				catch (Exception ex2)
				{
					Log.Warning("userfishnetinfo fallback user={0}: {1}", usernum, ex2.Message);
				}
			}
		}

		private static bool FishingCheck(int usernum, int rod, int bait)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Fishing_Check";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
					mySqlCommand.Parameters.Add("roditem", MySqlDbType.Int32).Value = rod;
					mySqlCommand.Parameters.Add("baititem", MySqlDbType.Int32).Value = bait;
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
				Log.Error("usp_Fishing_Check Error:{0}", ex.Message);
				return false;
			}
		}

		private static void GiveFishNetItemToMyRoom(int usernum, out List<UserFishedItem> fishnetitems)
		{
			fishnetitems = new List<UserFishedItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Fishing_GiveItemToMyRoom";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					int num = Convert.ToInt32(mySqlDataReader["fdFishNum"]);
					int itemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]);
					if (num > 0)
					{
						itemNum = num;
					}
					UserFishedItem item = new UserFishedItem
					{
						ItemNum = itemNum,
						Size = Convert.ToInt32(mySqlDataReader["fdMaxSize"]),
						Count = Convert.ToInt32(mySqlDataReader["fdCount"])
					};
					fishnetitems.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Fishing_GiveItemToMyRoom Error:{0}", ex.Message);
			}
		}

		public static int FishingSuccess(int usernum, int fishedItem, int fishsize, bool isFish, int realfishedItem, long uniqueNum, int rodNum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_Fishing_Success";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = usernum;
					mySqlCommand.Parameters.Add("fishedItem", MySqlDbType.Int32).Value = fishedItem;
					mySqlCommand.Parameters.Add("fishsize", MySqlDbType.Int32).Value = fishsize;
					mySqlCommand.Parameters.Add("isFish", MySqlDbType.Int16).Value = isFish;
					mySqlCommand.Parameters.Add("realfishedItem", MySqlDbType.Int32).Value = realfishedItem;
					mySqlCommand.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = uniqueNum;
					mySqlCommand.Parameters.Add("rodNum", MySqlDbType.Int32).Value = rodNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						return mySqlDataReader.GetInt32("ret");
					}
				}
				return 9;
			}
			catch (Exception ex)
			{
				Log.Error("usp_Fishing_Success Error:{0}", ex.Message);
				return 9;
			}
		}

		public static void GetFishRecord(string NickName, out List<UserFishedItem> fishrecord)
		{
			fishrecord = new List<UserFishedItem>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Fishing_GetUserFishRecord";
				mySqlCommand.Parameters.Add("NickName", MySqlDbType.String).Value = NickName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					UserFishedItem item = new UserFishedItem
					{
						ItemNum = Convert.ToInt32(mySqlDataReader["fdItemNum"]),
						Size = Convert.ToInt32(mySqlDataReader["fdSize"]),
						Count = 0
					};
					fishrecord.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_Fishing_GetUserFishRecord Error:{0}", ex.Message);
			}
		}

		public static bool storage_setAttr(int userNum, long uniqueNum, byte storageSaveType, short ptype, float pvalue)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_storage_setAttr";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = userNum;
					mySqlCommand.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = uniqueNum;
					mySqlCommand.Parameters.Add("storageSaveType", MySqlDbType.Byte).Value = storageSaveType;
					mySqlCommand.Parameters.Add("ptype", MySqlDbType.Int16).Value = ptype;
					mySqlCommand.Parameters.Add("pvalue", MySqlDbType.Float).Value = pvalue;
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
				Log.Error("usp_storage_setAttr Error:{0}", ex.Message);
				return false;
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
				Log.Error("usp_storage_GetFarmFishing Error:{0}", ex.Message);
				return false;
			}
		}

		public static int FishSizeCalc(Random rand, int min, int max)
		{
			int num = rand.Next(0, 400);
			int num2 = 398;
			int num3 = 390;
			int num4 = 370;
			int num5 = 330;
			if (num >= num2)
			{
				min *= 20;
			}
			else if (num >= num3)
			{
				min *= 15;
				max = max / 100 * 80;
			}
			else if (num >= num4)
			{
				min *= 6;
				max = max / 100 * 55;
			}
			else if (num >= num5)
			{
				min *= 3;
				max = max / 100 * 35;
			}
			else
			{
				max = max / 100 * 15;
			}
			return Math.Max(1, rand.Next(min, Math.Max(min + 1, max)));
		}
	}
}
