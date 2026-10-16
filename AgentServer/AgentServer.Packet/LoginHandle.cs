using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AgentServer;
using AgentServer.Cryptography;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using NetMsg.LBS;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class LoginHandle
	{
		public static void Handle_LoginCheck(ClientConnection Client, PacketReader reader)
		{
			try
			{
				byte[] raw = reader.Buffer;
				if (Conf.ProtocolDebug)
				{
					Log.Information("Login raw ({0}): {1}", raw.Length, Utility.ByteArrayToString(raw));
				}
				int fixedLength = reader.ReadLEInt16();
				string text = reader.ReadBig5StringSafe(fixedLength);
				int length = reader.ReadLEInt16();
				byte[] pwBytes = reader.ReadByteArray(Math.Max(0, length));
				List<string> candidates = new List<string>();
				AddPasswordCandidate(candidates, TryDecodeThaiAesPassword(pwBytes));
				AddDecodedPasswordCandidates(candidates, pwBytes);
				// Thai 1.3.x prefix is ASCII "016" then AES ciphertext + 32-byte salt.
				if (pwBytes.Length >= 51 && pwBytes[0] == 0x30 && pwBytes[1] == 0x31 && pwBytes[2] == 0x36)
				{
					byte[] body = new byte[pwBytes.Length - 3];
					Buffer.BlockCopy(pwBytes, 3, body, 0, body.Length);
					AddPasswordCandidate(candidates, TryDecodeThaiAesPassword(body));
					AddDecodedPasswordCandidates(candidates, body);
				}
				try
				{
					if (raw != null && raw.Length > 2)
					{
						byte[] fbdata = new byte[raw.Length - 2];
						Buffer.BlockCopy(raw, 2, fbdata, 0, fbdata.Length);
						FB.LoginInputInfo.LoginInputInfo loginInputInfo = FB.LoginInputInfo.LoginInputInfo.GetRootAsLoginInputInfo(new FlatBuffers.ByteBuffer(fbdata));
						if (!string.IsNullOrEmpty(loginInputInfo.Userid))
						{
							text = loginInputInfo.Userid.Trim('\0');
						}
						byte[] passwordArray = loginInputInfo.GetPasswordArray();
						if (passwordArray != null && passwordArray.Length != 0)
						{
							AddPasswordCandidate(candidates, NormalizePassword(pwdecode(passwordArray)));
							AddPasswordCandidate(candidates, NormalizePassword(Encoding.Unicode.GetString(passwordArray)));
							AddPasswordCandidate(candidates, Encoding.ASCII.GetString(passwordArray).Trim('\0'));
						}
					}
				}
				catch (Exception ex2)
				{
					if (Conf.ProtocolDebug)
					{
						Log.Information("Login FB parse skip: {0}", ex2.Message);
					}
				}
				bool flag = false;
				if (!new Regex("='\"").IsMatch(text) && text.Length != 0)
				{
					foreach (string candidate in candidates)
					{
						int accountResult = CheckUserAccountResult(text, candidate);
						if (accountResult == 1)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						int exists = UserAccountExists(text) ? 1 : 0;
						Log.Warning("Login auth failed user={0} accountExists={1} candidates={2} ip={3} decoded={4}",
							text, exists, candidates.Count, Client.IP, PreviewCandidates(candidates));
					}
				}
				if (Conf.ProtocolDebug)
				{
					Log.Information("LoginCheck user:{0} pwEnc:{1} candidates:{2} remain:{3} ok:{4}", text, length, candidates.Count, reader.Remaining, flag);
				}
				if (flag)
				{
					byte[] array = new byte[16];
					new RNGCryptoServiceProvider().GetBytes(array);
					byte[] xorKey = Encrypt.EncryptKey(array);
					long num = Utility.CurrentTimeMilliseconds();
					Account account2 = (Client.CurrentAccount = new Account
					{
						UserID = text,
						Connection = Client,
						LastIp = Client.IP,
						Port = (short)((IPEndPoint)Client.EP).Port,
						EncryptKey = array,
						XorKey = xorKey,
						Session = Client.session,
						LoginDateTime = DateTime.Now,
						bLogin = false,
						LoginAuthPassed = true,
						LastCheckTime = num,
						LastPingTime = num
					});
					ClientConnection.CurrentAccounts.TryAdd(Client.session, Client.CurrentAccount);
				}
				else
				{
					if (Client.CurrentAccount != null)
					{
						Client.CurrentAccount.LoginAuthPassed = false;
						Client.CurrentAccount.UserID = text ?? string.Empty;
					}
					Log.Warning("{0} Login Fail! ip:{1}", text, Client.IP);
				}
				Client.SendAsync(new LOGIN_AUTH_ACK_THAI(text, flag));
				ClientConnection.DDOS_IP.TryRemove(Client.IP, out var _);
			}
			catch (Exception ex)
			{
				Log.Error("LoginCheck Error:{0}", ex.ToString());
			}
		}

		private static void AddDecodedPasswordCandidates(List<string> candidates, byte[] pwBytes)
		{
			if (pwBytes == null || pwBytes.Length == 0)
			{
				return;
			}
			AddPasswordCandidate(candidates, SafePwdecode(pwBytes));
			if (pwBytes.Length >= 24)
			{
				byte[] first24 = new byte[24];
				Buffer.BlockCopy(pwBytes, 0, first24, 0, 24);
				AddPasswordCandidate(candidates, SafePwdecode(first24));
			}
			if (pwBytes.Length >= 48)
			{
				byte[] first48 = new byte[48];
				Buffer.BlockCopy(pwBytes, 0, first48, 0, 48);
				AddPasswordCandidate(candidates, SafePwdecode(first48));
			}
			if (pwBytes.Length >= 2 && pwBytes.Length % 2 == 0)
			{
				AddPasswordCandidate(candidates, NormalizePassword(Encoding.Unicode.GetString(pwBytes)));
			}
		}

		private static string TryDecodeThaiAesPassword(byte[] pwBytes)
		{
			if (pwBytes == null || pwBytes.Length < 48)
			{
				return null;
			}
			int offset = 0;
			if (pwBytes.Length >= 51 && pwBytes[0] == 0x30 && pwBytes[1] == 0x31 && pwBytes[2] == 0x36)
			{
				offset = 3;
			}
			int bodyLen = pwBytes.Length - offset;
			if (bodyLen > 32 && (bodyLen - 32) % 16 != 0 && pwBytes[pwBytes.Length - 1] == 0)
			{
				bodyLen--;
			}
			if (bodyLen < 48 || (bodyLen - 32) % 16 != 0)
			{
				return null;
			}
			int cipherLen = bodyLen - 32;
			byte[] cipher = new byte[cipherLen];
			byte[] salt = new byte[32];
			Buffer.BlockCopy(pwBytes, offset, cipher, 0, cipherLen);
			Buffer.BlockCopy(pwBytes, offset + cipherLen, salt, 0, 32);
			try
			{
				byte[] key;
				using (Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(Encoding.ASCII.GetBytes("Talesrunner_TH"), salt, 10000, HashAlgorithmName.SHA256))
				{
					key = kdf.GetBytes(32);
				}
				using (Aes aes = Aes.Create())
				{
					aes.Mode = CipherMode.ECB;
					aes.Padding = PaddingMode.PKCS7;
					aes.KeySize = 256;
					aes.BlockSize = 128;
					aes.Key = key;
					using (ICryptoTransform decryptor = aes.CreateDecryptor())
					{
						byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
						return NormalizePassword(Encoding.ASCII.GetString(plain));
					}
				}
			}
			catch (Exception ex)
			{
				if (Conf.ProtocolDebug)
				{
					Log.Information("Thai AES password decode skip: {0}", ex.Message);
				}
				return null;
			}
		}

		private static string SafePwdecode(byte[] pw)
		{
			try
			{
				return NormalizePassword(pwdecode(pw));
			}
			catch (Exception ex)
			{
				Log.Warning("pwdecode skip len={0}: {1}", pw != null ? pw.Length : 0, ex.Message);
				return null;
			}
		}

		private static string PreviewCandidates(List<string> candidates)
		{
			if (candidates == null || candidates.Count == 0)
			{
				return "-";
			}
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < candidates.Count && i < 4; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append('|');
				}
				string text = candidates[i] ?? string.Empty;
				int printable = 0;
				for (int j = 0; j < text.Length && j < 16; j++)
				{
					char c = text[j];
					if (c >= 32 && c < 127)
					{
						stringBuilder.Append(c);
						printable++;
					}
					else
					{
						stringBuilder.Append('?');
					}
				}
				stringBuilder.Append('#').Append(text.Length).Append('/').Append(printable);
			}
			return stringBuilder.ToString();
		}

		private static void AddPasswordCandidate(List<string> candidates, string password)
		{
			if (!string.IsNullOrEmpty(password) && !candidates.Contains(password))
			{
				candidates.Add(password);
			}
		}

		public static void Handle_GetClientKey(ClientConnection Client, PacketReader reader)
		{
			try
			{
				if (Client.CurrentAccount == null || !Client.CurrentAccount.LoginAuthPassed)
				{
					Log.Warning("GetClientKey rejected: login auth not passed ip:{0}", Client.IP);
					return;
				}
				Client.CurrentAccount.GotClientKey = true;
				byte[] collection = reader.ReadByteArray(255);
				List<byte> list = new List<byte>();
				list.AddRange(collection);
				list.Add(0);
				byte[] key = Encrypt.TFUNC_1_W(list.ToArray(), Client.CurrentAccount.EncryptKey, Conf.ServerIP);
				Client.SendAsync(new LoginGenKey(key));
			}
			catch (Exception ex)
			{
				Log.Error("GetClientKey Error:{0}", ex.Message);
			}
		}

		public static void Handle_LoginSuccess(ClientConnection Client, PacketReader reader, byte last)
		{
			reader.ReadByte();
			short num = reader.ReadLEInt16();
			int fixedLength = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength);
			int fixedLength2 = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength2);
			int fixedLength3 = reader.ReadLEInt16();
			reader.ReadBig5StringSafe(fixedLength3);
			Account currentAccount = Client.CurrentAccount;
			if (currentAccount == null || !currentAccount.LoginAuthPassed)
			{
				Log.Warning("LOGIN_REQ rejected: login auth not passed ip:{0}", Client.IP);
				Client.SendAsync(new LoginError(13, last, 0));
				return;
			}
			if (Conf.ProtocolDebug)
			{
				Log.Information("Client protocol version userid:{0} ver:{1}", currentAccount.UserID, num);
			}
			if (num < 1)
			{
				currentAccount.isBlocked = true;
				Log.Error("Incorrect Protocol version userid : {0}, server(4,{1}max), client(4,{2}max)", currentAccount.UserID, (short)Opcodes.eUDPProtocol_MAX, num);
				Client.SendAsync(new LoginError(5, last, 0));
				return;
			}
			if (Conf.HashCheck && !ServerSettingHolder.HashList.Contains(text))
			{
				currentAccount.isBlocked = true;
				Log.Error("InCorrect hash. {0} : {1}", currentAccount.UserID, text);
				Client.SendAsync(new LoginError(8, last, 0));
				return;
			}
			ServerStatus.LBServerActor.Tell(new AccountCheckRequest
			{
				Session = currentAccount.Session,
				UID = currentAccount.UserID,
				ServerID = ServerStatus.MyAgentID
			});
			if (ClientConnection.CurrentAccounts.Count((KeyValuePair<int, Account> c) => c.Value.isLogin) > Conf.MaxUserCount)
			{
				currentAccount.isBlocked = true;
				Log.Error("User [{0}] can't login because server full!", currentAccount.UserID);
				Client.SendAsync(new LoginError(7, last, 0));
			}
			else
			{
				LoginTrafficManager.LoginProcess(Client, last);
			}
		}

		public static void Handle_NOTIFY_MY_UDP(ClientConnection Client, PacketReader reader, byte last)
		{
			int n = reader.Remaining;
			byte[] info = null;
			if (n > 0)
			{
				info = reader.ReadByteArray(n > 48 ? 48 : n);
				byte[] padded = new byte[48];
				Buffer.BlockCopy(info, 0, padded, 0, info.Length);
				Client.CurrentAccount.UDPInfo = padded;
			}
			Client.SendAsync(new Login_NOTIFY_MY_UDP(last));
			if (Conf.ProtocolDebug)
			{
				Log.Information("NOTIFY_MY_UDP from {0} bytes {1} {2}", Client.CurrentAccount.UserID, n,
					info != null ? BitConverter.ToString(info, 0, Math.Min(info.Length, 32)) : "");
			}
		}

		public static void Handle_GetNickName(ClientConnection Client, PacketReader reader, byte last)
		{
			Client.SendAsync(new LoginGetNickName_0X1C(Client.CurrentAccount, last));
		}

		public static void Handle_GetUserCash(ClientConnection Client, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			if (currentAccount.CashNeedUpdateFromDB)
			{
				getUserCash(currentAccount);
			}
			Client.SendAsync(new LoginGetUserCash(currentAccount, last));
		}

		public static void Handle_FF7F01(ClientConnection Client, byte last)
		{
			Client.SendAsync(new Login_FF7F01_0x180(last));
		}

		public static void Handle_ReportSystemUserInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new ReportSystemUserInfoAck(last));
		}

		public static void Handle_ReportSystemUserReportInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new ReportSystemUserReportInfoAck(last));
		}

		public static void Handle_PieroPostboxUserInfo(ClientConnection Client, byte last)
		{
			Client.SendAsync(new PieroPostboxUserInfoAck(last));
		}

		public static void Handle_82(ClientConnection Client, byte last)
		{
			Client.SendAsync(new Login_82_0x83(Client.CurrentAccount, last));
		}

		public static void Handle_GetCommunityAgentServer(ClientConnection Client, byte last)
		{
			Client.SendAsync(new GetCommunityAgentServer(last));
		}

		public static void Handle_GetExtraAbilities(ClientConnection Client, byte last)
		{
			getExtraAbilities(Client.CurrentAccount, last);
		}

		private static bool checkUserAccount(string userid, string password)
		{
			return CheckUserAccountResult(userid, password) == 1;
		}

		private static bool UserAccountExists(string userid)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("SELECT fdUserID FROM userinfofrompublisher WHERE fdUserID=@userid LIMIT 1", CommandType.Text);
				mySqlCommandHelper.AddParamVarString("@userid", userid);
				mySqlCommandHelper.ExecuteSingle();
				return mySqlCommandHelper.HasResult();
			}
			catch (Exception ex)
			{
				Log.Error("UserAccountExists Error: {0}", ex.Message);
				return false;
			}
		}

		private static int CheckUserAccountResult(string userid, string password)
		{
			if (string.IsNullOrEmpty(userid) || password == null)
			{
				return 0;
			}
			int num = TryCheckUserAccountCode(userid, password);
			if (num == 1)
			{
				return 1;
			}
			string md5Ascii = CreateMD5(password);
			num = TryCheckUserAccountCode(userid, md5Ascii);
			if (num == 1)
			{
				return 1;
			}
			string md5Utf = CreateMD5Utf8(password);
			if (md5Utf != md5Ascii)
			{
				num = TryCheckUserAccountCode(userid, md5Utf);
				if (num == 1)
				{
					return 1;
				}
			}
			return num;
		}

		private static int TryCheckUserAccountCode(string userid, string password)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_checkUserAccount");
				mySqlCommandHelper.AddParamVarString("userid", userid);
				mySqlCommandHelper.AddParamVarString("password", password);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					string text = mySqlCommandHelper.GetString("result");
					if (int.TryParse(text, out var result))
					{
						return result;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_checkUserAccount Error: {0}", ex.Message);
			}
			return 0;
		}

		private static void getUserCash(Account User)
		{
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getUserCash");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					User.Cash = mySqlCommandHelper.GetInt("cash");
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_getUserCash Error: {0}", ex.Message);
			}
			User.CashNeedUpdateFromDB = false;
		}

		public static void getExtraAbilities(Account User, byte last)
		{
			Dictionary<int, ExtraAbilityInfo> dictionary = new Dictionary<int, ExtraAbilityInfo>();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_getExtraAbilities");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					while (mySqlCommandHelper.HasResult())
					{
						short key = (short)mySqlCommandHelper.GetInt("attrType");
						float @float = mySqlCommandHelper.GetFloat("attrValue");
						int @int = mySqlCommandHelper.GetInt("itemnum");
						int iLimitTime = mySqlCommandHelper.GetInt("limit") * 1000;
						long dateTime = mySqlCommandHelper.GetDateTime("gottime", 0L);
						if (!dictionary.TryGetValue(@int, out var value))
						{
							value = new ExtraAbilityInfo();
						}
						value.iItemDescNum = @int;
						value.iLimitTime = iLimitTime;
						value.tGotTime = dateTime;
						value.mapAttributes[key] = @float;
						if (!dictionary.ContainsKey(@int))
						{
							dictionary.Add(@int, value);
						}
					}
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_getExtraAbilities Error: {0}", ex.Message);
			}
			if (flag)
			{
				User.Connection.SendAsync(new GetExtraAbilities_ACK(dictionary, last));
			}
		}

		private static string NormalizePassword(string password)
		{
			if (string.IsNullOrEmpty(password))
			{
				return password;
			}
			if (password.IndexOf('\0') >= 0)
			{
				password = password.Replace("\0", string.Empty);
			}
			return password;
		}

		private static string pwdecode(byte[] pw)
		{
			int num = pw.Length / 4;
			int num2 = 0;
			string text = "";
			for (int i = 0; i < num; i++)
			{
				int num3 = ((pw[i * 4] >= pw[1 + i * 4]) ? (256 + pw[1 + i * 4] - pw[i * 4]) : (pw[1 + i * 4] - pw[i * 4]));
				int num4;
				num2 = ((pw[2 + i * 4] != byte.MaxValue) ? (num4 = pw[3 + i * 4] - pw[2 + i * 4] << 8) : (num4 = 256 - (pw[2 + i * 4] - pw[3 + i * 4]) << 8));
				num4 = num4 >> 11 << 11;
				num2 -= num4;
				int value = num3 + num2 >> 4;
				text += (char)(value & 0xFF);
			}
			return text;
		}

		private static string CreateMD5(string input)
		{
			return CreateMD5(input, Encoding.ASCII);
		}

		private static string CreateMD5Utf8(string input)
		{
			return CreateMD5(input, Encoding.UTF8);
		}

		private static string CreateMD5(string input, Encoding encoding)
		{
			using MD5 mD = MD5.Create();
			byte[] bytes = encoding.GetBytes(input);
			byte[] array = mD.ComputeHash(bytes);
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(array[i].ToString("x2"));
			}
			return stringBuilder.ToString();
		}
	}
}
