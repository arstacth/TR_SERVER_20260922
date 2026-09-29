using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.User;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class RankHandle
	{
		public static void Handle_GetRankInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			byte b = reader.ReadByte();
			int startRank = reader.ReadLEInt32();
			int showcount = reader.ReadLEInt32();
			byte b2 = reader.ReadByte();
			eRequestRankKind eRequestRankKind = (eRequestRankKind)b;
			Log.Information("RANK_BY_NUM kind={0} start={1} count={2} detail={3}", b, startRank, showcount, b2);
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_COUPLE)
			{
				coupleGetRankRange(startRank, showcount, b2, out var ranklist);
				Client.SendAsync(new GetCoupleRankRange_ACK(b, ranklist, last));
				return;
			}
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_NORMAL)
			{
				b2 = 0;
			}
			eRequestRankKind dbKind = RankKindForDb(eRequestRankKind);
			DB_getRankRange(dbKind, bSearchByNickname: false, startRank, showcount, b2, string.Empty, out var ranklist2);
			Client.SendAsync(new GetRankInfo_ALL(eRequestRankKind, ranklist2, last));
		}

		public static void Handle_GetMyRankInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte b = reader.ReadByte();
			byte b2 = reader.ReadByte();
			eRequestRankKind eRequestRankKind = (eRequestRankKind)b;
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_COUPLE)
			{
				coupleGetRankNickName(currentAccount.NickName, b2, out var info);
				if (info == null)
				{
					// Empty couple board was FAIL 65; unranked stub keeps UI usable.
					info = new CoupleRankInfo
					{
						coupleNum = 0,
						point = 0,
						rank = 0,
						level = 1,
						femaleNickName = currentAccount.NickName,
						maleNickName = string.Empty
					};
				}
				Client.SendAsync(new GetMyCoupleRankInfo_ACK(b, b2, info, last));
				return;
			}
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_NORMAL)
			{
				b2 = 0;
			}
			eRequestRankKind dbKind = RankKindForDb(eRequestRankKind);
			DB_getMyRank(dbKind, currentAccount.NickName, b2, out var myrank);
			if (myrank == null)
			{
				// Empty farm/sports tabs were FAIL 65; send unranked stub so UI stays usable.
				myrank = new CRankListData
				{
					m_nickname = currentAccount.NickName,
					m_ranking = 0,
					m_experienceValue = 0,
					m_level = 1
				};
			}
			Client.SendAsync(new GetMyRankInfo_ALL(eRequestRankKind, b2, myrank, last));
		}

		public static void Handle_SearchRank(ClientConnection Client, PacketReader reader, byte last)
		{
			byte b = reader.ReadByte();
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int showcount = reader.ReadLEInt32();
			byte b2 = reader.ReadByte();
			eRequestRankKind eRequestRankKind = (eRequestRankKind)b;
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_COUPLE)
			{
				coupleGetRankSearch(text, showcount, b2, out var ranklist);
				Client.SendAsync(new GetCoupleRankRange_ACK(b, ranklist, last));
				return;
			}
			if (eRequestRankKind == eRequestRankKind.eRequestRankKind_NORMAL)
			{
				b2 = 0;
			}
			eRequestRankKind dbKind = RankKindForDb(eRequestRankKind);
			DB_getRankRange(dbKind, bSearchByNickname: true, 0, showcount, b2, text, out var ranklist2);
			Client.SendAsync(new GetRankInfo_ALL(eRequestRankKind, ranklist2, last));
		}

		private static eRequestRankKind RankKindForDb(eRequestRankKind rankKind)
		{
			if (rankKind == eRequestRankKind.eRequestRankKind_ITEM_COLLECTION || (int)rankKind >= 11)
			{
				return eRequestRankKind.eRequestRankKind_ITEM_COLLECTION;
			}
			if (rankKind == eRequestRankKind.eRequestRankKind_FARM)
			{
				return eRequestRankKind.eRequestRankKind_FARM;
			}
			return eRequestRankKind.eRequestRankKind_NORMAL;
		}

		private static void EnsureFarmRank()
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("usp_Farm_makerank", mySqlConnection);
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_Farm_makerank Error:{0}", ex.Message);
			}
		}

		private static void DB_getRankRange(eRequestRankKind rankKind, bool bSearchByNickname, int startRank, int showcount, byte rankDetailKind, string strSearchNickname, out List<CRankListData> ranklist)
		{
			ranklist = new List<CRankListData>();
			if (rankKind == eRequestRankKind.eRequestRankKind_FARM)
			{
				EnsureFarmRank();
			}
			int num = startRank + showcount - 1;
			string text = string.Empty;
			if (bSearchByNickname)
			{
				switch (rankKind)
				{
				case eRequestRankKind.eRequestRankKind_NORMAL:
					text += "usp_Rank_Search";
					break;
				case eRequestRankKind.eRequestRankKind_SPORTS:
					text += "usp_sportsGetRankSearch";
					break;
				case eRequestRankKind.eRequestRankKind_FARM:
					text += "usp_Farm_getRankSearch";
					break;
				case eRequestRankKind.eRequestRankKind_COUPLE:
					text += "usp_coupleGetRankSearch";
					break;
				case eRequestRankKind.eRequestRankKind_ITEM_COLLECTION:
					text += "usp_itemCollection_getRankSearch";
					break;
				}
			}
			else
			{
				switch (rankKind)
				{
				case eRequestRankKind.eRequestRankKind_NORMAL:
					text += "usp_Rank_getRange";
					break;
				case eRequestRankKind.eRequestRankKind_SPORTS:
					text += "usp_sportsGetRankRange";
					break;
				case eRequestRankKind.eRequestRankKind_FARM:
					text += "usp_Farm_getRankRange";
					break;
				case eRequestRankKind.eRequestRankKind_COUPLE:
					text += "usp_coupleGetRankRange";
					break;
				case eRequestRankKind.eRequestRankKind_ITEM_COLLECTION:
					text += "usp_itemCollection_getRankRange";
					break;
				}
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = text;
				if (bSearchByNickname)
				{
					switch (rankKind)
					{
					case eRequestRankKind.eRequestRankKind_NORMAL:
					case eRequestRankKind.eRequestRankKind_COUPLE:
						mySqlCommand.Parameters.Add("usernickname", MySqlDbType.VarString).Value = strSearchNickname;
						mySqlCommand.Parameters.Add("showcount", MySqlDbType.Int32).Value = showcount;
						mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankDetailKind;
						break;
					case eRequestRankKind.eRequestRankKind_FARM:
						mySqlCommand.Parameters.Add("usernickname", MySqlDbType.VarString).Value = strSearchNickname;
						mySqlCommand.Parameters.Add("showcount", MySqlDbType.Int32).Value = showcount;
						break;
					default:
						mySqlCommand.Parameters.Add("nickName", MySqlDbType.VarString).Value = strSearchNickname;
						mySqlCommand.Parameters.Add("showCount", MySqlDbType.Int32).Value = showcount;
						break;
					}
				}
				else
				{
					switch (rankKind)
					{
					case eRequestRankKind.eRequestRankKind_NORMAL:
					case eRequestRankKind.eRequestRankKind_COUPLE:
						mySqlCommand.Parameters.Add("startRank", MySqlDbType.Int32).Value = startRank;
						mySqlCommand.Parameters.Add("showCount", MySqlDbType.Int32).Value = num;
						mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankDetailKind;
						break;
					case eRequestRankKind.eRequestRankKind_ITEM_COLLECTION:
						mySqlCommand.Parameters.Add("startRank", MySqlDbType.Int32).Value = startRank;
						mySqlCommand.Parameters.Add("showCount", MySqlDbType.Int32).Value = showcount;
						break;
					default:
						mySqlCommand.Parameters.Add("startRank", MySqlDbType.Int32).Value = startRank;
						mySqlCommand.Parameters.Add("endRank", MySqlDbType.Int32).Value = num;
						break;
					}
				}
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (eRequestRankKind.eRequestRankKind_ITEM_COLLECTION == rankKind)
				{
					// GameDataItemCollectionRank matches ItemCollection UI points.
					// renewal_rank was stale (111837 vs book 168863) → wrong board.
					ranklist.Clear();
					try
					{
						using MySqlConnection collConn = new MySqlConnection(Conf.Connstr);
						collConn.Open();
						using MySqlCommand collCmd = new MySqlCommand(
							"SELECT g.fdRank, g.fdNickName, g.fdPoint, COALESCE(r.fdUserLevel, 1) "
							+ "FROM GameDataItemCollectionRank g "
							+ "LEFT JOIN gamedataitem_collectionrenewal_rank r ON r.fdUserNum = g.fdUserNum "
							+ "WHERE g.fdRank BETWEEN @s AND @e ORDER BY g.fdRank",
							collConn);
						collCmd.Parameters.AddWithValue("@s", startRank);
						collCmd.Parameters.AddWithValue("@e", startRank + showcount - 1);
						using MySqlDataReader collReader = collCmd.ExecuteReader();
						while (collReader.Read())
						{
							ranklist.Add(new CRankListData
							{
								m_ranking = collReader.GetInt32(0),
								m_nickname = collReader.GetString(1),
								m_experienceValue = collReader.GetInt64(2),
								m_level = collReader.GetInt32(3),
								m_numbers = 0
							});
						}
					}
					catch (Exception exColl)
					{
						Log.Warning("GameDataItemCollectionRank range: {0}", exColl.Message);
					}
					if (ranklist.Count == 0)
					{
						while (mySqlDataReader.Read())
						{
							CRankListData item = new CRankListData
							{
								m_ranking = mySqlDataReader.GetInt32(0),
								m_nickname = mySqlDataReader.GetString(1),
								m_experienceValue = mySqlDataReader.GetInt64(2),
								m_numbers = 0
							};
							ranklist.Add(item);
						}
					}
				}
				else if (rankKind == eRequestRankKind.eRequestRankKind_NORMAL)
				{
					while (mySqlDataReader.Read())
					{
						CRankListData item2 = new CRankListData
						{
							m_ranking = mySqlDataReader.GetInt32("Rank"),
							m_nickname = mySqlDataReader.GetString("nickname"),
							m_experienceValue = mySqlDataReader.GetInt64("EXP"),
							m_experienceValue2 = mySqlDataReader.GetInt64("EXP"),
							m_numbers = 0
						};
						ranklist.Add(item2);
					}
				}
				else
				{
					while (mySqlDataReader.Read())
					{
						CRankListData item3 = new CRankListData
						{
							m_ranking = mySqlDataReader.GetInt32("RANK"),
							m_nickname = mySqlDataReader.GetString("nickname"),
							m_experienceValue = mySqlDataReader.GetInt64("EXP"),
							m_numbers = 0
						};
						ranklist.Add(item3);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("{0} Error:{1}", text, ex.Message);
			}
		}

		private static void DB_getMyRank(eRequestRankKind rankKind, string strSearchNickname, byte rankDetailKind, out CRankListData myrank)
		{
			myrank = null;
			if (rankKind == eRequestRankKind.eRequestRankKind_FARM)
			{
				EnsureFarmRank();
			}
			string text = string.Empty;
			switch (rankKind)
			{
			case eRequestRankKind.eRequestRankKind_NORMAL:
				text += "usp_Rank_getNickname";
				break;
			case eRequestRankKind.eRequestRankKind_SPORTS:
				text += "usp_sportsGetRankNickname";
				break;
			case eRequestRankKind.eRequestRankKind_FARM:
				text += "usp_Farm_getRankNickName";
				break;
			case eRequestRankKind.eRequestRankKind_COUPLE:
				text += "usp_coupleGetRankNickName";
				break;
			case eRequestRankKind.eRequestRankKind_ITEM_COLLECTION:
				try
				{
					using MySqlConnection icConn = new MySqlConnection(Conf.Connstr);
					icConn.Open();
					using MySqlCommand oldCmd = new MySqlCommand(
						"SELECT g.fdRank, g.fdPoint, COALESCE(r.fdUserLevel, 1) "
						+ "FROM GameDataItemCollectionRank g "
						+ "LEFT JOIN gamedataitem_collectionrenewal_rank r ON r.fdUserNum = g.fdUserNum "
						+ "WHERE g.fdNickName = @n LIMIT 1",
						icConn);
					oldCmd.Parameters.AddWithValue("@n", strSearchNickname);
					using MySqlDataReader oldReader = oldCmd.ExecuteReader();
					if (oldReader.Read())
					{
						myrank = new CRankListData();
						myrank.m_nickname = strSearchNickname;
						myrank.m_ranking = oldReader.GetInt32(0);
						myrank.m_experienceValue = oldReader.GetInt64(1);
						myrank.m_level = oldReader.GetInt32(2);
					}
				}
				catch (Exception exIc)
				{
					Log.Error("collection my-rank Error:{0}", exIc.Message);
				}
				return;
			}
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = text;
				switch (rankKind)
				{
				case eRequestRankKind.eRequestRankKind_NORMAL:
				case eRequestRankKind.eRequestRankKind_COUPLE:
					mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = strSearchNickname;
					mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int32).Value = rankDetailKind;
					break;
				default:
					mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = strSearchNickname;
					break;
				}
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				if (rankKind == eRequestRankKind.eRequestRankKind_NORMAL)
				{
					if (mySqlDataReader.HasRows && mySqlDataReader.Read())
					{
						myrank = new CRankListData();
						myrank.m_nickname = strSearchNickname;
						myrank.m_ranking = mySqlDataReader.GetInt32("Rank");
						myrank.m_experienceValue = mySqlDataReader.GetInt64("EXP");
						myrank.m_experienceValue2 = mySqlDataReader.GetInt64("EXP");
					}
				}
				else if (mySqlDataReader.HasRows && mySqlDataReader.Read())
				{
					myrank = new CRankListData();
					myrank.m_nickname = strSearchNickname;
					myrank.m_ranking = mySqlDataReader.GetInt32("RANK");
					myrank.m_experienceValue = mySqlDataReader.GetInt64("EXP");
				}
			}
			catch (Exception ex)
			{
				Log.Error("{0} Error:{1}", text, ex.Message);
			}
		}

		private static void coupleGetRankRange(int startRank, int showcount, short detailRank, out List<CoupleRankInfo> ranklist)
		{
			ranklist = new List<CoupleRankInfo>();
			int num = startRank + showcount - 1;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleGetRankRange";
				mySqlCommand.Parameters.Add("startRank", MySqlDbType.Int32).Value = startRank;
				mySqlCommand.Parameters.Add("endRank", MySqlDbType.Int32).Value = num;
				mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int16).Value = detailRank;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					CoupleRankInfo item = new CoupleRankInfo
					{
						rank = mySqlDataReader.GetInt32("rank"),
						coupleNum = mySqlDataReader.GetInt32("coupleNum"),
						point = mySqlDataReader.GetInt32("point"),
						femaleNickName = mySqlDataReader.GetString("femaleNickName"),
						maleNickName = mySqlDataReader.GetString("maleNickName"),
						level = mySqlDataReader.GetInt32("level")
					};
					ranklist.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleGetRankRange Error:{0}", ex.Message);
			}
		}

		private static void coupleGetRankNickName(string nickname, short detailRank, out CoupleRankInfo info)
		{
			info = null;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleGetRankNickName";
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
				mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int16).Value = detailRank;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					info = new CoupleRankInfo
					{
						rank = mySqlDataReader.GetInt32("rank"),
						coupleNum = mySqlDataReader.GetInt32("coupleNum"),
						point = mySqlDataReader.GetInt32("point"),
						femaleNickName = mySqlDataReader.GetString("femaleNickName"),
						maleNickName = mySqlDataReader.GetString("maleNickName"),
						level = mySqlDataReader.GetInt32("level")
					};
				}
			}
			catch (Exception ex)
			{
				info = null;
				Log.Error("usp_coupleGetRankNickName Error:{0}", ex.Message);
			}
		}

		private static void coupleGetRankSearch(string nickname, int showcount, short detailRank, out List<CoupleRankInfo> ranklist)
		{
			ranklist = new List<CoupleRankInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_coupleGetRankSearch";
				mySqlCommand.Parameters.Add("usernickname", MySqlDbType.VarString).Value = nickname;
				mySqlCommand.Parameters.Add("showcount", MySqlDbType.Int32).Value = showcount;
				mySqlCommand.Parameters.Add("detailRank", MySqlDbType.Int16).Value = detailRank;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					CoupleRankInfo item = new CoupleRankInfo
					{
						rank = mySqlDataReader.GetInt32("rank"),
						coupleNum = mySqlDataReader.GetInt32("coupleNum"),
						point = mySqlDataReader.GetInt32("point"),
						femaleNickName = mySqlDataReader.GetString("femaleNickName"),
						maleNickName = mySqlDataReader.GetString("maleNickName"),
						level = mySqlDataReader.GetInt32("level")
					};
					ranklist.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_coupleGetRankSearch Error:{0}", ex.Message);
			}
		}
	}
}
