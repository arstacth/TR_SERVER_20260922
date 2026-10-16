using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Database;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using Akka.Actor;
using LocalCommons.Network;
using NetMsg.LBS;
using Serilog;

namespace AgentServer.Packet
{
	public static class GMCommandHandle
	{
		public class ClientCheckAutoBanACK : NetPacket
		{
			public ClientCheckAutoBanACK(int eBlockReason, byte last)
			{
				_ = last;
				// Packed Blocking_To_Me_ACK: reason int only. Trailer caused RemainSize=1
				// and client onRecvBlockingResult (JUMPKEY_MACRO / "hacked") UI glitch.
				ns.WriteOP(Opcodes.eServer_Blocking_To_Me_ACK);
				ns.Write(eBlockReason);
			}
		}

		public static void Handle_Notice(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			text = text.Substring(0, Math.Min(128, text.Length));
			if (currentAccount.Attribute == 0)
			{
				Log.Warning("!!!!!! [{0}] try send notice [{1}]", currentAccount.NickName, text);
				return;
			}
			ServerStatus.LBServerActor.Tell(new NoticePacket(text, last));
			Log.Information("[{0}] Send notice NoticeType : 0, noticeKind : 1,  {1}", currentAccount.NickName, text);
		}

		public static void Handle_DisconnectUser(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			if (currentAccount.Attribute == 0)
			{
				Log.Warning("!!!!!! [{0}] try to disconnect a user with no auth", currentAccount.NickName);
				return;
			}
			Log.Information("[{0}] try to disconnect user {1}", currentAccount.NickName, text);
			ServerStatus.LBServerActor.Tell(new DisconnectUser
			{
				NickName = text
			});
		}

		public static void Handle_FindGo(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			if (ClientConnection.CurrentAccounts.Values.Any((Account p) => p.NickName == nickname))
			{
				Account value = ClientConnection.CurrentAccounts.ToList().Find((KeyValuePair<int, Account> p) => p.Value.NickName == nickname).Value;
				if (currentAccount.Attribute == 0 && value.Attribute == 0)
				{
					value.Connection.Disconnect();
				}
			}
		}

		public static void Handle_ClientCheckAutoBan(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			reader.ReadLEInt32();
			// Client self-reports anti-cheat (e.g. JUMPKEY_MACRO=11). On a private
			// server these are false positives — log only, do not ban/kick.
			Log.Warning("[{0}] Client anti-cheat report eHackType={1} (ignored, no ban)", currentAccount.NickName, num);
			Client.SendAsync(new ClientCheckAutoBanACK(0, last));
		}

		public static void Handle_BlockUser(ClientConnection Client, PacketReader reader)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			byte blockreason = reader.ReadByte();
			byte b = reader.ReadByte();
			if (currentAccount.Attribute == 0)
			{
				Log.Warning("!!!!!! User [{0}] try to use {2} to {3} [{1}] with no auth!", currentAccount.NickName, text, (b == 0) ? "baduser" : "blockuser", (b == 0) ? "report" : "block");
			}
			else
			{
				Log.Information("User [{0}] try to use {2} to {3} [{1}]!", currentAccount.NickName, text, (b == 0) ? "baduser" : "blockuser", (b == 0) ? "report" : "block");
				AutoBan(text, blockreason, 10, currentAccount.UserNum, currentAccount.LastIp);
				ServerStatus.LBServerActor.Tell(new DisconnectUser
				{
					NickName = text
				});
			}
		}

		public static void AutoBan(string nickname, int blockreason, int blocktimeminute, int commandusernum, string ip)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_insertBlackList");
				mySqlCommandHelper.AddParamInt("commandusernum", commandusernum);
				mySqlCommandHelper.AddParamVarString("nickname", nickname);
				mySqlCommandHelper.AddParamInt("blockreason", blockreason);
				mySqlCommandHelper.AddParamInt("blocktime", blocktimeminute);
				mySqlCommandHelper.AddParamVarString("remoteIP", ip);
				mySqlCommandHelper.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Log.Error("usp_insertBlackList Error: {0}", ex.Message);
			}
		}
	}
}
