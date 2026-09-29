using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Park;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class EventPickBoardHandle
	{
		public static void Handle_GetEventPickBoardInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (EventPickBoardHolder.EventPickBoardInfo.TryGetValue(num, out var value) && DateTime.Now >= value.StartDateTime && DateTime.Now < value.EndDateTime)
			{
				if (eventPickBoardGetMyState(currentAccount, num, out var PickBoardStep, out var RemainCount, out var infos))
				{
					currentAccount.EventPickBoardStatus = 1;
					Client.SendAsync(new EventPickBoardUserInfo_ACK(num, value, PickBoardStep, RemainCount, infos, last));
				}
				else
				{
					Client.SendAsync(new EventPickBoardUserInfoFail_ACK(num, 2, last));
				}
			}
			else
			{
				Client.SendAsync(new EventPickBoardUserInfoFail_ACK(num, 2, last));
			}
		}

		public static void Handle_EventPickBoard_Use(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte PickBoardStep = reader.ReadByte();
			byte orderNum = reader.ReadByte();
			EventPickBoardInfo value;
			if (currentAccount.CuerrentPickBoardNum != num)
			{
				Client.SendAsync(new EventPickBoardUseFail_ACK(num, 9, last));
			}
			else if (currentAccount.CuerrentPickBoardRemain <= 0)
			{
				Client.SendAsync(new EventPickBoardUseFail_ACK(num, 11, last));
			}
			else if (currentAccount.EventPickBoardStatus == 1 && EventPickBoardHolder.EventPickBoardInfo.TryGetValue(num, out value) && DateTime.Now >= value.StartDateTime && DateTime.Now < value.EndDateTime)
			{
				if (EventPickBoardHolder.EventPickBoardBasicInfo.TryGetValue(num, out var value2) && value2.ContainsKey(PickBoardStep))
				{
					IOrderedEnumerable<byte> source = from w in value2.Keys
						where w > PickBoardStep
						select w into o
						orderby o
						select o;
					byte b = PickBoardStep;
					if (source.Count() > 0)
					{
						b = source.FirstOrDefault();
					}
					byte b2 = 2;
					bool flag = false;
					if (!currentAccount.CuerrentPickBoardIsReward1St)
					{
						float num2 = value2[PickBoardStep];
						if (new Random(Guid.NewGuid().GetHashCode()).NextDouble() * 100.0 <= (double)num2 || currentAccount.CuerrentPickBoardRemain == 1)
						{
							b2 = 1;
							flag = true;
						}
					}
					if (value.StepResetType == 1 && b2 == 1 && PickBoardStep == value.LastStep)
					{
						flag = false;
					}
					if (eventPickBoardUse(currentAccount, num, PickBoardStep, orderNum, b2, b, flag, out var ret, out var info))
					{
						byte nextStep = (flag ? b : PickBoardStep);
						currentAccount.EventPickBoardStatus = 2;
						Client.SendAsync(new EventPickBoardUse_ACK(num, PickBoardStep, orderNum, nextStep, flag, info, last));
					}
					else
					{
						Client.SendAsync(new EventPickBoardUseFail_ACK(num, ret, last));
					}
				}
				else
				{
					Client.SendAsync(new EventPickBoardUseFail_ACK(num, 11, last));
				}
			}
			else
			{
				Client.SendAsync(new EventPickBoardUseFail_ACK(num, 9, last));
			}
		}

		public static void Handle_EventPickBoard_Give(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			byte pickBoardStep = reader.ReadByte();
			byte orderNum = reader.ReadByte();
			int fixedLength = reader.ReadLEInt16();
			string receiveNickname = reader.ReadBig5StringSafe(fixedLength);
			fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			ExchangeItemInfo info;
			if (currentAccount.CuerrentPickBoardNum != num)
			{
				Client.SendAsync(new EventPickBoardGiveFail_ACK(9, last));
			}
			else if (currentAccount.EventPickBoardStatus == 2 && eventPickBoardGive(currentAccount, num, pickBoardStep, orderNum, receiveNickname, empty, out info))
			{
				currentAccount.EventPickBoardStatus = 1;
				Client.SendAsync(new EventPickBoardGive_ACK(info, last));
			}
			else
			{
				Client.SendAsync(new EventPickBoardGiveFail_ACK(8, last));
			}
		}

		public static void Handle_GetHuMongPickBoardInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			if (EventPickBoardHolder.HuMongPickBoardContainer.TryGetValue(key, out var value))
			{
				Client.SendAsync(new GetHuMongPickBoardInfo(value, last));
			}
			else
			{
				Client.SendAsync(new GetHuMongPickBoardInfo(eServerResult.eServerResult_HUMONGPICKBOARD_INVALID_BOARD, last));
			}
		}

		public static void Handle_HuMongPickBoard_PickItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int key = reader.ReadLEInt32();
			short num = reader.ReadLEInt16();
			if (EventPickBoardHolder.HuMongPickBoardContainer.TryGetValue(key, out var value) && num <= value.PickInfo.Length)
			{
				if (value.LastResetTime < DateTime.Now && !value.PickInfo[num - 1])
				{
					int AdditionRewardItemNum;
					int ret;
					byte rank = value.PickItem(currentAccount, num, out AdditionRewardItemNum, out ret);
					if (ret == 0)
					{
						Client.SendAsync(new HuMongPickBoard_PickItem_OK(num, rank, AdditionRewardItemNum, last));
					}
					else
					{
						Client.SendAsync(new HuMongPickBoard_PickItem_Fail(eServerResult.eServerResult_DB_FAILED_ACK, last));
					}
				}
				else
				{
					Client.SendAsync(new HuMongPickBoard_PickItem_Fail(eServerResult.eServerResult_HUMONGPICKBOARD_ALREADY_PICKED, last));
				}
			}
			else
			{
				Client.SendAsync(new HuMongPickBoard_PickItem_Fail(eServerResult.eServerResult_HUMONGPICKBOARD_INVALID_BOARD, last));
			}
		}

		public static void Handle_HuMongPickBoard_GiveItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte b = reader.ReadByte();
			short num = reader.ReadLEInt16();
			string text = string.Empty;
			if (num > 0)
			{
				text = reader.ReadBig5StringSafe(num);
			}
			string memo = string.Empty;
			num = reader.ReadLEInt16();
			if (num > 0)
			{
				memo = reader.ReadBig5StringSafe(num);
			}
			if (b == 2 && text == currentAccount.NickName)
			{
				Client.SendAsync(new HuMongPickBoard_GiveItem_Fail(eServerResult.eServerResult_HUMONGPICKBOARD_INVALID_TARGET, last));
				return;
			}
			HuMongPickBoard_GiveItem(currentAccount.UserNum, text, b, memo, out var result, out var PickID, out var itemNum, out var AdditionRewardItemNum);
			if (result == eServerResult.eServerResult_OK_ACK)
			{
				Client.SendAsync(new HuMongPickBoard_GiveItem_OK(PickID, itemNum, AdditionRewardItemNum, last));
			}
			else
			{
				Client.SendAsync(new HuMongPickBoard_GiveItem_Fail(result, last));
			}
		}

		private static bool eventPickBoardGetMyState(Account User, int pickBoardNum, out byte PickBoardStep, out int RemainCount, out Dictionary<byte, ExchangeItemInfo> infos)
		{
			PickBoardStep = 0;
			RemainCount = 0;
			infos = new Dictionary<byte, ExchangeItemInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_eventPickBoardGetMyState";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pickBoardNum", MySqlDbType.Int32).Value = pickBoardNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					User.CuerrentPickBoardNum = pickBoardNum;
					PickBoardStep = mySqlDataReader.GetByte("PickBoardStep");
					User.CuerrentPickBoardRemain = (RemainCount = mySqlDataReader.GetInt32("RemainCount"));
					User.CuerrentPickBoardIsReward1St = mySqlDataReader.GetBoolean("IsReward1St");
					mySqlDataReader.NextResult();
					while (mySqlDataReader.Read())
					{
						byte @byte = mySqlDataReader.GetByte("Order");
						ExchangeItemInfo value = new ExchangeItemInfo
						{
							type = Convert.ToInt32(mySqlDataReader["RewardType"]),
							id = Convert.ToInt32(mySqlDataReader["RewardID"]),
							count = Convert.ToInt32(mySqlDataReader["RewardCount"])
						};
						infos.Add(@byte, value);
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_eventPickBoardGetMyState Error:{0}", ex.Message);
			}
			return false;
		}

		private static bool eventPickBoardUse(Account User, int pickBoardNum, byte PickBoardStep, byte OrderNum, byte nRewardStep, byte nNextStep, bool isReset, out int ret, out ExchangeItemInfo info)
		{
			ret = 10;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_eventPickBoardUse";
				mySqlCommand.Parameters.Add("nUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nPickBoardNum", MySqlDbType.Int32).Value = pickBoardNum;
				mySqlCommand.Parameters.Add("nPickBoardStep", MySqlDbType.Byte).Value = PickBoardStep;
				mySqlCommand.Parameters.Add("nOrderNum", MySqlDbType.Byte).Value = OrderNum;
				mySqlCommand.Parameters.Add("nRewardStep", MySqlDbType.Byte).Value = nRewardStep;
				mySqlCommand.Parameters.Add("nNextStep", MySqlDbType.Byte).Value = nNextStep;
				mySqlCommand.Parameters.Add("isReset", MySqlDbType.Byte).Value = isReset;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					ret = mySqlDataReader.GetInt32("ret");
					if (ret == 0)
					{
						User.CuerrentPickBoardIsReward1St = Convert.ToBoolean(mySqlDataReader["IsReward1St"]);
						info = new ExchangeItemInfo
						{
							type = Convert.ToInt32(mySqlDataReader["RewardType"]),
							id = Convert.ToInt32(mySqlDataReader["RewardID"]),
							count = Convert.ToInt32(mySqlDataReader["RewardCount"])
						};
						if (nRewardStep == 1)
						{
							User.CuerrentPickBoardRemain = 50;
						}
						else
						{
							User.CuerrentPickBoardRemain--;
						}
						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_eventPickBoardUse Error:{0}", ex.Message);
			}
			info = null;
			return false;
		}

		private static bool eventPickBoardGive(Account User, int pickBoardNum, byte PickBoardStep, byte OrderNum, string receiveNickname, string memo, out ExchangeItemInfo info)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_eventPickBoardGive";
				mySqlCommand.Parameters.Add("nUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("nPickBoardNum", MySqlDbType.Int32).Value = pickBoardNum;
				mySqlCommand.Parameters.Add("nPickBoardStep", MySqlDbType.Byte).Value = PickBoardStep;
				mySqlCommand.Parameters.Add("nOrderNum", MySqlDbType.Byte).Value = OrderNum;
				mySqlCommand.Parameters.Add("sendNickname", MySqlDbType.VarString).Value = User.NickName;
				mySqlCommand.Parameters.Add("receiveNickname", MySqlDbType.VarString).Value = receiveNickname;
				mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					info = new ExchangeItemInfo
					{
						type = Convert.ToInt32(mySqlDataReader["RewardType"]),
						id = Convert.ToInt32(mySqlDataReader["RewardID"]),
						count = Convert.ToInt32(mySqlDataReader["RewardCount"])
					};
					if (info.type == 200)
					{
						User.TR += info.count;
					}
					else if (info.type == 500)
					{
						User.EXPNeedUpdateFromDB = true;
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_eventPickBoardGive Error:{0}", ex.Message);
			}
			info = null;
			return false;
		}

		private static void HuMongPickBoard_GiveItem(int UserNum, string targetNickname, int action, string memo, out eServerResult result, out short PickID, out int itemNum, out int AdditionRewardItemNum)
		{
			PickID = 0;
			itemNum = 0;
			AdditionRewardItemNum = 0;
			result = eServerResult.eServerResult_OK_ACK;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_HuMongPickBoard_GiveItem");
				mySqlCommandHelper.AddParamInt("UserNum", UserNum);
				mySqlCommandHelper.AddParamVarString("targetNickname", targetNickname);
				mySqlCommandHelper.AddParamInt("action", action);
				mySqlCommandHelper.AddParamVarString("memo", targetNickname);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					PickID = mySqlCommandHelper.GetShort("PickID");
					itemNum = mySqlCommandHelper.GetInt("itemNum");
					AdditionRewardItemNum = mySqlCommandHelper.GetInt("AdditionRewardItemNum");
				}
			}
			catch (MySqlException ex)
			{
				result = eServerResult.eServerResult_DB_FAILED_ACK;
				if (ex.Message.Contains("no item"))
				{
					result = eServerResult.eServerResult_HUMONGPICKBOARD_NO_ITEM;
				}
				else if (ex.Message.Contains("invalid target nickname"))
				{
					result = eServerResult.eServerResult_HUMONGPICKBOARD_INVALID_TARGET;
				}
				else if (ex.Message.Contains("Not Enough Capacity"))
				{
					result = eServerResult.eServerResult_HUMONGPICKBOARD_NOT_ENOUGH_STORAGE;
				}
				else if (ex.Message.Contains("can not gift item"))
				{
					result = eServerResult.eServerResult_HUMONGPICKBOARD_NOT_GIVE_ITEM;
				}
				else
				{
					result = eServerResult.eServerResult_DB_FAILED_ACK;
				}
				Log.Error("usp_HuMongPickBoard_GiveItem Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = eServerResult.eServerResult_DB_FAILED_ACK;
				Log.Error("usp_HuMongPickBoard_GiveItem Error:{0}", ex2.Message);
			}
		}
	}
}
