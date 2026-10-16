using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using AgentServer.Packet;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUserInfo : NetPacket
	{
		public LoginUserInfo(Account User, UserLoginInfo logininfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_OK_ACK);
			long value = 0L;
			byte value2 = 20;
			byte value3 = 20;
			// Always re-read TR from UserInfoGame — stale/wrong login column left UI at 781k while DB was 281k.
			ShopHandle.RefreshGameMoney(User);
			ns.Write(User.Session);
			ns.Write((short)(-1));
			ns.Write(25687);
			ns.Write(User.TR);
			ns.Write(User.Exp);
			ns.Write(User.Attribute);
			ns.Write(logininfo.playingTime);
			ns.Write(User.CoupleInfo.CoupleNum);
			ns.Write(User.CoupleInfo.CoupleType);
			long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			WriteCoupleBlockThenRelay(ns, User, nowMs);
			ns.Write(2018008620);
			ns.Write(0);
			ns.Write(value: true);
			ns.Write(0);
			// Client LOGIN_OK read #20 is an int64 at body 171. Non-zero opens HackAlarmPopup.
			// The old byte 1 plus PartyType 0x8C made that value 0x8C00000000000100.
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(nowMs);
			ns.Write(User.GameOption);
			ns.Write(601);
			// Always re-read fdMP — login SP sometimes returns stale/5 while UserShuInfo is 20.
			// Thai HUD "%d/%d" reads remain+max as the two ints after 601 (same 8 bytes KR
			// used for nowMs). Writing nowMs there made remain = timestamp low dword
			// (e.g. -1157562368 / -563823871) while max came from the following shuMP=20.
			int shuMp = ReadShuMpForLogin(User.UserNum, logininfo.shuMP);
			logininfo.shuMP = shuMp;
			ns.Write(shuMp); // remainMP
			ns.Write(20); // maxMP
			ns.Write(shuMp); // legacy KR shuMP before FreePass (keeps FreePass aligned)
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_freepass_getUserDesc";
				mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader.Read();
				User.FreePassType = Convert.ToInt32(mySqlDataReader["type"]);
				value = (Convert.IsDBNull(mySqlDataReader["expireTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["expireTime"])));
			}
			ns.Write(User.FreePassType);
			ns.Write(value);
			using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection2.Open();
				using MySqlCommand mySqlCommand2 = new MySqlCommand(string.Empty, mySqlConnection2);
				mySqlCommand2.Parameters.Clear();
				mySqlCommand2.CommandType = CommandType.StoredProcedure;
				mySqlCommand2.CommandText = "usp_storage_getUserDesc";
				mySqlCommand2.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
				using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader(CommandBehavior.SingleRow);
				mySqlDataReader2.Read();
				User.FreePassType = Convert.ToInt32(mySqlDataReader2["type"]);
				value = (Convert.IsDBNull(mySqlDataReader2["expireTime"]) ? 0 : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader2["expireTime"])));
				value2 = ClampStorageByte(mySqlDataReader2["maxSavableCount"], 127);
				value3 = ClampStorageByte(mySqlDataReader2["maxReceivableCount"], 20);
			}
			ns.Write(value2);
			ns.Write(value3);
			ns.Write(User.FreePassType);
			ns.Write(value);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write((byte)0);
			ns.Write(User.TopRank);
			ns.Write(value: false);
			// PacketSize=268 RemainSize=5 → client consumes 263. Pad to exact body length.
			int target = 263;
			int extra = target - (int)ns.Length;
			if (extra > 0)
			{
				ns.UnderlyingStream.Write(new byte[extra], 0, extra);
			}
			_ = last;
		}

		// LOGIN_OK after coupleNum/type (trRelease 0x82E1B7 / 0x71ECB0):
		//   8+4+4 gap, then couple struct (4,4, uint16-len name, 8,8,8, 12, 4,8, 4,4, 1, 4,4),
		//   then byte, byte, 8, then 16-byte relay sockaddr.
		// Putting the name too early made 0x6F11F0 read timestamp bytes B6-8B as length 35766
		// and overpop: "Overpop packet (popRawData) 70 269 35766 2".
		private static void WriteCoupleBlockThenRelay(PacketWriter ns, Account User, long nowMs)
		{
			ns.Write(0L);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(User.CoupleInfo.MateName ?? string.Empty);
			ns.Write(User.CoupleInfo.CreateTime);
			ns.Write(User.CoupleInfo.MarriedTime);
			ns.Write(User.CoupleInfo.RingChangedTime);
			ns.Write(User.CoupleInfo.CoupleRingNum);
			ns.Write(User.CoupleInfo.MaxRingDays);
			ns.Write(User.CoupleInfo.CoupleLevel);
			ns.Write((short)0);
			ns.Write(User.CoupleInfo.CondDays);
			ns.Write(0L);
			ns.Write(User.CoupleInfo.AccumulateExp);
			ns.Write(User.CoupleInfo.CouplePoint);
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write(nowMs);
			WriteRelaySockaddr(ns);
		}

		private static void WriteRelaySockaddr(PacketWriter ns)
		{
			// Must match settings RelayServerIP. Use RelayAdvertiseIP from settings
			// (local TEST-NET alias via SETUP_RELAY_ALIAS.bat — not a remote server).
			string relayIp = Conf.GetRelayAdvertiseIP();
			ns.Write((short)2);
			byte[] port = BitConverter.GetBytes((short)Conf.RelayPort).Reverse().ToArray();
			byte[] ip = IPAddress.Parse(relayIp).GetAddressBytes();
			ns.Write(port, 0, 2);
			ns.Write(ip, 0, 4);
			ns.Write(new byte[8], 0, 8);
		}

		private static byte ClampStorageByte(object raw, byte fallback)
		{
			try
			{
				int v = Convert.ToInt32(raw);
				if (v < 0)
				{
					return fallback;
				}
				if (v > 255)
				{
					return 255;
				}
				return (byte)v;
			}
			catch
			{
				return fallback;
			}
		}

		private static int ReadShuMpForLogin(int userNum, int fallback)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT fdMP FROM UserShuInfo WHERE fdUserNum=@u LIMIT 1", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				object o = cmd.ExecuteScalar();
				if (o != null && o != DBNull.Value)
				{
					fallback = Convert.ToInt32(o);
				}
			}
			catch
			{
			}
			if (fallback < 0)
			{
				return 0;
			}
			if (fallback > 20)
			{
				return 20;
			}
			return fallback;
		}
	}
}
