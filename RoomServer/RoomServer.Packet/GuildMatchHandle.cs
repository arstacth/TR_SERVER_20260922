using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Guild;
using Serilog;

namespace RoomServer.Packet
{
	public static class GuildMatchHandle
	{
		public static void Handle_LookingForGuildMatch(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				short mode = reader.ReadLEInt16();
				room?.BroadcastToAll(new LookingForGuildMatch(mode, last));
			}
		}

		public static void Handle_PickGuildForMatch(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				reader.ReadLEInt16();
				Interlocked.Increment(ref room.RandomSearchTime);
				value.SendAsync(new CannotFindGuildForMatch(last));
				if (room.RandomSearchTime >= 7)
				{
					room.IsSearchGuildMatch = false;
					room.RandomSearchTime = 0;
					room.BroadcastToAll(new GameRoom_CancelGuildMatching(last));
				}
			}
		}

		public static void Handle_CancelLookingForGuildMatch(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				room.IsSearchGuildMatch = false;
				room.RandomSearchTime = 0;
				room.BroadcastToAll(new GameRoom_CancelGuildMatching(last));
				room.BroadcastToAll(new CancelLookingForGuildMatch(last));
			}
		}

		public static void Handle_ChooseGuildForMatch(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			short num = reader.ReadLEInt16();
			NormalRoom roomForGuild = Rooms.GetRoomForGuild(reader.ReadLEInt32());
			if (room == null || roomForGuild == null)
			{
				return;
			}
			if (num == 1)
			{
				foreach (Account item in room.PlayerList())
				{
					item.IsGuildMatching = true;
				}
				foreach (Account item2 in roomForGuild.PlayerList())
				{
					item2.IsGuildMatching = true;
				}
				roomForGuild.RoomKindID = 81;
				roomForGuild.RivalGuildMatchRoomID = room.GuildMatchRoomID;
				roomForGuild.IsSearchGuildMatch = false;
				roomForGuild.BroadcastToAll(new GameRoom_CancelGuildMatching(last));
				room.BroadcastToAll(new GameRoom_GuildMatchRoomInfo(value, last));
				room.BroadcastToAll(new GetRivalGuildInfo(roomForGuild, num, master: true, last));
				roomForGuild.BroadcastToAll(new GetRivalGuildInfo(room, num, master: false, last));
				roomForGuild.BroadcastToAll(new GameRoom_UpdateGuildMatchRoomInfo(roomForGuild, last));
			}
			else
			{
				room.BroadcastToAll(new InvitingForGuildMatch(roomForGuild, num, master: true, last));
				roomForGuild.BroadcastToAll(new InvitingForGuildMatch(room, num, master: false, last));
			}
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
