using System;
using System.Data;
using AgentServer.Structuring;
using AgentServer.Structuring.Farm;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class FirstLogin_CreateFarm_GetMyFarmInfo : NetPacket
	{
		public FirstLogin_CreateFarm_GetMyFarmInfo(Account User, byte last)
		{
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_Farm_CreateFarm";
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("pFarmName", MySqlDbType.VarChar).Value = "";
				mySqlCommand.Parameters.Add("pCheckFarmPeriod", MySqlDbType.Int32).Value = 0;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					User.MyFarmUniqueNum = Convert.ToInt32(mySqlDataReader["FarmIndex"]);
					MyFarmInfo myFarmInfo2 = (User.MyFarmInfo = new MyFarmInfo
					{
						FarmTypeNum = Convert.ToInt32(mySqlDataReader["FarmTypeNum"]),
						FarmSkyTypeNum = Convert.ToInt32(mySqlDataReader["FarmSkyBoxNum"]),
						FarmWeatherTypeNum = Convert.ToInt32(mySqlDataReader["FarmWeatherNum"]),
						FarmName = mySqlDataReader["FarmName"].ToString(),
						MasterName = User.NickName,
						ExpireTime = (Convert.IsDBNull(mySqlDataReader["ExpireDateTime"]) ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ExpireDateTime"]))),
						CreateTime = (Convert.IsDBNull(mySqlDataReader["CreateDateTime"]) ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["CreateDateTime"]))),
						isPublic = false,
						TotalCount = 0,
						TodaysVisitorCount = 0,
						PremiumFarmUsing = false,
						PremiumFarmExpireDateTime = 0L,
						farmExp = 0
					});
				}
			}
			if (User.MyFarmInfo != null)
			{
				User.MyFarmInfo.farmExp = global::AgentServer.Packet.FarmHandle.EnsureFarmExp(User.UserNum, User.MyFarmInfo.farmExp);
			}
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.CreateFarm_ACK);
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
			ns.Write(Utility.FarmHudExp(User.MyFarmInfo.farmExp));
			ns.Write(0);
			ns.Write((byte)1);
			_ = last;
		}
	}
}
