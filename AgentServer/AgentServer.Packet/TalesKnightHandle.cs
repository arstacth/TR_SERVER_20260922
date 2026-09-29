using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class TalesKnightHandle
	{
		public static void Handle_GetMyTalesKnightUnitInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			GetMyTalesKnightUnitInfo(Client.CurrentAccount.UserNum, out var Infos);
			Client.SendAsync(new GetMyTalesKnightUnitInfo_Ack(Infos, last));
		}

		public static void Handle_GetMyTalesKnightsGroupName(ClientConnection Client, PacketReader reader, byte last)
		{
			GetMyTalesKnightsGroupName(Client.CurrentAccount, out var Infos);
			Client.SendAsync(new GetMyTalesKnightsGroupName_Ack(Infos, last));
		}

		public static void Handle_GetMyTalesKnightsGroupInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			GetMyTalesKnightsGroupInfo(Client.CurrentAccount, out var Infos);
			Client.SendAsync(new GetMyTalesKnightsGroupInfo_Ack(Infos, last));
		}

		public static void Handle_UpdateTalesKnightsGroup(ClientConnection Client, PacketReader reader, byte last)
		{
			int groupNum = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			string text = string.Empty;
			for (int i = 0; i < num; i++)
			{
				int num2 = reader.ReadLEInt32();
				reader.ReadLEInt16();
				short num3 = reader.ReadLEInt16();
				text += $"T{num3}:{num2},";
			}
			if (UpdateTalesKnightsGroup(Client.CurrentAccount, groupNum, text, out var Infos, out var GPinfo))
			{
				Client.SendAsync(new UpdateTalesKnightsGroup_Ack(groupNum, Infos, last));
				Client.SendAsync(new UpdateTalesKnightsGroup_Name_Ack(GPinfo, last));
			}
		}

		public static void Handle_UpdateTalesKnightsGroup_Name(ClientConnection Client, PacketReader reader, byte last)
		{
			int groupNum = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string groupName = reader.ReadBig5StringSafe(fixedLength);
			UpdateTalesKnightsGroup_Name(Client.CurrentAccount, groupNum, groupName, out var GPinfo);
			Client.SendAsync(new UpdateTalesKnightsGroup_Name_Ack(GPinfo, last));
		}

		public static void Handle_HasTalesKnightUnit(ClientConnection Client, PacketReader reader, byte last)
		{
			int itemNum = reader.ReadLEInt32();
			hasTalesKnightUnit(Client.CurrentAccount.UserNum, itemNum, out var Infos);
			Client.SendAsync(new hasTalesKnightUnit_Ack(Infos, last));
		}

		public static void Handle_GetTalesKnightStageInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			GetTalesKnightStageInfo(out var StageInfos);
			Client.SendAsync(new TalesKnightStageInfo_Ack(StageInfos, last));
		}

		public static void Handle_TalesKnightStageEnter(ClientConnection Client, PacketReader reader, byte last)
		{
			int stageGroupNum = reader.ReadLEInt32();
			int groupNum = reader.ReadLEInt32();
			int stageNum = reader.ReadLEInt32();
			if (TalesKnightStageEnter(Client.CurrentAccount, groupNum, stageGroupNum, stageNum, out var Infos))
			{
				Client.SendAsync(new TalesKnightStageEnter_Ack(Infos, last));
			}
		}

		public static void Handle_TalesKnights_AttackCheck(ClientConnection Client, PacketReader reader, byte last)
		{
			int num = reader.ReadLEInt32();
			int groupNum = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			Account currentAccount = Client.CurrentAccount;
			if (!TalesKnights_AttackCheck(currentAccount, groupNum, out var Infos))
			{
				return;
			}
			List<int> DeadList = new List<int>();
			string text = string.Empty;
			foreach (TalesKnightsUnitStatus item2 in Infos)
			{
				text += $"{item2.UnitNum},";
				if (new Random(Guid.NewGuid().GetHashCode()).Next(0, 100) - item2.UnitHealth >= 80)
				{
					DeadList.Add(item2.UnitNum);
				}
			}
			Client.SendAsync(new TalesKnights_AttackCheck_Dead_Ack(groupNum, num2, DeadList, last));
			Client.SendAsync(new TalesKnights_AttackCheck_Unk_Ack(groupNum, last));
			if (TalesKnightAddExp(currentAccount, 59L, text, out var ExpInfos))
			{
				Client.SendAsync(new TalesKnights_AttackCheck_AddExp_Ack(groupNum, num2, ExpInfos, last));
			}
			string text2 = string.Empty;
			List<TalesKnights_AttackReward> list = new List<TalesKnights_AttackReward>();
			foreach (TalesKnightsUnitStatus item3 in Infos.Where((TalesKnightsUnitStatus w) => !DeadList.Contains(w.UnitNum)))
			{
				if (new Random(Guid.NewGuid().GetHashCode()).Next(0, 100) >= 50)
				{
					int key = ((num != 4) ? 1 : (5 * num2 - 4));
					if (TalesKnightHolder.TalesKnightStageRewardDetail.TryGetValue(num, key, out var value))
					{
						int rewardItemNum = value.NextWithReplacement().RewardItemNum;
						TalesKnights_AttackReward item = new TalesKnights_AttackReward
						{
							RewardItem = rewardItemNum,
							UnitNum = item3.UnitNum
						};
						text2 += $"{rewardItemNum},";
						list.Add(item);
					}
				}
			}
			if (text2 != string.Empty)
			{
				TalesKnightReward(currentAccount.UserNum, text2);
			}
			Client.SendAsync(new TalesKnights_AttackCheck_RewardResult_Ack(num, groupNum, num2, list, last));
		}

		public static void Handle_TalesKnightsRewardReceive(ClientConnection Client, PacketReader reader, byte last)
		{
			int itemNum = reader.ReadLEInt32();
			Account currentAccount = Client.CurrentAccount;
			int num = TalesKnightsRewardReceive(currentAccount.UserNum, itemNum);
			if (num <= 0)
			{
				return;
			}
			if (ShopItemTable.getRealItemDataFromItemDescNum(num, out var itemData))
			{
				if (itemData.isPosition(eFuncItemPosition.eFuncItemPosition_ADD_TR))
				{
					currentAccount.TR += (int)itemData.m_mapAttr[37];
				}
				else if (itemData.isPosition(eFuncItemPosition.eFuncItemPosition_ADD_EXP))
				{
					currentAccount.Exp += (int)itemData.m_mapAttr[38];
				}
			}
			Client.SendAsync(new TalesKnightsRewardReceive_Ack(num, last));
		}

		public static void Handle_MyKnightsCallComeBack(ClientConnection Client, PacketReader reader, byte last)
		{
			int groupNum = reader.ReadLEInt32();
			if (MyKnightsCallComeBack(Client.CurrentAccount, groupNum, out var CompleteGroupNum))
			{
				Client.SendAsync(new MyKnightsCallComeBack_Ack(CompleteGroupNum, last));
			}
		}

		public static void Handle_TalesKnightsAdd_MaxLevel(ClientConnection Client, PacketReader reader, byte last)
		{
			int unitNum = reader.ReadLEInt32();
			Account currentAccount = Client.CurrentAccount;
			if (TalesKnightsAdd_MaxLevel(RandValue: new Random().Next(0, 100), UserNum: currentAccount.UserNum, UnitNum: unitNum, Info: out var Info))
			{
				Client.SendAsync(new TalesKnightsAdd_MaxLevel_Ack(Info, last));
			}
		}

		public static void Handle_TalesKnightsUseExpUpItem(ClientConnection Client, PacketReader reader, byte last)
		{
			int unitNum = reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			if (TalesKnightsUseExpUpItem(Client.CurrentAccount.UserNum, unitNum, num, out var ExpInfo))
			{
				Client.SendAsync(new TalesKnightsUseExpUpItem_Ack(num, ExpInfo, last));
			}
		}

		private static void GetMyTalesKnightUnitInfo(int UserNum, out List<TalesKnightUnitInfo> Infos)
		{
			Infos = new List<TalesKnightUnitInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_GetMyTalesKnightUnitInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TalesKnightUnitInfo item = new TalesKnightUnitInfo
					{
						UnitNum = mySqlDataReader.GetInt32("UnitNum"),
						UnitLevel = mySqlDataReader.GetInt16("UnitLevel"),
						UnitMaxLevel = mySqlDataReader.GetInt16("UnitMaxLevel"),
						UnitExp = mySqlDataReader.GetInt64("UnitExp"),
						UnitGotDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("UnitGotDate")),
						UnitReinForceCount = mySqlDataReader.GetInt32("UnitReinForceCount")
					};
					Infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_GetMyTalesKnightUnitInfo error: {0}", ex.Message);
			}
		}

		private static void GetMyTalesKnightsGroupName(Account User, out List<TalesKnightsGroupName> Infos)
		{
			Infos = new List<TalesKnightsGroupName>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_GetMyTalesKnightsGroupName";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TalesKnightsGroupName item = new TalesKnightsGroupName
					{
						OrderNumber = mySqlDataReader.GetInt16("OrderNumber"),
						OrderName = mySqlDataReader.GetString("OrderName"),
						StageGroupNum = mySqlDataReader.GetInt32("StageGroupNum"),
						CurStageNum = mySqlDataReader.GetInt32("CurStageNum"),
						RewardTime = mySqlDataReader.GetFloat("RewardTime"),
						AccTime = mySqlDataReader.GetFloat("AccTime"),
						Quests = mySqlDataReader.GetString("Quests")
					};
					Infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_GetMyTalesKnightsGroupName error: {0}", ex.Message);
			}
		}

		private static void GetMyTalesKnightsGroupInfo(Account User, out List<TalesKnightsGroupInfo> Infos)
		{
			Infos = new List<TalesKnightsGroupInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_GetMyTalesKnightsGroupInfo";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TalesKnightsGroupInfo item = new TalesKnightsGroupInfo
					{
						OrderNumber = mySqlDataReader.GetInt16("OrderNumber"),
						UnitSlot = mySqlDataReader.GetInt16("UnitSlot"),
						UnitNum = mySqlDataReader.GetInt32("UnitNum")
					};
					Infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_GetMyTalesKnightsGroupName error: {0}", ex.Message);
			}
		}

		private static bool UpdateTalesKnightsGroup(Account User, int GroupNum, string UnitInfo, out List<TalesKnightsGroupInfo> Infos, out TalesKnightsGroupName GPinfo)
		{
			Infos = new List<TalesKnightsGroupInfo>();
			GPinfo = new TalesKnightsGroupName();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_UpdateTalesKnightsGroup";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("GroupNum", MySqlDbType.Int16).Value = (short)GroupNum;
					mySqlCommand.Parameters.Add("UnitInfo", MySqlDbType.VarString).Value = UnitInfo;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							TalesKnightsGroupInfo item = new TalesKnightsGroupInfo
							{
								OrderNumber = mySqlDataReader.GetInt16("OrderNumber"),
								UnitSlot = mySqlDataReader.GetInt16("UnitSlot"),
								UnitNum = mySqlDataReader.GetInt32("UnitNum")
							};
							Infos.Add(item);
						}
						mySqlDataReader.NextResult();
						mySqlDataReader.Read();
						GPinfo.OrderNumber = mySqlDataReader.GetInt16("GroupNum");
						GPinfo.OrderName = mySqlDataReader.GetString("GroupName");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_UpdateTalesKnightsGroup error: {0}", ex.Message);
				return false;
			}
		}

		private static void UpdateTalesKnightsGroup_Name(Account User, int GroupNum, string GroupName, out TalesKnightsGroupName GPinfo)
		{
			GPinfo = new TalesKnightsGroupName();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_UpdateTalesKnightsGroup_Name";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("GroupNum", MySqlDbType.Int16).Value = GroupNum;
				mySqlCommand.Parameters.Add("GroupName", MySqlDbType.VarString).Value = GroupName;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				mySqlDataReader.Read();
				GPinfo.OrderNumber = mySqlDataReader.GetInt16("GroupNum");
				GPinfo.OrderName = mySqlDataReader.GetString("GroupName");
			}
			catch (Exception ex)
			{
				Log.Error("usp_UpdateTalesKnightsGroup_Name error: {0}", ex.Message);
			}
		}

		private static void hasTalesKnightUnit(int UserNum, int ItemNum, out List<TalesKnightUnitInfo> Infos)
		{
			Infos = new List<TalesKnightUnitInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_User_hasTalesKnightUnit";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TalesKnightUnitInfo item = new TalesKnightUnitInfo
					{
						UnitNum = mySqlDataReader.GetInt32("UnitNum"),
						UnitLevel = mySqlDataReader.GetInt16("UnitLevel"),
						UnitMaxLevel = mySqlDataReader.GetInt16("UnitMaxLevel"),
						UnitExp = mySqlDataReader.GetInt64("UnitExp"),
						UnitGotDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("UnitGotDate")),
						UnitReinForceCount = mySqlDataReader.GetInt32("ReinForceCount")
					};
					Infos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_User_hasTalesKnightUnit error: {0}", ex.Message);
			}
		}

		private static void GetTalesKnightStageInfo(out List<TalesKnightStageInfo> StageInfos)
		{
			StageInfos = new List<TalesKnightStageInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getTalesKnightStageInfo";
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					TalesKnightStageInfo item = new TalesKnightStageInfo
					{
						StageGroupNum = mySqlDataReader.GetInt32("StageGroupNum"),
						StageNum = mySqlDataReader.GetInt32("StageNum"),
						HP = mySqlDataReader.GetInt32("HP"),
						Attribute = mySqlDataReader.GetInt32("Attribute"),
						StatusType = mySqlDataReader.GetInt32("StatusType")
					};
					StageInfos.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getTalesKnightStageInfo error: {0}", ex.Message);
			}
		}

		private static bool TalesKnightStageEnter(Account User, int GroupNum, int StageGroupNum, int StageNum, out List<TalesKnightsGroupName> Infos)
		{
			Infos = new List<TalesKnightsGroupName>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TalesKnightStageEnter";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("GroupNum", MySqlDbType.Int16).Value = (short)GroupNum;
					mySqlCommand.Parameters.Add("StageGroupNum", MySqlDbType.Int32).Value = StageGroupNum;
					mySqlCommand.Parameters.Add("StageNum", MySqlDbType.Int32).Value = StageNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						mySqlDataReader.GetInt32("StageAttribute");
						mySqlDataReader.NextResult();
						while (mySqlDataReader.Read())
						{
							TalesKnightsGroupName item = new TalesKnightsGroupName
							{
								OrderNumber = mySqlDataReader.GetInt16("OrderNumber"),
								OrderName = mySqlDataReader.GetString("OrderName"),
								StageGroupNum = mySqlDataReader.GetInt32("StageGroupNum"),
								CurStageNum = mySqlDataReader.GetInt32("CurStageNum"),
								RewardTime = mySqlDataReader.GetFloat("RewardTime"),
								AccTime = mySqlDataReader.GetFloat("AccTime"),
								Quests = mySqlDataReader.GetString("Quests")
							};
							Infos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightStageEnter error: {0}", ex.Message);
				return false;
			}
		}

		private static bool MyKnightsCallComeBack(Account User, int GroupNum, out int CompleteGroupNum)
		{
			CompleteGroupNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_MyKnightsCallComeBack";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("GroupNum", MySqlDbType.Int32).Value = GroupNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						CompleteGroupNum = mySqlDataReader.GetInt32("CompleteGroupNum");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_MyKnightsCallComeBack error: {0}", ex.Message);
				return false;
			}
		}

		private static bool TalesKnights_AttackCheck(Account User, int GroupNum, out List<TalesKnightsUnitStatus> Infos)
		{
			Infos = new List<TalesKnightsUnitStatus>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TalesKnights_AttackCheck";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("GroupNum", MySqlDbType.Int16).Value = GroupNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							TalesKnightsUnitStatus item = new TalesKnightsUnitStatus
							{
								UnitNum = mySqlDataReader.GetInt32("UnitNum"),
								UnitLevel = mySqlDataReader.GetInt16("UnitLevel"),
								UnitMaxLevel = mySqlDataReader.GetInt16("UnitMaxLevel"),
								UnitExp = mySqlDataReader.GetInt64("UnitExp"),
								UnitPower = mySqlDataReader.GetInt32("UnitPower"),
								UnitMana = mySqlDataReader.GetInt32("UnitMana"),
								UnitDefence = mySqlDataReader.GetInt32("UnitDefence"),
								UnitHealth = mySqlDataReader.GetInt32("UnitHealth"),
								UnitLuck = mySqlDataReader.GetInt32("UnitLuck"),
								UnitGrade = mySqlDataReader.GetInt32("UnitGrade"),
								UnitGotDate = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("UnitGotDate")),
								Attribute = mySqlDataReader.GetInt32("Attribute")
							};
							Infos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnights_AttackCheck error: {0}", ex.Message);
				return false;
			}
		}

		private static void TalesKnightReward(int UserNum, string RewardList)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_TalesKnightReward";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("RewardList", MySqlDbType.VarString).Value = RewardList;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightReward error: {0}", ex.Message);
			}
		}

		private static int TalesKnightsRewardReceive(int UserNum, int ItemNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_TalesKnightsRewardReceive";
				mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = ItemNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				return mySqlDataReader.GetInt32("RefreshItemNum");
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightsRewardReceive error: {0}", ex.Message);
				return 0;
			}
		}

		private static bool TalesKnightAddExp(Account User, long Exp, string UnitList, out List<TalesKnightsUnitExpInfo> ExpInfos)
		{
			ExpInfos = new List<TalesKnightsUnitExpInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TalesKnightAddExp";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("Exp", MySqlDbType.Int64).Value = Exp;
					mySqlCommand.Parameters.Add("UnitList", MySqlDbType.VarString).Value = UnitList;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							TalesKnightsUnitExpInfo item = new TalesKnightsUnitExpInfo
							{
								UnitNum = mySqlDataReader.GetInt32("UnitNum"),
								BeforeExp = mySqlDataReader.GetInt64("BeforeExp"),
								BeforeLevel = mySqlDataReader.GetInt16("BeforeLevel"),
								NowExp = mySqlDataReader.GetInt64("NowExp"),
								NowLevel = mySqlDataReader.GetInt16("NowLevel"),
								MaxLevel = mySqlDataReader.GetInt16("MaxLevel")
							};
							ExpInfos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightAddExp error: {0}", ex.Message);
				return false;
			}
		}

		private static bool TalesKnightsAdd_MaxLevel(int UserNum, int UnitNum, int RandValue, out TalesKnightsAdd_MaxLevelInfo Info)
		{
			Info = new TalesKnightsAdd_MaxLevelInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TalesKnightsAdd_MaxLevel";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("UnitNum", MySqlDbType.Int32).Value = UnitNum;
					mySqlCommand.Parameters.Add("RandValue", MySqlDbType.Int32).Value = RandValue;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						Info.UnitNum = mySqlDataReader.GetInt32("UnitNum");
						Info.MaxLevel = mySqlDataReader.GetInt32("MaxLevel");
						Info.LevelUpScucess = mySqlDataReader.GetBoolean("LevelUpScucess");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightsAdd_MaxLevel error: {0}", ex.Message);
				return false;
			}
		}

		private static bool TalesKnightsUseExpUpItem(int UserNum, int UnitNum, int UseItemNum, out TalesKnightsUnitExpInfo ExpInfo)
		{
			ExpInfo = new TalesKnightsUnitExpInfo();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_TalesKnightsUseExpUpItem";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("UnitNum", MySqlDbType.Int32).Value = UnitNum;
					mySqlCommand.Parameters.Add("UseItemNum", MySqlDbType.Int32).Value = UseItemNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						ExpInfo.UnitNum = mySqlDataReader.GetInt32("UnitNum");
						ExpInfo.BeforeExp = mySqlDataReader.GetInt64("BeforeExp");
						ExpInfo.BeforeLevel = mySqlDataReader.GetInt16("BeforeLevel");
						ExpInfo.NowExp = mySqlDataReader.GetInt64("NowExp");
						ExpInfo.NowLevel = mySqlDataReader.GetInt16("NowLevel");
						ExpInfo.MaxLevel = mySqlDataReader.GetInt16("MaxLevel");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_TalesKnightsUseExpUpItem error: {0}", ex.Message);
				return false;
			}
		}
	}
}
