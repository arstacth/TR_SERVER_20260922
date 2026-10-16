using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using Akka.Actor;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Serilog;

namespace AgentServer.Structuring.Park
{
	public class HuMongPickBoardData
	{
		public int HuMongPickBoardNum;

		private int TotalItemCount;

		private int ResetCount;

		public DateTime LastResetTime;

		public List<HuMongPickBoardItemInfo> ItemList = new List<HuMongPickBoardItemInfo>();

		public bool[] PickInfo;

		private readonly object drawLock = new object();

		public void AddToItemList(HuMongPickBoardItemInfo Item)
		{
			ItemList.Add(Item);
		}

		public void UpdatePickBoardInfo(int huMongPickBoardNum, bool[] pickInfo)
		{
			HuMongPickBoardNum = huMongPickBoardNum;
			PickInfo = pickInfo;
			TotalItemCount = ItemList.Sum((HuMongPickBoardItemInfo s) => s.ItemMax) + 1;
			ResetCount = ItemList.FirstOrDefault().ResetCount;
			LastResetTime = DateTime.Now.AddSeconds(10.0);
		}

		public byte PickItem(Account User, short PickID, out int AdditionRewardItemNum, out int ret)
		{
			AdditionRewardItemNum = 0;
			if (ItemList.Sum((HuMongPickBoardItemInfo s) => s.ItemCount) <= 0)
			{
				ret = 10;
				return 0;
			}
			HuMongPickBoardItemInfo huMongPickBoardItemInfo = new HuMongPickBoardItemInfo();
			int num = 11;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			while (true)
			{
				IL_005f:
				int num2 = random.Next(1, TotalItemCount);
				int num3 = 1;
				int num4 = 1;
				foreach (HuMongPickBoardItemInfo item in ItemList.OrderBy((HuMongPickBoardItemInfo o) => o.ItemMax))
				{
					num4 += item.ItemMax;
					if (num3 <= num2 && num2 < num4)
					{
						if (item.ItemCount <= 0)
						{
							goto IL_005f;
						}
						if (num >= item.Rank)
						{
							num = item.Rank;
							huMongPickBoardItemInfo = item;
						}
						break;
					}
					num3 = num4;
				}
				break;
			}
			lock (drawLock)
			{
				ret = 0;
				byte result = 0;
				bool flag = false;
				int num5 = 0;
				int num6 = 0;
				try
				{
					using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_HuMongPickBoard_PickItem";
					mySqlCommand.Parameters.Add("PickBoardNum", MySqlDbType.Int32).Value = HuMongPickBoardNum;
					mySqlCommand.Parameters.Add("PickID", MySqlDbType.Int16).Value = PickID;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = huMongPickBoardItemInfo.ItemNum;
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("selectTime", MySqlDbType.DateTime).Value = DateTime.Now;
					mySqlCommand.Parameters.Add("resetCount", MySqlDbType.Int32).Value = ResetCount;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					ret = mySqlDataReader.GetInt32("retval");
					if (ret == 0)
					{
						num5 = mySqlDataReader.GetInt32("resultItemNum");
						result = mySqlDataReader.GetByte("rank");
						flag = mySqlDataReader.GetBoolean("isReset");
						num6 = mySqlDataReader.GetInt32("Cost");
						int @int = mySqlDataReader.GetInt32("CostType");
						AdditionRewardItemNum = mySqlDataReader.GetInt32("AdditionRewardItemNum");
						ServerStatus.LBServerActor.Tell(new HuMongPickBoardUpdate
						{
							PickBoardNum = HuMongPickBoardNum,
							ItemNum = num5,
							isReset = flag,
							PickID = PickID - 1
						});
						if (@int == 1)
						{
							User.Cash -= num6;
						}
					}
					else
					{
						Log.Error("HuMongPickBoard:{3} Nickname:{0}, itemnum:{2}, RET: {1}", User.NickName, ret, huMongPickBoardItemInfo.ItemNum, HuMongPickBoardNum);
					}
				}
				catch (Exception ex)
				{
					Log.Error("usp_HuMongPickBoard_PickItem Error: {0}", ex.Message);
					ret = 1;
				}
				return result;
			}
		}

		public int UpdateHuMongPickBoardInfo()
		{
			Dictionary<int, HuMongPickBoardItemInfo> dictionary = new Dictionary<int, HuMongPickBoardItemInfo>();
			Dictionary<short, bool> dictionary2 = new Dictionary<short, bool>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_HuMongPickBoard_GetInfo";
				mySqlCommand.Parameters.Add("PickBoardNum", MySqlDbType.Int32).Value = HuMongPickBoardNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					mySqlDataReader.GetInt32("fdPickBoardNum");
					int @int = mySqlDataReader.GetInt32("fdItemNum");
					mySqlDataReader.GetInt16("fdResetCount");
					HuMongPickBoardItemInfo value = new HuMongPickBoardItemInfo
					{
						ItemNum = @int,
						ItemCount = mySqlDataReader.GetInt16("fdItemCount"),
						ItemMax = mySqlDataReader.GetInt16("fdItemMax"),
						Rank = mySqlDataReader.GetByte("fdRank"),
						ResetCount = mySqlDataReader.GetInt32("fdResetCount")
					};
					dictionary.Add(@int, value);
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					mySqlDataReader.GetInt32("fdPickBoardNum");
					short int2 = mySqlDataReader.GetInt16("fdPickID");
					bool boolean = mySqlDataReader.GetBoolean("fdIsPicked");
					dictionary2.Add(int2, boolean);
				}
			}
			HuMongPickBoardData huMongPickBoardData = new HuMongPickBoardData();
			foreach (KeyValuePair<int, HuMongPickBoardItemInfo> item in dictionary)
			{
				huMongPickBoardData.AddToItemList(item.Value);
			}
			huMongPickBoardData.UpdatePickBoardInfo(HuMongPickBoardNum, dictionary2.Values.ToArray());
			EventPickBoardHolder.HuMongPickBoardContainer[HuMongPickBoardNum] = huMongPickBoardData;
			return HuMongPickBoardNum;
		}
	}
}
