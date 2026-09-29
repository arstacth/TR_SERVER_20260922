using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Packet.Send;
using Akka.Actor;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using NetMsg.LBS;
using Serilog;
using TRCommon;

namespace AgentServer.Structuring.Park
{
	public class CapsuleMachineData
	{
		public int RealMachineNum;

		public int RealMachineNumKind;

		public bool isRotate;

		private int TotalItemCount;

		private short ResetCount;

		public DateTime LastResetTime;

		public List<CapsuleMachineItemNew> ItemList = new List<CapsuleMachineItemNew>();

		private readonly object drawLock = new object();

		public void AddToItemList(CapsuleMachineItemNew Item)
		{
			ItemList.Add(Item);
		}

		public void UpdateMachineInfo(int realMachineNum)
		{
			RealMachineNum = realMachineNum;
			ShopItemTable.getItemQueryFromItemDescNum(RealMachineNum, out var query);
			RealMachineNumKind = query.Item3;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT fdMachineKind, fdRotateGroupNum FROM essenrotatecapsulemachinegroup WHERE fdMachineNum=@n LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@n", realMachineNum);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.Read())
				{
					int kind = mySqlDataReader.GetInt32(0);
					if (kind > 0)
					{
						RealMachineNumKind = kind;
					}
					if (!mySqlDataReader.IsDBNull(1) && mySqlDataReader.GetInt32(1) != 0)
					{
						isRotate = true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warning("capsule kind lookup {0}: {1}", realMachineNum, ex.Message);
			}
			if (!isRotate)
			{
				isRotate = ItemList.Count > 0 && ItemList.FirstOrDefault().RotateGroupNum != 0;
			}
			TotalItemCount = ItemList.Sum((CapsuleMachineItemNew s) => s.ItemMax);
			ResetCount = ItemList.FirstOrDefault().ResetCount;
			LastResetTime = DateTime.Now.AddSeconds(10.0);
		}

		public int DrawItem(Account User, out int ret)
		{
			if (ItemList.Sum((CapsuleMachineItemNew s) => s.ItemCount) <= 0)
			{
				ret = 10;
				Log.Error("CapsuleMachine:{0} itemcount <= 0", RealMachineNum);
				return 0;
			}
			CapsuleMachineItemNew capsuleMachineItemNew = new CapsuleMachineItemNew();
			double num = (double)User.Luck;
			double num2 = 300.0;
			int num3 = 10;
			int num4 = 1;
			num4 = (int)(num / num2 + 1.0);
			if (num4 > num3)
			{
				num4 = num3;
			}
			int num5 = 0;
			foreach (CapsuleMachineItemNew item in ItemList)
			{
				num5 += item.ItemCount;
			}
			double num6 = (double)num5 * 1.0 / (double)TotalItemCount * 1.0;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			int num7 = 0;
			byte b = 250;
			while (num4 > 0)
			{
				num7 = random.Next() % num5 + 1;
				foreach (CapsuleMachineItemNew item2 in ItemList.OrderBy((CapsuleMachineItemNew o) => o.ItemMax))
				{
					num7 -= item2.ItemCount;
					if (num7 <= 0)
					{
						if (b >= item2.Level)
						{
							b = item2.Level;
							capsuleMachineItemNew = item2;
						}
						break;
					}
				}
				if (1 == b)
				{
					if (!(num6 > 0.5) || random.Next() % 2 == 0)
					{
						break;
					}
					b = 250;
				}
				else
				{
					num4--;
				}
			}
			lock (drawLock)
			{
				ret = 0;
				bool flag = false;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				int num11 = 0;
				try
				{
					using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
					{
						mySqlConnection.Open();
						using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
						mySqlCommand.Parameters.Clear();
						mySqlCommand.CommandType = CommandType.StoredProcedure;
						mySqlCommand.CommandText = "usp_capsuleMachineSelect";
						mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = RealMachineNum;
						mySqlCommand.Parameters.Add("machineKind", MySqlDbType.Int32).Value = RealMachineNumKind;
						mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = capsuleMachineItemNew.ItemNum;
						mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
						mySqlCommand.Parameters.Add("selectTime", MySqlDbType.DateTime).Value = DateTime.Now;
						mySqlCommand.Parameters.Add("resetCount", MySqlDbType.Int32).Value = ResetCount;
						using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
						mySqlDataReader.Read();
						ret = mySqlDataReader.GetInt32("retval");
						if (ret == 0)
						{
							num8 = mySqlDataReader.GetInt32("resultItemNum");
							num9 = mySqlDataReader.GetInt32("level");
							flag = mySqlDataReader.GetBoolean("isReset");
							num11 = mySqlDataReader.GetInt32("cost");
							num10 = Convert.ToInt32(mySqlDataReader["PriceType"]);
							ServerStatus.LBServerActor.Tell(new CapsuleMachineItemUpdate
							{
								MachineNum = RealMachineNum,
								ItemNum = num8,
								isReset = flag,
								isRotate = isRotate
							});
						}
						else if (ret == 5)
						{
							Log.Warning("Select When CapsuleMachine Resetting:{0} Nickname:{1}, itemnum:{2}", RealMachineNum, User.NickName, capsuleMachineItemNew.ItemNum);
							ret += 2;
						}
						else if (ret == 8 || ret == 9)
						{
							User.TRNeedUpdateFromDB = true;
							User.CashNeedUpdateFromDB = true;
						}
						else
						{
							Log.Error("CapsuleMachine:{3} Nickname:{0}, itemnum:{2}, RET: {1}", User.NickName, ret, capsuleMachineItemNew.ItemNum, RealMachineNum);
						}
					}
					if (ret == 0)
					{
						if (num9 < 3)
						{
							ServerStatus.LBServerActor.Tell(new CapsuleMachineNotice((byte)num9, RealMachineNum, num8, User.NickName));
						}
						switch (num10)
						{
						case 0:
							User.TR -= num11;
							break;
						case 1:
							User.Cash -= num11;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error("usp_capsuleMachineSelect Error: {0}, UserNum:{1}", ex.Message, User.UserNum);
					ret = 1;
				}
				return num8;
			}
		}

		public int UpdateCapsuleMachineInfo()
		{
			int num = 0;
			short num2 = 0;
			int num3 = 0;
			NestedDictionary<int, int, CapsuleMachineItemNew> nestedDictionary = new NestedDictionary<int, int, CapsuleMachineItemNew>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_capsuleMachineGetMachineInfo";
				mySqlCommand.Parameters.Add("machineNum", MySqlDbType.Int32).Value = RealMachineNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					num3 = Convert.ToInt32(mySqlDataReader["fdMachineNum"]);
					int num4 = Convert.ToInt32(mySqlDataReader["fdItemNum"]);
					num = Convert.ToInt32(mySqlDataReader["fdRotateGroupNum"]);
					num2 = Convert.ToInt16(mySqlDataReader["fdResetCount"]);
					CapsuleMachineItemNew value = new CapsuleMachineItemNew
					{
						ItemNum = num4,
						ItemCount = Convert.ToInt16(mySqlDataReader["fdItemCount"]),
						ItemMax = Convert.ToInt16(mySqlDataReader["fdItemMax"]),
						Level = Convert.ToByte(mySqlDataReader["fdLevel"]),
						ResetCount = num2,
						RotateGroupNum = num
					};
					nestedDictionary.Add(num3, num4, value);
				}
			}
			foreach (KeyValuePair<int, NestedDictionary<int, CapsuleMachineItemNew>> item in nestedDictionary)
			{
				CapsuleMachineData capsuleMachineData = new CapsuleMachineData();
				foreach (CapsuleMachineItemNew value2 in item.Value.Values)
				{
					capsuleMachineData.AddToItemList(value2);
				}
				capsuleMachineData.UpdateMachineInfo(item.Key);
				CapsuleMachineHolder.CapsuleMachineContainer[item.Key] = capsuleMachineData;
			}
			return num3;
		}
	}
}
