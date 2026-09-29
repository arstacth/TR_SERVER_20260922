using System;
using System.Collections.Generic;
using System.Data;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Guild;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public static class GuildMatchHandle
	{
		public static void Handle_GuildMatch_GetRankRange(ClientConnection Client, PacketReader reader, byte last)
		{
			byte rankKind = reader.ReadByte();
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32() - 1;
			int endRank = num + num2;
			guildMatchGetRankRange(num, endRank, rankKind, out var GuildMatchRankList);
			Client.SendAsync(new GuildMatch_GetRankRange(rankKind, GuildMatchRankList, last));
		}

		public static void Handle_GuildMatch_GetRankMyGuild(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			byte rankKind = reader.ReadByte();
			guildMatchGetRankMyGuild(currentAccount.GuildNum, rankKind, out var GuildMatchMyRank);
			Client.SendAsync(new GuildMatch_GetRankMyGuild(rankKind, GuildMatchMyRank, last));
		}

		public static void Handle_LookingForGuildMatch(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short mode = reader.ReadLEInt16();
			ServerStatus.ToRoomServer(new RM_LookingForGuildMatch(currentAccount, mode, last), currentAccount.RoomServerID);
		}

		public static void Handle_PickGuildForMatch(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short mode = reader.ReadLEInt16();
			ServerStatus.ToRoomServer(new RM_PickGuildForMatch(currentAccount, mode, last), currentAccount.RoomServerID);
		}

		public static void Handle_CancelLookingForGuildMatch(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			ServerStatus.ToRoomServer(new RM_CancelLookingForGuildMatch(currentAccount, last), currentAccount.RoomServerID);
		}

		public static void Handle_ChooseGuildForMatch(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short status = reader.ReadLEInt16();
			int guildmatchroomid = reader.ReadLEInt32();
			ServerStatus.ToRoomServer(new RM_ChooseGuildForMatch(currentAccount, status, guildmatchroomid, last), currentAccount.RoomServerID);
		}

		private static void guildMatchGetLatestMatch(int GuildNum, short matchType, out List<GuildLastMatchInfo> GuildLastMatchList)
		{
			GuildLastMatchList = new List<GuildLastMatchInfo>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMatchGetLatestMatch";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = GuildNum;
				mySqlCommand.Parameters.Add("matchType", MySqlDbType.Int16).Value = matchType;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildLastMatchInfo item = new GuildLastMatchInfo
					{
						opponentName = mySqlDataReader.GetString("opponentName"),
						result = mySqlDataReader.GetBoolean("result"),
						datetime = Utility.ConvertToTimestamp(mySqlDataReader.GetDateTime("datetime"))
					};
					GuildLastMatchList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMatchGetLatestMatch Error:{0}", ex.Message);
			}
		}

		private static void guildMatchGetRankRange(int startRank, int endRank, short rankKind, out List<GuildMatchRank> GuildMatchRankList)
		{
			GuildMatchRankList = new List<GuildMatchRank>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMatchGetRankRange";
				mySqlCommand.Parameters.Add("startRank", MySqlDbType.Int32).Value = startRank;
				mySqlCommand.Parameters.Add("endRank", MySqlDbType.Int32).Value = endRank;
				mySqlCommand.Parameters.Add("rankKind", MySqlDbType.Int16).Value = rankKind;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					GuildMatchRank item = new GuildMatchRank
					{
						rank = mySqlDataReader.GetInt32("RANK"),
						guildNum = mySqlDataReader.GetInt32("guildNum"),
						guildName = mySqlDataReader.GetString("guildName"),
						win = mySqlDataReader.GetInt32("win"),
						lose = mySqlDataReader.GetInt32("lose"),
						level = mySqlDataReader.GetInt32("level")
					};
					GuildMatchRankList.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMatchGetRankRange Error:{0}", ex.Message);
			}
		}

		private static void guildMatchGetRankMyGuild(int GuildNum, short rankKind, out GuildMatchRank GuildMatchMyRank)
		{
			GuildMatchMyRank = new GuildMatchRank();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_guildMatchGetRankMyGuild";
				mySqlCommand.Parameters.Add("guildNum", MySqlDbType.Int32).Value = GuildNum;
				mySqlCommand.Parameters.Add("rankKind", MySqlDbType.Int16).Value = rankKind;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					GuildMatchMyRank.rank = mySqlDataReader.GetInt32("RANK");
					GuildMatchMyRank.guildNum = mySqlDataReader.GetInt32("guildNum");
					GuildMatchMyRank.guildName = mySqlDataReader.GetString("guildName");
					GuildMatchMyRank.win = mySqlDataReader.GetInt32("win");
					GuildMatchMyRank.lose = mySqlDataReader.GetInt32("lose");
					GuildMatchMyRank.level = mySqlDataReader.GetInt32("level");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_guildMatchGetRankMyGuild Error:{0}", ex.Message);
			}
		}
	}
}
