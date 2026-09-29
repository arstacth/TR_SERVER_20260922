using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Park;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class ParkHandle
	{
		public static void Handle_GetMachineInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int machineID = reader.ReadLEInt32();
			ResolveCapsuleMachine(num, out CapsuleMachineData value, out int key);
			if (value != null)
			{
				if (value.LastResetTime < DateTime.Now)
				{
					Client.SendAsync(new GetMachineInfo(currentAccount, key, value, last));
				}
				else
				{
					Client.SendAsync(new GetMachineInfoResetting(currentAccount, key, value, last));
				}
			}
			else
			{
				Log.Warning("Unknown CapsuleMachineNum:{0}, NickName:{1}", num, currentAccount.NickName);
				Client.SendAsync(new GetMachineInfoFail(currentAccount, num, machineID, last));
			}
		}

		/// <summary>
		/// CapsuleMachine V2 (wire 2179). REQ: GetInfo=4, PickUP/spin=6 (legacy 1/5).
		/// GetInfo ACK id=5 (GET_CAPSULE_INFO); PickUP ACK id=7; GiveReward ACK id=11.
		/// Official open is V2 GetInfo only — do not dual-send classic VER2 (loads missing Dlg1011).
		/// </summary>
		public static void Handle_CapsuleMachineV2Protocol(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int inner = reader.ReadLEInt32();
			Log.Information("CAPSULE_MACHINE_V2 inner={0} user={1}", inner, currentAccount != null ? currentAccount.UserID : "?");
			if (inner == 4)
			{
				int machineItemNum = reader.ReadLEInt32();
				while (reader.Remaining > 0)
				{
					reader.ReadByte();
				}
				ResolveCapsuleMachine(machineItemNum, out CapsuleMachineData machine, out _);
				// Info only — do not draw / grant; do not send classic VER2 on V2 open.
				Client.SendAsync(new CapsuleMachineV2InfoAck(currentAccount, machineItemNum, machine, last));
				return;
			}
			// Client spin writer: inner 6, machine, costIndex. Keep 1/5 as legacy aliases.
			if (inner == 6 || inner == 5 || inner == 1)
			{
				int machineItemNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
				int costIndex = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
				while (reader.Remaining > 0)
				{
					reader.ReadByte();
				}
				ResolveCapsuleMachine(machineItemNum, out CapsuleMachineData machine, out _);
				if (machine != null && machine.LastResetTime < DateTime.Now)
				{
					int ret;
					int resultItemNum = machine.DrawItem(currentAccount, out ret);
					Log.Information("Capsule V2 PickUP inner={0} machine={1} costIdx={2} ret={3} item={4}",
						inner, machineItemNum, costIndex, ret, resultItemNum);
					Client.SendAsync(new CapsuleMachineV2PickUpAck(machineItemNum, (byte)((ret == 0) ? 1 : 0), resultItemNum, last));
					return;
				}
				Client.SendAsync(new CapsuleMachineV2PickUpAck(machineItemNum, 0, 0, last));
				return;
			}
			// GiveReward (inner 10) — ack 11 only; no grant without V2 reward tables.
			if (inner == 10)
			{
				int machineItemNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
				Log.Information("Capsule V2 GiveReward inner=10 machine={0} (ack only)", machineItemNum);
				Client.SendAsync(new CapsuleMachineV2GiveRewardAck(machineItemNum, last));
				return;
			}
			Log.Warning("Capsule V2 unhandled inner={0} remain={1}", inner, reader.Remaining);
			Client.SendAsync(new CapsuleMachineV2InfoAck(currentAccount, 0, null, last));
		}

		/// <summary>
		/// Classic/Capsule2 keep-claim (wire 2450). Reject when there is no real keep unique
		/// (catalog click after GetInfo sends ItemCount as if won). Never grant here.
		/// </summary>
		public static void Handle_ReceiveItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int peekItem = 0;
			int peekType = 0;
			try
			{
				if (reader.Remaining >= 4)
				{
					int count = reader.ReadLEInt32();
					if (count > 0 && reader.Remaining >= 16)
					{
						peekItem = reader.ReadLEInt32();
						reader.ReadLEInt32();
						peekType = reader.ReadLEInt32();
					}
				}
			}
			catch
			{
			}
			while (reader.Remaining > 0)
			{
				reader.ReadByte();
			}
			Log.Information("RECEIVE_ITEM rejected (no keep grant) user={0} type={1} itemOrCount={2}",
				currentAccount != null ? currentAccount.UserID : "?", peekType, peekItem);
			Client.SendAsync(new ReceiveItemFailAck(last));
		}

		private static void ResolveCapsuleMachine(int machineItemOrNum, out CapsuleMachineData machine, out int key)
		{
			machine = null;
			key = machineItemOrNum;
			// Park rotate furniture / group id → current rotate machine from DB setting.
			if (machineItemOrNum == 43845 || machineItemOrNum == 115360 || machineItemOrNum == 115361
				|| machineItemOrNum == 130572)
			{
				key = CapsuleMachineHolder.CurrentRotateNum;
			}
			if (CapsuleMachineHolder.CapsuleMachineContainer.TryGetValue(key, out machine))
			{
				return;
			}
			if (CapsuleMachineHolder.CapsuleMachineContainer.TryGetValue(machineItemOrNum, out machine))
			{
				key = machineItemOrNum;
				return;
			}
			// Match by RealMachineNumKind when client sends a kind-linked furniture id.
			foreach (KeyValuePair<int, CapsuleMachineData> kv in CapsuleMachineHolder.CapsuleMachineContainer)
			{
				if (kv.Value.RealMachineNum == machineItemOrNum || kv.Value.RealMachineNumKind == machineItemOrNum)
				{
					key = kv.Key;
					machine = kv.Value;
					return;
				}
			}
			// Last resort: current rotate (not FirstOrDefault isRotate — that picked kind-139 junk).
			key = CapsuleMachineHolder.CurrentRotateNum;
			CapsuleMachineHolder.CapsuleMachineContainer.TryGetValue(key, out machine);
		}

		public static void Handle_Alchemist_MachineSelect(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			switch (num)
			{
			case 2:
			{
				int fixedLength4 = reader.ReadLEInt16();
				int num3 = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength4));
				fixedLength4 = reader.ReadLEInt16();
				int num4 = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength4));
				if (num3 >= 1 && num3 <= 3 && num4 >= 1 && num4 <= 6)
				{
					Log.Debug("Divination  pDivination:{0}, pReqDivination:{1}", num3, num4);
					if (useDivination(currentAccount, num3, num4, 0, last))
					{
						currentAccount.CashNeedUpdateFromDB = true;
					}
				}
				break;
			}
			case 3:
			{
				int fixedLength7 = reader.ReadLEInt16();
				int num5 = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength7));
				int fixedLength8 = reader.ReadLEInt16();
				Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength8));
				if ((DateTime.Now - currentAccount.LastSelectMachineTime).TotalSeconds < 2.0)
				{
					Client.SendAsync(new GetMachineSelectItemFail(num5, 4, last));
					break;
				}
				currentAccount.LastSelectMachineTime = DateTime.Now;
				ResolveCapsuleMachine(num5, out var value2, out _);
				if (value2 != null)
				{
					if (value2.RealMachineNumKind > 1000 && !value2.isRotate)
					{
						Client.SendAsync(new GetMachineSelectItemFail(value2.RealMachineNum, 1, last));
					}
					else if (value2.LastResetTime < DateTime.Now)
					{
						int ret2;
						int resultItemNum = value2.DrawItem(currentAccount, out ret2);
						if (ret2 == 0)
						{
							Client.SendAsync(new GetMachineSelectItem(value2.RealMachineNum, resultItemNum, last));
							break;
						}
						byte err = (byte)(2 + ret2);
						Client.SendAsync(new GetMachineSelectItemFail(value2.RealMachineNum, err, last));
					}
					else
					{
						Client.SendAsync(new GetMachineSelectItemFail(value2.RealMachineNum, 9, last));
					}
				}
				else
				{
					Client.SendAsync(new GetMachineSelectItemFail(num5, 2, last));
				}
				break;
			}
			case 4:
			{
				int fixedLength5 = reader.ReadLEInt16();
				int itemnum = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength5));
				if (GiveUserFarmSlot(currentAccount, itemnum, out var SlotNum))
				{
					Client.SendAsync(new BuyFarmSlotOK_Ack(SlotNum, last));
				}
				break;
			}
			case 5:
			{
				int fixedLength6 = reader.ReadLEInt16();
				int itemnum2 = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength6));
				if (MyRoomGiveUserMyRoomSlot(currentAccount, itemnum2, out var SlotNum2))
				{
					Client.SendAsync(new Myroom_BuyMyRoomSlotOK(SlotNum2, last));
				}
				break;
			}
			case 8:
			{
				int fixedLength = reader.ReadLEInt16();
				int key = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength));
				int fixedLength2 = reader.ReadLEInt16();
				short num2 = Convert.ToInt16(reader.ReadBig5StringSafe(fixedLength2));
				int fixedLength3 = reader.ReadLEInt16();
				Convert.ToInt32(reader.ReadBig5StringSafe(fixedLength3));
				if (EventPickBoardHolder.HuMongPickBoardContainer.TryGetValue(key, out var value) && num2 <= value.PickInfo.Length)
				{
					if (value.LastResetTime < DateTime.Now && !value.PickInfo[num2 - 1])
					{
						int AdditionRewardItemNum;
						int ret;
						byte rank = value.PickItem(currentAccount, num2, out AdditionRewardItemNum, out ret);
						if (ret == 0)
						{
							Client.SendAsync(new HuMongPickBoard_PickItem_OK(num2, rank, AdditionRewardItemNum, last));
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
				break;
			}
			case 10000:
			{
				// StatSystem page unlock: PAY_CASH_REQ type=ALCHEMIST_SLOT_CONSUME_ITEM.
				int fixedLengthSlot = reader.ReadLEInt16();
				int slotItemNum = Convert.ToInt32(reader.ReadBig5StringSafe(fixedLengthSlot));
				if (reader.Remaining >= 4)
				{
					reader.ReadLEInt32();
				}
				bool consumed = ConsumeInventoryItem(currentAccount, slotItemNum, 1);
				// Unlock even if item missing (private server / client asks 109321..109324).
				if (currentAccount.StatSystemPageCount < 10)
				{
					currentAccount.StatSystemPageCount++;
				}
				try
				{
					using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
					conn.Open();
					using MySqlCommand cmd = new MySqlCommand(
						@"INSERT INTO UserStatSystemInfo (fdUserNum, fdPageCount, fdRemainPoint, fdLastSlot)
						  VALUES (@u, @p, 999, @l)
						  ON DUPLICATE KEY UPDATE fdPageCount=@p, fdLastSlot=@l", conn);
					cmd.Parameters.AddWithValue("@u", currentAccount.UserNum);
					cmd.Parameters.AddWithValue("@p", currentAccount.StatSystemPageCount);
					cmd.Parameters.AddWithValue("@l", currentAccount.StatSystemPageCount - 1);
					cmd.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Log.Warning("StatSystem page DB update: {0}", ex.Message);
				}
				Log.Information("StatSystem slot consume user={0} item={1} ok={2} pages={3}", currentAccount.UserID, slotItemNum, consumed, currentAccount.StatSystemPageCount);
				Client.SendAsync(new StatSystemPayPageResultAck(last));
				Client.SendAsync(new StatSystemMyInfoAck(last, currentAccount.StatSystemPageCount, currentAccount.StatSystemTitles));
				break;
			}
			default:
				Log.Warning("Unknown Alchemist_MachineSelect type: {0}!", num);
				break;
			}
		}

		public static void Handle_MachineReceiveORGiftItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int resultItemNum = 0;
			bool isGift = false;
			int num = reader.ReadLEInt16();
			string text = string.Empty;
			if (num > 0)
			{
				text = reader.ReadBig5StringSafe(num);
			}
			byte ret;
			if (text == string.Empty)
			{
				ret = 1;
			}
			else
			{
				int num2 = reader.ReadLEInt16();
				string memo = string.Empty;
				if (num2 > 0)
				{
					memo = reader.ReadBig5StringSafe(num2);
				}
				resultItemNum = GiveItem(currentAccount, text, memo, out isGift, out ret);
			}
			if (ret == 0)
			{
				Client.SendAsync(new MachineGiveItem(currentAccount, resultItemNum, isGift, text, last));
				return;
			}
			byte err = (byte)(2 + ret);
			Client.SendAsync(new MachineGiveItemFail(err, last));
		}

		public static void Handle_MachineKeepItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int type = reader.ReadLEInt32();
			int itemnum = reader.ReadLEInt32();
			long uniqueNum;
			int itemNum;
			long dateTime;
			bool isSuccess = KeepItem(currentAccount, type, itemnum, out uniqueNum, out itemNum, out dateTime);
			Client.SendAsync(new MachineKeepItem(currentAccount, isSuccess, uniqueNum, itemNum, dateTime, last));
		}

		public static void Handle_CheckDivinationFree(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num >= 1 && num <= 3)
			{
				checkDivinationFree(currentAccount, num, out var isFree);
				Client.SendAsync(new CheckDivinationFree_ACK(num, isFree, last));
			}
		}

		public static void Handle_ParkUpdateCoupleAbility(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (reader.Remaining >= 4)
			{
				int pExItemNum = reader.ReadLEInt32();
				divinationUpdateCoupleAbility(currentAccount, pExItemNum);
			}
			_ = last;
		}

		public static void Handle_ParkUseDivinationFree(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (num >= 1 && num <= 3 && num2 >= 1 && num2 <= 6 && useDivination(currentAccount, num, num2, 1, last))
			{
				currentAccount.CashNeedUpdateFromDB = true;
			}
		}

		private static int GiveItem(Account User, string NickName, string memo, out bool isGift, out byte ret)
		{
			ret = 0;
			isGift = false;
			int result = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_capsuleMachineGiveItem";
				mySqlCommand.Parameters.Add("sendUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("sendNickname", MySqlDbType.VarString).Value = User.NickName;
				mySqlCommand.Parameters.Add("receiveNickname", MySqlDbType.VarString).Value = NickName;
				mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = memo;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				ret = (byte)mySqlDataReader.GetInt32("retval");
				if (ret == 0)
				{
					result = mySqlDataReader.GetInt32("itemNum");
					isGift = mySqlDataReader.GetBoolean("isGift");
					Convert.ToInt64(mySqlDataReader["reamainGameMoney"]);
					return result;
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_capsuleMachineGiveItem Error: {0}", ex.Message);
				ret = 2;
				return result;
			}
		}

		private static bool KeepItem(Account User, int type, int itemnum, out long uniqueNum, out int itemNum, out long dateTime)
		{
			uniqueNum = 0L;
			itemNum = 0;
			dateTime = 0L;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				// SP only accepts type=1 (capsule keep). Client also sends type=3 for
				// inventory→storage; fall through to Insert_Keeping directly.
				if (type == 1)
				{
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_storage_save";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("type", MySqlDbType.Int32).Value = type;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemnum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						uniqueNum = Convert.ToInt64(mySqlDataReader["uniqueNum"]);
						itemNum = Convert.ToInt32(mySqlDataReader["itemNum"]);
						dateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["dateTime"]));
						return true;
					}
					return false;
				}
				using (MySqlCommand insert = new MySqlCommand(string.Empty, mySqlConnection))
				{
					insert.CommandType = CommandType.StoredProcedure;
					insert.CommandText = "usp_storage_Insert_Keeping";
					insert.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					insert.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemnum;
					MySqlParameter ret = insert.Parameters.Add("ret", MySqlDbType.Int32);
					ret.Direction = ParameterDirection.Output;
					insert.ExecuteNonQuery();
					int code = ret.Value != null && ret.Value != DBNull.Value ? Convert.ToInt32(ret.Value) : 1;
					if (code != 0)
					{
						Log.Warning("storage Insert_Keeping type={0} item={1} ret={2}", type, itemnum, code);
						return false;
					}
				}
				using (MySqlCommand sel = new MySqlCommand(
					"SELECT fdUniqueNum, fdItemNum, fdDateTime FROM userstoragekeepingitem WHERE fdUserNum=@u AND fdItemNum=@i ORDER BY fdUniqueNum DESC LIMIT 1",
					mySqlConnection))
				{
					sel.Parameters.AddWithValue("@u", User.UserNum);
					sel.Parameters.AddWithValue("@i", itemnum);
					using MySqlDataReader rd = sel.ExecuteReader(CommandBehavior.SingleRow);
					if (!rd.Read())
					{
						return false;
					}
					uniqueNum = Convert.ToInt64(rd["fdUniqueNum"]);
					itemNum = Convert.ToInt32(rd["fdItemNum"]);
					dateTime = Utility.ConvertToTimestamp(Convert.ToDateTime(rd["fdDateTime"]));
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error on keep item: {0}", ex.Message);
				return false;
			}
		}

		public static int GetAlchemistMixGrade(float luck)
		{
			int num = 0;
			int num2 = 0;
			Random random = new Random(Guid.NewGuid().GetHashCode());
			if (luck <= 0f)
			{
				int num3 = 81;
				num2 = random.Next() % num3;
			}
			else
			{
				int num4 = 70;
				int num5 = (int)(luck / (float)num4 + 1f);
				num5 = ((num5 > 10) ? 10 : num5);
				for (int i = 0; i < num5; i++)
				{
					int num6 = 30;
					int num7 = num6 + random.Next() % (101 - num6);
					if (num2 < num7)
					{
						num2 = num7;
					}
				}
			}
			int num8 = 98;
			int num9 = 75;
			int num10 = 40;
			if (num2 < num8)
			{
				if (num2 < num9)
				{
					if (num2 < num10)
					{
						return 400;
					}
					return 300;
				}
				return 200;
			}
			return 100;
		}

		private static bool MyRoomGiveUserMyRoomSlot(Account User, int itemnum, out int SlotNum)
		{
			SlotNum = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_myRoom_GiveUserMyRoomSlot";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pMyRoomSlotItemNum", MySqlDbType.Int32).Value = itemnum;
				mySqlCommand.Parameters.Add("pMyRoomSlotMethod", MySqlDbType.Int32).Value = 2;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					SlotNum = Convert.ToInt32(mySqlDataReader["fdSlotNum"]);
					User.TR -= Convert.ToInt32(mySqlDataReader["TRPrice"]);
					User.Cash -= Convert.ToInt32(mySqlDataReader["CashPrice"]);
					return true;
				}
			}
			return false;
		}

		private static bool GiveUserFarmSlot(Account User, int itemnum, out int SlotNum)
		{
			SlotNum = 0;
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_GiveUserFarmSlot";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pFarmSlotItemNum", MySqlDbType.Int32).Value = itemnum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					SlotNum = Convert.ToInt32(mySqlDataReader["fdSlotNum"]);
					User.TR -= Convert.ToInt32(mySqlDataReader["TRPrice"]);
					User.Cash -= Convert.ToInt32(mySqlDataReader["CashPrice"]);
					return true;
				}
			}
			return false;
		}

		private static void checkDivinationFree(Account User, int pDivination, out bool isFree)
		{
			isFree = false;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_checkDivinationFree";
				mySqlCommand.Parameters.Add("pUsernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pDivination", MySqlDbType.Int32).Value = pDivination;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					if (mySqlDataReader.GetInt32("retVal") != 0)
					{
						isFree = true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_checkDivinationFree Error:{0}", ex.Message);
			}
		}

		private static bool useDivination2(Account User, int pDivination, int pReqDivination, int pFree, out ExtraAbilityItemAttrInfo exItemAttrInfo, out ExtraAbilityItemInfo exItemInfo)
		{
			exItemAttrInfo = new ExtraAbilityItemAttrInfo();
			exItemInfo = new ExtraAbilityItemInfo();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_useDivination";
				mySqlCommand.Parameters.Add("pUsernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pDivination", MySqlDbType.Int32).Value = pDivination;
				mySqlCommand.Parameters.Add("pReqDivination", MySqlDbType.Int32).Value = pReqDivination;
				mySqlCommand.Parameters.Add("pCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
				mySqlCommand.Parameters.Add("pFree", MySqlDbType.Int32).Value = pFree;
				mySqlCommand.Parameters.Add("ignoreFreeCheck", MySqlDbType.Int32).Value = 0;
				mySqlCommand.Parameters.Add("iMinute", MySqlDbType.Int32).Value = 15;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (mySqlDataReader.HasRows)
				{
					while (mySqlDataReader.Read())
					{
						ushort attr = Convert.ToUInt16(mySqlDataReader["attrtype"]);
						float attrValue = Convert.ToSingle(mySqlDataReader["attrvalue"]);
						ItemAttr item = new ItemAttr
						{
							Attr = attr,
							AttrValue = attrValue
						};
						exItemAttrInfo.attrlist.Add(item);
						int @int = mySqlDataReader.GetInt32("itemnum");
						exItemAttrInfo.itemnum = @int;
						exItemAttrInfo.limit = Convert.ToInt32(mySqlDataReader["limit"]);
						long num = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("gottime"));
						exItemAttrInfo.gottime = num;
						NetItemInfo info = new NetItemInfo(@int, Convert.ToInt16(mySqlDataReader["character"]), Convert.ToUInt16(mySqlDataReader["position"]), Convert.ToUInt16(mySqlDataReader["kind"]), Convert.ToBoolean(mySqlDataReader["using"]), Convert.ToInt32(mySqlDataReader["count"]), Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("expiretime")), num);
						User.activeItem.replaceItemInfoByItem(info);
					}
					exItemInfo.divinationType = pDivination;
					exItemInfo.remainItemCount = 0;
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_useDivination Error:{0}", ex.Message);
			}
			return false;
		}

		private static bool useDivination(Account User, int pDivination, int pReqDivination, int pFree, byte last)
		{
			CActiveItems cActiveItems = new CActiveItems();
			Dictionary<int, ExtraAbilityInfo> dictionary = new Dictionary<int, ExtraAbilityInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_useDivination");
				mySqlCommandHelper.AddParamInt("pUsernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pDivination", pDivination);
				mySqlCommandHelper.AddParamInt("pReqDivination", pReqDivination);
				mySqlCommandHelper.AddParamInt("pCoupleNum", User.CoupleInfo.CoupleNum);
				mySqlCommandHelper.AddParamInt("pFree", pFree);
				mySqlCommandHelper.AddParamInt("ignoreFreeCheck", 0);
				mySqlCommandHelper.AddParamInt("iMinute", 15);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					int @int = mySqlCommandHelper.GetInt("itemNum");
					if (!dictionary.TryGetValue(@int, out var value))
					{
						value = new ExtraAbilityInfo();
					}
					short key = (short)mySqlCommandHelper.GetInt("attr");
					float @float = mySqlCommandHelper.GetFloat("value");
					int int2 = mySqlCommandHelper.GetInt("limit");
					long dateTime = mySqlCommandHelper.GetDateTime("gotTime", 0L);
					value.iItemDescNum = @int;
					value.iLimitTime = int2;
					value.tGotTime = dateTime;
					value.mapAttributes[key] = @float;
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, value);
					}
					else
					{
						dictionary[@int] = value;
					}
				}
				mySqlCommandHelper.NextResult();
				while (mySqlCommandHelper.HasResult())
				{
					int int3 = mySqlCommandHelper.GetInt("itemNum");
					cpk_type character = mySqlCommandHelper.GetInt("itemCharacter");
					cpk_type position = mySqlCommandHelper.GetInt("itemPosition");
					cpk_type kind = mySqlCommandHelper.GetInt("itemKind");
					bool boolean = mySqlCommandHelper.GetBoolean("using");
					int int4 = mySqlCommandHelper.GetInt("itemCount");
					long expiretime = 0L;
					if (!mySqlCommandHelper.IsDBNull("expireTime"))
					{
						expiretime = mySqlCommandHelper.GetDateTime("expireTime", 0L);
					}
					cActiveItems.insertItem(new NetItemInfo(int3, character, position, kind, boolean, int4, expiretime, 0L));
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_useDivination Error:{0}", ex.Message);
			}
			if (flag)
			{
				User.Connection.SendAsync(new ParkUseDivination_ACK(User, pDivination, dictionary, last));
				foreach (NetItemInfo item in cActiveItems.getVector())
				{
					User.activeItem.replaceItemInfoByItem(item);
				}
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(User, cActiveItems, dictionary, last), room.RoomServerID);
				}
				if (pDivination == 3)
				{
					int iItemDescNum = dictionary.FirstOrDefault().Value.iItemDescNum;
					ParkDivinationCouple message = new ParkDivinationCouple
					{
						ItemNum = iItemDescNum,
						UserNum = User.UserNum,
						CoupleNum = User.CoupleInfo.CoupleNum,
						Attrs = new Dictionary<ushort, float>(),
						limit = 0,
						gottime = 0L
					};
					ServerStatus.LBServerActor.Tell(message);
				}
			}
			return flag;
		}

		public static bool divinationUpdateCoupleAbility(Account User, int pExItemNum)
		{
			CActiveItems cActiveItems = new CActiveItems();
			Dictionary<int, ExtraAbilityInfo> dictionary = new Dictionary<int, ExtraAbilityInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_divinationUpdateCoupleAbility");
				mySqlCommandHelper.AddParamInt("pUsernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pExItemNum", pExItemNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					ExtraAbilityInfo extraAbilityInfo = new ExtraAbilityInfo();
					short key = (short)mySqlCommandHelper.GetInt("attr");
					float @float = mySqlCommandHelper.GetFloat("value");
					int @int = mySqlCommandHelper.GetInt("itemNum");
					int int2 = mySqlCommandHelper.GetInt("limit");
					long dateTime = mySqlCommandHelper.GetDateTime("gotTime", 0L);
					extraAbilityInfo.iItemDescNum = @int;
					extraAbilityInfo.iLimitTime = int2;
					extraAbilityInfo.tGotTime = dateTime;
					extraAbilityInfo.mapAttributes[key] = @float;
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, extraAbilityInfo);
					}
					else
					{
						dictionary[@int] = extraAbilityInfo;
					}
				}
				mySqlCommandHelper.NextResult();
				while (mySqlCommandHelper.HasResult())
				{
					int int3 = mySqlCommandHelper.GetInt("itemNum");
					cpk_type character = mySqlCommandHelper.GetInt("itemCharacter");
					cpk_type position = mySqlCommandHelper.GetInt("itemPosition");
					cpk_type kind = mySqlCommandHelper.GetInt("itemKind");
					bool boolean = mySqlCommandHelper.GetBoolean("using");
					int int4 = mySqlCommandHelper.GetInt("itemCount");
					long expiretime = 0L;
					if (!mySqlCommandHelper.IsDBNull("expireTime"))
					{
						expiretime = mySqlCommandHelper.GetDateTime("expireTime", 0L);
					}
					cActiveItems.insertItem(new NetItemInfo(int3, character, position, kind, boolean, int4, expiretime, 0L));
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_divinationUpdateCoupleAbility Error:{0}", ex.Message);
			}
			if (flag)
			{
				User.Connection.SendAsync(new ParkUseDivination_Couple_ACK(User, dictionary, 1));
				foreach (NetItemInfo item in cActiveItems.getVector())
				{
					User.activeItem.replaceItemInfoByItem(item);
				}
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(User, cActiveItems, dictionary, 1), room.RoomServerID);
				}
			}
			return flag;
		}

		private static bool ConsumeInventoryItem(Account User, int itemDescNum, int count)
		{
			if (itemDescNum <= 0 || count <= 0)
			{
				return false;
			}
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using (MySqlCommand upd = new MySqlCommand(
					@"UPDATE tblAvatarUser
					  SET fdCount = fdCount - @c
					  WHERE fdUserNum = @u AND fdItemDescNum = @i AND fdCount >= @c", conn))
				{
					upd.Parameters.AddWithValue("@u", User.UserNum);
					upd.Parameters.AddWithValue("@i", itemDescNum);
					upd.Parameters.AddWithValue("@c", count);
					if (upd.ExecuteNonQuery() <= 0)
					{
						return false;
					}
				}
				using (MySqlCommand del = new MySqlCommand(
					@"DELETE FROM tblAvatarUser
					  WHERE fdUserNum = @u AND fdItemDescNum = @i AND fdCount <= 0", conn))
				{
					del.Parameters.AddWithValue("@u", User.UserNum);
					del.Parameters.AddWithValue("@i", itemDescNum);
					del.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("ConsumeInventoryItem user={0} item={1}: {2}", User.UserNum, itemDescNum, ex.Message);
				return false;
			}
		}
	}
}
