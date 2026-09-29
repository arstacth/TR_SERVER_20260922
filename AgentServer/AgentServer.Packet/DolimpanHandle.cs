using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class DolimpanHandle
	{
		public enum Dolimpan
		{
			eServerResult_OK_ACK = 0,
			eServerResult_UNKNOWN_FAILED_ACK = 2,
			eServerResult_DOLIMPAN_INVALID_ITEM = 355,
			eServerResult_DOLIMPAN_NO_RESET_COUPON = 356,
			eServerResult_DOLIMPAN_NO_CHARGE_COUPON = 357,
			eServerResult_DOLIMPAN_NO_PLAY_COUNT = 358,
			eServerResult_DOLIMPAN_NOT_ENOUGH_POINT = 359
		}

		private static readonly List<int> DolimpanDraw = new List<int>(8) { 7, 5, 3, 2, 1, -1, -1, 0 };

		public static void Handle_GetMyInfo(ClientConnection Client, byte last)
		{
			Dolimpan_Get(Client.CurrentAccount.UserNum, out var iDolimpanPoint, out var ret, out var result);
			Client.SendAsync(new DOLIMPAN_MY_INFO_ACK(result, iDolimpanPoint, ret, last));
		}

		public static void Handle_Play(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte idx = reader.ReadByte();
			int num = 0;
			bool bCharge = true;
			if (currentAccount.isDolimpanDoubleBonus())
			{
				num = currentAccount.getDolimpanBonusRate() * (from _ in DolimpanDraw.Take(7)
					orderby Guid.NewGuid()
					select _).FirstOrDefault();
				currentAccount.resetDolimpanDoubleBonus();
				bCharge = false;
			}
			else
			{
				num = DolimpanDraw.OrderBy((int _) => Guid.NewGuid()).FirstOrDefault();
				if (num == 0)
				{
					currentAccount.setDolimpanDoubleBonus();
				}
			}
			Dolimpan_AddPoint(currentAccount.UserNum, idx, num, bCharge, out var RemainPoint, out var DolimpanPoint, out var result);
			Client.SendAsync(new DOLIMPAN_TURN_BOARD_ACK(result, RemainPoint, DolimpanPoint, idx, num, currentAccount.isDolimpanDoubleBonus(), last));
		}

		public static void Handle_Reward(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte idx = reader.ReadByte();
			Dolimpan_ReceiveReward(currentAccount.UserNum, idx, out var ret, out var result);
			Client.SendAsync(new DOLIMPAN_RECEIVE_REWARD__ACK(result, ret, idx, last));
		}

		public static void Handle_Reset(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte idx = reader.ReadByte();
			int iCouponNum = reader.ReadLEInt32();
			Dolimpan_Reset(currentAccount.UserNum, idx, iCouponNum, out var ret, out var result);
			Client.SendAsync(new DOLIMPAN_RESET_ACK(result, ret, idx, iCouponNum, last));
		}

		public static void Handle_Charge(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			Dolimpan_Charge(iCouponNum: reader.ReadLEInt32(), UserNum: currentAccount.UserNum, DolimpanPoint: out var _, result: out var result);
			Client.SendAsync(new DOLIMPAN_USE_CHARGE_COUPON_ACK(result, last));
		}

		private static void Dolimpan_Get(int UserNum, out short iDolimpanPoint, out List<Tuple<int, int>> ret, out Dolimpan result)
		{
			result = Dolimpan.eServerResult_OK_ACK;
			ret = new List<Tuple<int, int>>();
			iDolimpanPoint = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Dolimpan_Get";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("noResultSet", MySqlDbType.Bit).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows && mySqlDataReader.Read())
				{
					iDolimpanPoint = mySqlDataReader.GetInt16("DolimpanPoint");
				}
				mySqlDataReader.NextResult();
				while (mySqlDataReader.Read())
				{
					int @int = mySqlDataReader.GetInt32("ItemNum");
					int int2 = mySqlDataReader.GetInt32("Point");
					ret.Add(new Tuple<int, int>(@int, int2));
				}
			}
			catch (MySqlException ex)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_Get Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_Get Error:{0}", ex2.ToString());
			}
		}

		private static void Dolimpan_AddPoint(int UserNum, byte idx, int iPoint, bool bCharge, out int RemainPoint, out short DolimpanPoint, out Dolimpan result)
		{
			result = Dolimpan.eServerResult_OK_ACK;
			RemainPoint = 0;
			DolimpanPoint = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Dolimpan_AddPoint";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("idx", MySqlDbType.Byte).Value = idx;
				mySqlCommand.Parameters.Add("point", MySqlDbType.Int32).Value = iPoint;
				mySqlCommand.Parameters.Add("bCharge", MySqlDbType.Int32).Value = bCharge;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows && mySqlDataReader.Read())
				{
					RemainPoint = Convert.ToInt32(mySqlDataReader["RemainPoint"]);
					DolimpanPoint = Convert.ToInt16(mySqlDataReader["DolimpanPoint"]);
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("invalid item"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_INVALID_ITEM;
				}
				else if (ex.Message.Contains("not enough play count"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_NO_PLAY_COUNT;
				}
				Log.Error("usp_Dolimpan_AddPoint Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_AddPoint Error:{0}", ex2.ToString());
			}
		}

		private static void Dolimpan_ReceiveReward(int UserNum, byte idx, out Tuple<int, int> ret, out Dolimpan result)
		{
			result = Dolimpan.eServerResult_OK_ACK;
			ret = null;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Dolimpan_ReceiveReward";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("idx", MySqlDbType.Byte).Value = idx;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.Read())
				{
					ret = new Tuple<int, int>(mySqlDataReader.GetInt32("NewItemNum"), mySqlDataReader.GetInt32("RewardItemNum"));
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("not enough point"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_NOT_ENOUGH_POINT;
				}
				Log.Error("usp_Dolimpan_ReceiveReward Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_ReceiveReward Error:{0}", ex2.ToString());
			}
		}

		private static void Dolimpan_Reset(int UserNum, byte idx, int iCouponNum, out Tuple<int, int> ret, out Dolimpan result)
		{
			result = Dolimpan.eServerResult_OK_ACK;
			ret = null;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Dolimpan_Reset";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("idx", MySqlDbType.Byte).Value = idx;
				mySqlCommand.Parameters.Add("resetCouponNum", MySqlDbType.Int32).Value = iCouponNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.Read())
				{
					ret = new Tuple<int, int>(mySqlDataReader.GetInt32("BeforeItemNum"), mySqlDataReader.GetInt32("NewItemNum"));
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("invalid item"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_INVALID_ITEM;
				}
				else if (ex.Message.Contains("no coupon"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_NO_RESET_COUPON;
				}
				Log.Error("usp_Dolimpan_Reset Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_Reset Error:{0}", ex2.ToString());
			}
		}

		private static void Dolimpan_Charge(int UserNum, int iCouponNum, out short DolimpanPoint, out Dolimpan result)
		{
			result = Dolimpan.eServerResult_OK_ACK;
			DolimpanPoint = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Dolimpan_Charge";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("couponNum", MySqlDbType.Int32).Value = iCouponNum;
				mySqlCommand.Parameters.Add("noRecordSet", MySqlDbType.Int32).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.Read())
				{
					DolimpanPoint = Convert.ToInt16(mySqlDataReader["DolimpanPoint"]);
				}
			}
			catch (MySqlException ex)
			{
				if (ex.Message.Contains("invalid dolimpan charge coupon"))
				{
					result = Dolimpan.eServerResult_DOLIMPAN_NO_CHARGE_COUPON;
				}
				Log.Error("usp_Dolimpan_Charge Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				result = Dolimpan.eServerResult_UNKNOWN_FAILED_ACK;
				Log.Error("usp_Dolimpan_Charge Error:{0}", ex2.ToString());
			}
		}
	}
}
