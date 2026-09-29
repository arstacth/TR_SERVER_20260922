using System;
using System.Data;
using System.Globalization;
using AgentServer.Structuring;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class LoginFarm_GetMyFarmInfo : NetPacket
	{
		public LoginFarm_GetMyFarmInfo(Account User, byte last)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_GetMyFarmInfo";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					User.MyFarmUniqueNum = Convert.ToInt32(mySqlDataReader["FarmIndex"]);
					MyFarmInfo myFarmInfo2 = (User.MyFarmInfo = new MyFarmInfo
					{
						FarmTypeNum = Convert.ToInt32(mySqlDataReader["FarmTypeNum"]),
						FarmSkyTypeNum = 0,
						FarmWeatherTypeNum = 0,
						FarmName = mySqlDataReader["FarmName"].ToString(),
						MasterName = mySqlDataReader["MasterName"].ToString(),
						ExpireTime = (Convert.IsDBNull(mySqlDataReader["ExpireDateTime"]) ? 1842465389770955L : FarmDateInvariant(mySqlDataReader["ExpireDateTime"])),
						CreateTime = (Convert.IsDBNull(mySqlDataReader["CreateDateTime"]) ? 1842465389770955L : FarmDateInvariant(mySqlDataReader["CreateDateTime"])),
						isPublic = false,
						TotalCount = Convert.ToInt32(mySqlDataReader["TotalVisitedCount"]),
						TodaysVisitorCount = Convert.ToInt32(mySqlDataReader["TodaysVisitorCount"]),
						PremiumFarmUsing = false,
						PremiumFarmExpireDateTime = 0L,
						farmExp = Convert.ToInt32(mySqlDataReader["farmExp"])
					});
				}
			}
			if (User.MyFarmInfo != null)
			{
				User.MyFarmInfo.farmExp = global::AgentServer.Packet.FarmHandle.EnsureFarmExp(User.UserNum, User.MyFarmInfo.farmExp);
			}
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetMyFarmInfo_ACK);
			ns.Write(0);
			ns.Write(Utility.PackFarmUnique(User.MyFarmUniqueNum));
			ns.Write(User.MyFarmInfo.FarmWeatherTypeNum);
			ns.Write(User.MyFarmInfo.FarmSkyTypeNum);
			// packed: type 0 → client cmove farm_01 (type 18 became farm_1179648.trv)
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(User.MyFarmInfo.FarmName);
			ns.WriteAnsiFixed_intSize(User.MyFarmInfo.MasterName);
			ns.Write(User.MyFarmInfo.ExpireTime);
			ns.Write(User.MyFarmInfo.CreateTime);
			ns.Write((byte)1);
			ns.Write(User.MyFarmInfo.isPublic);
			ns.Write((byte)(User.MyFarmUniqueNum > 0 ? 1 : 0));
			ns.Write(User.MyFarmInfo.TotalCount);
			ns.Write(User.MyFarmInfo.TodaysVisitorCount);
			ns.Write((byte)(User.MyFarmUniqueNum > 0 ? 1 : 0));
			ns.Fill(2);
			ns.Write(User.MyFarmInfo.PremiumFarmUsing);
			ns.Write(User.MyFarmInfo.PremiumFarmExpireDateTime);
			// farmExp is int64 on the Thai client. Packing unique here made rank 196608.
			// 0 indexes a blank farmrankicon; official 0 exp is rank 1.
			ns.Write(Utility.FarmHudExp(User.MyFarmInfo.farmExp));
			ns.Write(0);
		}

		private static long FarmDateInvariant(object value)
		{
			DateTime dt;
			if (value is DateTime dateTime)
			{
				dt = dateTime;
			}
			else if (!DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dt))
			{
				return 1842465389770955L;
			}
			if (dt.Year > 0 && dt.Year < 1900)
			{
				dt = dt.AddYears(543);
			}
			long ms = Utility.ConvertToTimestamp(dt);
			return ms > 0L ? ms : 1842465389770955L;
		}
	}
}
