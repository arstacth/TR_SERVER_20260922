using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Map;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class SingleChallengeHandle
	{
		public static void Handle_GetUserInfo(ClientConnection Client, PacketReader reader, byte last)
		{
			_ = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string nickName = reader.ReadBig5StringSafe(fixedLength);
			Client.SendAsync(new SingleChallengeUserInfo(nickName, last));
		}

		public static void Handle_MapSummary(ClientConnection Client, PacketReader reader, byte last)
		{
			int count = 0;
			if (reader.Remaining >= 4)
			{
				count = reader.ReadLEInt32();
			}
			if (count < 0)
			{
				count = 0;
			}
			int max = reader.Remaining / 4;
			if (count > max)
			{
				count = max;
			}
			List<int> maps = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				maps.Add(reader.ReadLEInt32());
			}
			Log.Warning("CHALLENGE_MAP_SUMMARY user={0} maps={1}", Client.CurrentAccount != null ? Client.CurrentAccount.UserID : "?", count);
			Client.SendAsync(new SingleChallengeMapSummaryAck(Client.CurrentAccount.UserNum, maps, last));
		}

		public static void Handle_StartChallenge(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num <= 0)
			{
				return;
			}
			if (SingleChallengeHolder.MapInfos.ContainsKey(num))
			{
				try
				{
					ChallengeStart(currentAccount, num);
				}
				catch (Exception ex)
				{
					Log.Warning(ex, "CHALLENGE_MAP_START db failed map={0} user={1}", num, currentAccount != null ? currentAccount.UserID : "?");
				}
			}
			else if (MapHolder.MapInfos.ContainsKey(num))
			{
				// Classic maps (5xxx) exist in tblMapInfo but not essenChallengeModeMapInfo.
				Log.Warning("CHALLENGE_MAP_START map={0} user={1} not in SingleChallengeHolder; ACK anyway", num, currentAccount != null ? currentAccount.UserID : "?");
			}
			else
			{
				Log.Warning("CHALLENGE_MAP_START map={0} user={1} unknown; ACK anyway", num, currentAccount != null ? currentAccount.UserID : "?");
			}
			currentAccount.ChallengeMapNum = num;
			currentAccount.ChallengeStartTime = Utility.CurrentTimeMilliseconds();
			Client.SendAsync(new StartChallengeOK(last));
		}

		public static void Handle_ChallengeAction(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int finishTime = (int)(Utility.CurrentTimeMilliseconds() - currentAccount.ChallengeStartTime);
			if (num == 0)
			{
				if (SingleChallengeHolder.MapInfos.TryGetValue(currentAccount.ChallengeMapNum, out var value))
				{
					byte medalType = value.OrderBy((ChallengeMapInfo o) => o.GoalSec).FirstOrDefault((ChallengeMapInfo f) => f.GoalSec >= finishTime)?.MedalType ?? 0;
					Client.SendAsync(new SingleChallengeGoalInOK(currentAccount.UserNum, currentAccount.ChallengeMapNum, medalType, finishTime, last));
				}
			}
			else
			{
				// ActionType 2/3 = give-up / game-over; still need map + play time for result UI.
				Client.SendAsync(new SingleChallengeAction(num, currentAccount.ChallengeMapNum, finishTime, last));
			}
		}

		private static void ChallengeStart(Account User, int MapNum)
		{
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_SingleChallenge_Start";
			mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("MapNum", MySqlDbType.Int32).Value = MapNum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			User.TR -= Convert.ToInt32(mySqlDataReader["TryCost"]);
		}
	}
}
