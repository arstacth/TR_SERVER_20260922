using System;
using System.Collections.Generic;
using System.Data;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.User;
using Serilog;

namespace RoomServer.Packet
{
	public class CoupleHandle
	{
		public static void Handle_WeddingSetItem(Account User, PacketReader reader, byte last)
		{
			long cerremonyOfficerID = reader.ReadLEInt64();
			int cerremonyOfficerItemNum = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && room.RoomKindID == 75)
			{
				room.CerremonyOfficerID = cerremonyOfficerID;
				room.CerremonyOfficerItemNum = cerremonyOfficerItemNum;
				room.BroadcastToAll(new SetWeddingItemACK(last));
			}
		}

		public static void Handle_WeddingReady(Account User, byte last)
		{
			NormalRoom room = Rooms.GetRoom(User.CurrentRoomId);
			if (User.InGame && room.RoomKindID == 75)
			{
				room.Wedding(User);
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
