using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.User;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using NetMsg.LBS;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class CoupleHandle
	{
		public static void Handle_CheckProposeInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(num);
			int fixedLength = reader.ReadLEInt16();
			string empty2 = string.Empty;
			empty2 = reader.ReadBig5StringSafe(fixedLength);
			reader.ReadLEInt32();
			int num2 = 0;
			num2 = ((num > 0 && num <= 18) ? CheckProposeInfo(currentAccount.UserNum, empty, empty2) : 117);
			if (num2 > 0)
			{
				num2 = 122;
			}
			Client.SendAsync(new CheckProposeInfoACK(num2, last));
		}

		public static void Handle_ChangeCoupleRing(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int jewelBoxNum = reader.ReadLEInt32();
			DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(currentAccount.CoupleInfo.RingChangedTime).LocalDateTime;
			int num = (int)TimeSpan.FromHours(currentAccount.CoupleInfo.AccumulateExp / 100).TotalDays;
			int coupleDays = (DateTime.Now - localDateTime).Days + num;
			if (ChangeCoupleRing(currentAccount, jewelBoxNum, coupleDays, out var bOnline, out var additionItemNum))
			{
				Client.SendAsync(new ChangeCoupleRingACK(currentAccount, bOnline, additionItemNum, last));
				string shoutMsg = $"{currentAccount.NickName}/{currentAccount.CoupleInfo.MateName}";
				ServerStatus.LBServerActor.Tell(new CommandHandle.ShoutToAll("coupleMSG", 2, currentAccount.CoupleInfo.CoupleRingNum, shoutMsg, 2603354669L, last));
			}
			else
			{
				Client.SendAsync(new ChangeCoupleRingFailACK(0, last));
			}
		}

		public static void Handle_InitProposeInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			_ = string.Empty;
			InitProposeInfo(reader.ReadBig5StringSafe(fixedLength));
			InitProposeInfo(currentAccount.NickName);
			Client.SendAsync(new InitProposeInfoACK(last));
		}

		public static void Handle_CreateCoupleInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			int fixedLength2 = reader.ReadLEInt16();
			string empty2 = string.Empty;
			empty2 = reader.ReadBig5StringSafe(fixedLength2);
			int fixedLength3 = reader.ReadLEInt16();
			string empty3 = string.Empty;
			empty3 = reader.ReadBig5StringSafe(fixedLength3);
			int jewelBoxNum = reader.ReadLEInt32();
			if (CreateCoupleInfo(currentAccount.UserNum, empty, empty2, empty3, jewelBoxNum, out var coupleNum, out var coupleRingNum))
			{
				Client.SendAsync(new eServer_COUPLE_CREATE_COUPLE_INFO_ACK(0, coupleNum, coupleRingNum, last));
				string shoutMsg = $"{currentAccount.NickName}/{empty3}";
				ServerStatus.LBServerActor.Tell(new CommandHandle.ShoutToAll("coupleMSG", 1, 0, shoutMsg, currentAccount.Exp, last));
			}
			else
			{
				byte failedReason = 125;
				Client.SendAsync(new eServer_COUPLE_CREATE_COUPLE_INFO_ACK(failedReason, -1, 0, last));
			}
		}

		public static void Handle_ModifyCoupleInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			ModifyCoupleInfo(currentAccount, empty);
			Client.SendAsync(new ModifyCoupleInfoACK(last));
		}

		public static void Handle_ModifyCoupleName(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			if (ModifyCoupleName(currentAccount, empty))
			{
				Client.SendAsync(new ModifyCoupleNameACK(empty, last));
			}
		}

		public static void Handle_RemoveCoupleInfo(ClientConnection Client, byte last)
		{
			if (RemoveCoupleInfo(Client.CurrentAccount))
			{
				Client.SendAsync(new RemoveCoupleACK(last));
			}
		}

		public static void Handle_GetCoupleInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int coupleNum = reader.ReadLEInt32();
			byte flag = reader.ReadByte();
			Client.SendAsync(new CoupleInfoACK(coupleNum, flag, last));
		}

		public static void Handle_UpdateCoupleInfo(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			UpdateCoupleInfo(currentAccount);
			Client.SendAsync(new UpdateCoupleInfoACK(currentAccount, last));
		}

		public static void Handle_WeddingSuitForDivorce(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int divorceType = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			if (WeddingSuitForDivorce(currentAccount, divorceType, empty))
			{
				Client.SendAsync(new WeddingSuitForDivorceACK(0, divorceType, last));
			}
			else
			{
				Client.SendAsync(new WeddingSuitForDivorceACK(126, divorceType, last));
			}
		}

		public static void Handle_WeddingDivorceReject(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int disagreeType = reader.ReadLEInt32();
			if (WeddingInitRequestDivorceInfo(currentAccount, disagreeType))
			{
				Client.SendAsync(new WeddingDivorceRejectACK(disagreeType, last));
			}
		}

		public static void Handle_WeddingDivorce(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			if (!(num == 2 && flag) && WeddingDivorce(currentAccount, num, flag))
			{
				Client.SendAsync(new WeddingDivorceACK(num, flag, last));
				if (num == 1 || num == 2)
				{
					currentAccount.resetCouple();
				}
				if (num == 0)
				{
					currentAccount.setCoupleType(num);
				}
				ServerStatus.LBServerActor.Tell(new WeddingDivorce
				{
					NickName = currentAccount.CoupleInfo.MateName,
					DivorceType = num
				});
			}
		}

		public static void Handle_GetFamilyInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			string text = string.Empty;
			try
			{
				_ = Client.CurrentAccount;
				int num = reader.ReadLEInt16();
				if (num > 0)
				{
					text = reader.ReadBig5StringSafe(num);
				}
				byte flag = reader.ReadByte();
				if (!(text == string.Empty) && GetFamilyInfo(text, out var familyinfos) && familyinfos.Count > 0)
				{
					Client.SendAsync(new FamilyInfoACK(text, flag, familyinfos, last));
				}
			}
			catch (Exception ex)
			{
				Log.Error("GetFamilyInfo Error:{0}, nickName:{1}", ex.Message, text);
			}
		}

		public static void Handle_MakeFamilyCheck(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			bool isParents = reader.ReadBoolean();
			reader.ReadLEInt32();
			if (FamilyCheckProposeCondition(empty, currentAccount.UserNum, isParents))
			{
				Client.SendAsync(new MakeFamilyCheckOKACK(empty, isParents, last));
			}
			else
			{
				Client.SendAsync(new MakeFamilyCheckFailACK(132, last));
			}
		}

		public static void Handle_MakeFamily(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			bool bParents = reader.ReadBoolean();
			if (MakeFamily(currentAccount, empty, bParents, out var familyinfos))
			{
				Client.SendAsync(new MakeFamilyOKACK(currentAccount.NickName, familyinfos, last));
			}
		}

		public static void Handle_DissolveFamily(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short num = reader.ReadLEInt16();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = reader.ReadBig5StringSafe(fixedLength);
			bool flag = false;
			if (num == 0)
			{
				flag = true;
			}
			if (DissolveFamily(currentAccount, empty, flag, out var familyinfos))
			{
				Client.SendAsync(new DissolveFamilyOKACK(currentAccount.NickName, empty, flag, familyinfos, last));
			}
		}

		public static void Handle_UseCoupleExpAddItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int err = 0;
			DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(currentAccount.CoupleInfo.RingChangedTime).LocalDateTime;
			int num2 = (int)TimeSpan.FromHours(currentAccount.CoupleInfo.AccumulateExp / 100).TotalDays;
			int num3 = (DateTime.Now - localDateTime).Days + num2;
			if (ShopItemTable.getItemDataFromItemDescNum(num, out var itemData))
			{
				bool flag = false;
				int addExp = 0;
				if (!itemData.isPosition(eFuncItemPosition.eFuncItemPosition_COUPLE_EXP_ADD) || !itemData.m_mapAttr.ContainsKey(226))
				{
					err = 264;
				}
				else if (currentAccount.CoupleInfo.CoupleNum == -1)
				{
					err = 266;
				}
				else if (currentAccount.CoupleInfo.CoupleLevel >= 25)
				{
					err = 268;
				}
				else if (num3 >= currentAccount.CoupleInfo.MaxRingDays)
				{
					err = 267;
				}
				else
				{
					flag = true;
					addExp = (int)itemData.m_mapAttr[226];
				}
				if (flag && useCoupleExpAddItem(currentAccount, num, addExp))
				{
					Client.SendAsync(new UseCoupleExpAddItem_ACK(num, currentAccount.CoupleInfo.AccumulateExp, last));
				}
				else
				{
					Client.SendAsync(new UseCoupleExpAddItemFail_ACK(err, last));
				}
			}
			else
			{
				short err2 = 264;
				Client.SendAsync(new UseCoupleExpAddItemFail_ACK(err2, last));
				Log.Warning("Unknown Use ItemNum: {0}", num);
			}
		}

		private static int CheckProposeInfo(int UserNum, string coupleName, string mateName)
		{
			int result = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_coupleCheckProposeInfo";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("coupleName", MySqlDbType.String).Value = coupleName;
					mySqlCommand.Parameters.Add("mateName", MySqlDbType.String).Value = mateName;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					result = Convert.ToInt32(mySqlDataReader["ret"]);
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleCheckProposeInfo Error:{0}", ex.Message);
				return 7;
			}
		}

		private static void InitProposeInfo(string NickName)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleInitProposeInfo";
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.String).Value = NickName;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleInitProposeInfo Error:{0}", ex.Message);
			}
		}

		private static bool CreateCoupleInfo(int UserNum, string coupleName, string coupleDesc, string mateName, int jewelBoxNum, out int coupleNum, out int coupleRingNum)
		{
			coupleNum = -1;
			coupleRingNum = -1;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_coupleCreateCoupleInfo";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("coupleName", MySqlDbType.String).Value = coupleName;
					mySqlCommand.Parameters.Add("coupleDesc", MySqlDbType.String).Value = coupleDesc;
					mySqlCommand.Parameters.Add("mateName", MySqlDbType.String).Value = mateName;
					mySqlCommand.Parameters.Add("jewelBoxNum", MySqlDbType.Int32).Value = jewelBoxNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					mySqlDataReader.Read();
					if (Convert.ToInt32(mySqlDataReader["ret"]) == 0)
					{
						coupleNum = Convert.ToInt32(mySqlDataReader["coupleNum"]);
						coupleRingNum = Convert.ToInt32(mySqlDataReader["coupleRingNum"]);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleCreateCoupleInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool ChangeCoupleRing(Account User, int jewelBoxNum, int coupleDays, out bool bOnline, out int additionItemNum)
		{
			bOnline = false;
			additionItemNum = 0;
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_coupleChangeCoupleRing";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("coupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("jewelBoxNum", MySqlDbType.Int32).Value = jewelBoxNum;
					mySqlCommand.Parameters.Add("coupleDays", MySqlDbType.Int32).Value = coupleDays;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						User.CoupleInfo.CoupleNum = mySqlDataReader.GetInt32("coupleNum");
						User.CoupleInfo.MateName = mySqlDataReader.GetString("mateName");
						User.CoupleInfo.CreateTime = ((mySqlDataReader.GetString("createTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("createtime")));
						User.CoupleInfo.CoupleRingNum = mySqlDataReader.GetInt32("coupleRingNum");
						User.CoupleInfo.CondDays = mySqlDataReader.GetInt32("condDays");
						User.CoupleInfo.CoupleLevel = mySqlDataReader.GetInt16("coupleLevel");
						User.CoupleInfo.MaxRingDays = mySqlDataReader.GetInt32("maxRingDays");
						User.CoupleInfo.CoupleType = mySqlDataReader.GetInt32("coupleType");
						User.CoupleInfo.MarriedTime = ((mySqlDataReader.GetString("marriedTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("marriedTime")));
						User.CoupleInfo.RingChangedTime = ((mySqlDataReader.GetString("ringChangedTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("ringChangedTime")));
						User.CoupleInfo.AccumulateExp = mySqlDataReader.GetInt32("accumulateExp");
						User.CoupleInfo.CouplePoint = mySqlDataReader.GetInt32("couplePoint");
						User.CoupleInfo.CoupleRank = mySqlDataReader.GetInt32("coupleRank");
						bOnline = Convert.ToBoolean(mySqlDataReader["online"]);
						additionItemNum = mySqlDataReader.GetInt32("additionItemNum");
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleChangeCoupleRing Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool RemoveCoupleInfo(Account User)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_coupleRemoveCoupleInfo";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("couplenum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleRemoveCoupleInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static void UpdateCoupleInfo(Account User)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleUpdateCoupleInfo";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("additionItemNum", MySqlDbType.Int32).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				User.CoupleInfo.CoupleNum = mySqlDataReader.GetInt32("coupleNum");
				User.CoupleInfo.MateName = mySqlDataReader.GetString("mateName");
				User.CoupleInfo.CreateTime = ((mySqlDataReader.GetString("createTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("createtime")));
				User.CoupleInfo.CoupleRingNum = mySqlDataReader.GetInt32("coupleRingNum");
				User.CoupleInfo.CondDays = mySqlDataReader.GetInt32("condDays");
				User.CoupleInfo.CoupleLevel = mySqlDataReader.GetInt16("coupleLevel");
				User.CoupleInfo.MaxRingDays = mySqlDataReader.GetInt32("maxRingDays");
				User.CoupleInfo.CoupleType = mySqlDataReader.GetInt32("coupleType");
				User.CoupleInfo.MarriedTime = ((mySqlDataReader.GetString("marriedTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("marriedTime")));
				User.CoupleInfo.RingChangedTime = ((mySqlDataReader.GetString("ringChangedTime") == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("ringChangedTime")));
				User.CoupleInfo.AccumulateExp = mySqlDataReader.GetInt32("accumulateExp");
				User.CoupleInfo.CouplePoint = mySqlDataReader.GetInt32("couplePoint");
				User.CoupleInfo.CoupleRank = mySqlDataReader.GetInt32("coupleRank");
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleUpdateCoupleInfo Error:{0}", ex.Message);
			}
		}

		private static void ModifyCoupleInfo(Account User, string coupleDesc)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleModifyCoupleInfo";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("username", MySqlDbType.String).Value = User.NickName;
				mySqlCommand.Parameters.Add("couplenum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
				mySqlCommand.Parameters.Add("coupleDesc", MySqlDbType.String).Value = coupleDesc;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleModifyCoupleInfo Error:{0}", ex.Message);
			}
		}

		private static bool ModifyCoupleName(Account User, string coupleName)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_coupleModifyCoupleName";
					mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("username", MySqlDbType.String).Value = User.NickName;
					mySqlCommand.Parameters.Add("coupleName", MySqlDbType.String).Value = coupleName;
					mySqlCommand.Parameters.Add("mateName", MySqlDbType.String).Value = User.CoupleInfo.MateName;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleModifyCoupleName Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool useCoupleExpAddItem(Account User, int itemNum, int addExp)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_useCoupleExpAddItem";
					mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("coupleMateNickName", MySqlDbType.String).Value = User.CoupleInfo.MateName;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemNum;
					mySqlCommand.Parameters.Add("addExp", MySqlDbType.Int32).Value = addExp;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						User.CoupleInfo.AccumulateExp = Convert.ToInt32(mySqlDataReader["coupleExp"]);
						int iCount = Convert.ToInt32(mySqlDataReader["remainCount"]);
						User.activeItem.updateItemCount(itemNum, iCount);
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_useCoupleExpAddItem Error:{0}", ex.Message);
				return false;
			}
		}

		public static bool WeddingCreateCouple(Account User, long CerremonyOfficerID, int itemnum)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_weddingCreateCouple";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pMateUserName", MySqlDbType.String).Value = User.CoupleInfo.MateName;
					mySqlCommand.Parameters.Add("pCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("pCerremonyOfficerID", MySqlDbType.Int64).Value = CerremonyOfficerID;
					mySqlCommand.Parameters.Add("itemNum", MySqlDbType.Int32).Value = itemnum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						mySqlDataReader.Read();
						if (mySqlDataReader.GetInt32("weddingItemNum") == itemnum)
						{
							return true;
						}
						return false;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_weddingCreateCouple Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool WeddingSuitForDivorce(Account User, int DivorceType, string MateName)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_weddingSuitForDivorce";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pNickname", MySqlDbType.String).Value = User.NickName;
					mySqlCommand.Parameters.Add("pCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("pDivorceType", MySqlDbType.Int32).Value = DivorceType;
					mySqlCommand.Parameters.Add("pMateNickname", MySqlDbType.String).Value = MateName;
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
				Log.Error("usp_weddingSuitForDivorce Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool WeddingInitRequestDivorceInfo(Account User, int DisagreeType)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_weddingInitRequestDivorceInfo";
					mySqlCommand.Parameters.Add("pNickname", MySqlDbType.String).Value = User.NickName;
					mySqlCommand.Parameters.Add("pMateNickname", MySqlDbType.String).Value = User.CoupleInfo.MateName;
					mySqlCommand.Parameters.Add("pDisagreeType", MySqlDbType.Int32).Value = DisagreeType;
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
				Log.Error("usp_weddingInitRequestDivorceInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool WeddingDivorce(Account User, int DivorceType, bool isEnforce)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_weddingDivorce";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pNickname", MySqlDbType.String).Value = User.NickName;
					mySqlCommand.Parameters.Add("pCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("pDivorceType", MySqlDbType.Int32).Value = DivorceType;
					mySqlCommand.Parameters.Add("pEnforce", MySqlDbType.Int32).Value = isEnforce;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
					if (mySqlDataReader.HasRows)
					{
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_weddingDivorce Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool GetFamilyInfo(string NickName, out List<FamilyInfo> familyinfos)
		{
			familyinfos = new List<FamilyInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_familyGetFamilyInfo";
					mySqlCommand.Parameters.Add("nickname", MySqlDbType.String).Value = NickName;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							FamilyInfo item = new FamilyInfo
							{
								NickName = mySqlDataReader.GetString("nickName"),
								CharacterNum = mySqlDataReader.GetInt16("selectedCharacterNum"),
								familyUnitType = (byte)mySqlDataReader.GetInt16("familyUnitType"),
								coupleNum = mySqlDataReader.GetInt32("coupleNum"),
								coupleType = (short)mySqlDataReader.GetInt32("coupleType")
							};
							familyinfos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_familyGetFamilyInfo Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool FamilyCheckProposeCondition(string targetNickname, int myUserNum, bool isParents)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_familyCheckProposeCondition";
					mySqlCommand.Parameters.Add("targetNickname", MySqlDbType.String).Value = targetNickname;
					mySqlCommand.Parameters.Add("myUserNum", MySqlDbType.Int32).Value = myUserNum;
					mySqlCommand.Parameters.Add("isParents", MySqlDbType.Int16).Value = isParents;
					mySqlCommand.Parameters.Add("ret", MySqlDbType.Int32).Direction = ParameterDirection.Output;
					using (mySqlCommand.ExecuteReader(CommandBehavior.SingleRow))
					{
						if (Convert.ToInt32(mySqlCommand.Parameters["ret"].Value) == 0)
						{
							return true;
						}
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_familyCheckProposeCondition Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool MakeFamily(Account User, string NickName, bool bParents, out List<FamilyInfo> familyinfos)
		{
			familyinfos = new List<FamilyInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_familyMakeFamily";
					mySqlCommand.Parameters.Add("myUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("targetNickName", MySqlDbType.String).Value = NickName;
					mySqlCommand.Parameters.Add("myCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("bParents", MySqlDbType.Int16).Value = bParents;
					mySqlCommand.Parameters.Add("covenantItemNum", MySqlDbType.Int32).Value = 7601;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							FamilyInfo item = new FamilyInfo
							{
								NickName = mySqlDataReader.GetString("nickName"),
								CharacterNum = mySqlDataReader.GetInt16("selectedCharacterNum"),
								familyUnitType = (byte)mySqlDataReader.GetInt16("familyUnitType"),
								coupleNum = mySqlDataReader.GetInt32("coupleNum"),
								coupleType = (short)mySqlDataReader.GetInt32("coupleType")
							};
							familyinfos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_familyMakeFamily Error:{0}", ex.Message);
				return false;
			}
		}

		private static bool DissolveFamily(Account User, string NickName, bool bParents, out List<FamilyInfo> familyinfos)
		{
			familyinfos = new List<FamilyInfo>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_familyDissolveFamily";
					mySqlCommand.Parameters.Add("myUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("targetNickName", MySqlDbType.String).Value = NickName;
					mySqlCommand.Parameters.Add("myCoupleNum", MySqlDbType.Int32).Value = User.CoupleInfo.CoupleNum;
					mySqlCommand.Parameters.Add("bParents", MySqlDbType.Int16).Value = bParents;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							FamilyInfo item = new FamilyInfo
							{
								NickName = mySqlDataReader.GetString("nickName"),
								CharacterNum = mySqlDataReader.GetInt16("selectedCharacterNum"),
								familyUnitType = (byte)mySqlDataReader.GetInt16("familyUnitType"),
								coupleNum = mySqlDataReader.GetInt32("coupleNum"),
								coupleType = (short)mySqlDataReader.GetInt32("coupleType")
							};
							familyinfos.Add(item);
						}
						return true;
					}
				}
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("usp_familyDissolveFamily Error:{0}", ex.Message);
				return false;
			}
		}
	}
}
