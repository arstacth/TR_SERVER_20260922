using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using AgentServer.Structuring;
using AgentServer.Structuring.Fishing;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserInfo : NetPacket
	{
		public GetUserInfo(Account User, string nickname, List<UserItemDyeing> AvatarItemDyeing, List<UserFishedItem> fishrecord, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_USER_INFO_ACK);
			int value = 0;
			int num = (int)ns.Position;
			ns.Write(value);
			ns.WriteAnsiFixed_intSize(nickname);
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_getUserInfo";
				mySqlCommand.Parameters.Add("nickname", MySqlDbType.VarString).Value = nickname;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
				if (mySqlDataReader.HasRows)
				{
					mySqlDataReader.Read();
					AvatarLock.UserInfo_AvatarLock_Load(Convert.ToInt32(mySqlDataReader["fdUserNum"]), out var AvatarLock_RealItem, out var AvatarLock_CostumeItem);
					bool flag = AvatarLock_RealItem.Count > 0;
					ns.Write(Convert.ToByte(mySqlDataReader["attribute"]));
					ns.Write(Convert.ToInt64(mySqlDataReader["fdExp"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["character"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["head"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["topBody"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["downBody"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["foot"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acHead"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acFace"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acHand"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acBack"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acNeck"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["pet"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["expansion"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acWrist"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acBooster"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["acTail"]));
					ns.Fill(16);
					for (int i = 0; i < 12; i++)
					{
						ns.Write(AvatarItemDyeing[i].DyeingPart);
						ns.Write(AvatarItemDyeing[i].Color1, 0, 3);
						ns.Write(AvatarItemDyeing[i].Color2, 0, 3);
						ns.Write(AvatarItemDyeing[i].Color3, 0, 3);
					}
					ns.Fill(10);
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_character"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_head"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_topBody"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_downBody"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_foot"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acHead"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acFace"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acHand"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acBack"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acNeck"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_pet"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_expansion"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acWrist"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acBooster"]));
					ns.Write(Convert.ToUInt16(mySqlDataReader["cos_acTail"]));
					ns.Fill(16);
					for (int j = 12; j < 24; j++)
					{
						ns.Write(AvatarItemDyeing[j].DyeingPart);
						ns.Write(AvatarItemDyeing[j].Color1, 0, 3);
						ns.Write(AvatarItemDyeing[j].Color2, 0, 3);
						ns.Write(AvatarItemDyeing[j].Color3, 0, 3);
					}
					ns.Fill(10);
					short num2 = Convert.ToInt16(mySqlDataReader["fdCostumeMode"]);
					ns.Write((byte)num2);
					long value2 = 0L;
					if (Convert.IsDBNull(mySqlDataReader["fdGuildName"]))
					{
						ns.WriteAnsiFixed_intSize(string.Empty);
						ns.WriteAnsiFixed_intSize("http://0");
					}
					else
					{
						value2 = Convert.ToInt64(mySqlDataReader["fdGuildNum"]);
						ns.WriteAnsiFixed_intSize(mySqlDataReader["fdGuildName"].ToString());
						ns.WriteAnsiFixed_intSize("http://0");
					}
					ns.Write(Convert.ToInt32(mySqlDataReader["couplenum"]));
					ns.Write(0);
					ns.WriteAnsiFixed_intSize(mySqlDataReader["matename"].ToString());
					ns.Write((mySqlDataReader["createtime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["createtime"])));
					ns.Write(0L);
					ns.Write((mySqlDataReader["ringChangedTime"].ToString() == "0") ? 1842465389770955L : Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["ringChangedTime"])));
					ns.Write(-1L);
					ns.Write(Convert.ToInt32(mySqlDataReader["coupleLevel"]));
					ns.Write(-1);
					ns.Write(0L);
					ns.Write(Convert.ToInt32(mySqlDataReader["accumulateExp"]));
					ns.Write(0);
					ns.Write((byte)0);
					ns.Write(Convert.ToInt32(mySqlDataReader["emblemCount"]));
					ns.Write(0L);
					ns.Write(value2);
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["lastLogoutTime"])));
					ns.Write(Convert.ToInt32(mySqlDataReader["playingTime"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdLikeable"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["fdGameOption"]));
					ns.Write(Utility.PackFarmUnique(Convert.ToInt32(mySqlDataReader["FarmUniqueNum"])));
					ns.Write(0L);
					ns.Write(0);
					ns.WriteAnsiFixed_intSize(mySqlDataReader["Farmname"].ToString());
					ns.WriteAnsiFixed_intSize(string.Empty);
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["FarmExpireDateTime"])));
					ns.Write(Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["FarmCreateDateTime"])));
					ns.Write(string.IsNullOrEmpty(mySqlDataReader.GetString("FarmPassword")));
					ns.Write((byte)0);
					ns.Write((byte)0);
					ns.Write(0);
					ns.Write(0);
					ns.Fill(3);
					ns.Write((byte)0);
					ns.Write(0L);
					ns.Write(Convert.ToInt64(mySqlDataReader["FarmExp"]));
					ns.Fill(6);
					int farmType = Convert.ToInt32(mySqlDataReader["type"]);
					long expireTs = Convert.IsDBNull(mySqlDataReader["expireTime"])
						? 0L
						: Utility.ConvertToTimestamp(Convert.ToDateTime(mySqlDataReader["expireTime"]));
					ns.Write(farmType);
					ns.Write(expireTs);
					int maxSave = Convert.ToInt32(mySqlDataReader["maxSavableCount"]);
					int maxRecv = Convert.ToInt32(mySqlDataReader["maxReceivableCount"]);
					if (maxSave < 0)
					{
						maxSave = 127;
					}
					if (maxSave > 255)
					{
						maxSave = 255;
					}
					if (maxRecv < 0)
					{
						maxRecv = 20;
					}
					if (maxRecv > 255)
					{
						maxRecv = 255;
					}
					ns.Write((byte)maxSave);
					ns.Write((byte)maxRecv);
					ns.Write(farmType);
					ns.Write(expireTs);
					ns.Write(Convert.ToInt32(mySqlDataReader["TopRank"]));
					ns.Write(6);
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal0"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal1"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal2"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal3"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal4"]));
					ns.Write(Convert.ToInt32(mySqlDataReader["Medal5"]));
					ns.Write(0);
					ns.Write(Convert.ToInt32(mySqlDataReader["TalesBookBG"]));
					ns.Write(fishrecord.Count);
					foreach (UserFishedItem item in fishrecord)
					{
						ns.Write(item.ItemNum);
						ns.Write(item.Size);
						ns.Write(item.Count);
					}
					ns.Fill(17);
					ns.Write(AvatarLock_RealItem.Count);
					foreach (int avatarItem in AvatarLock_RealItem)
					{
						ns.Write(avatarItem);
					}
					ns.Write(AvatarLock_CostumeItem.Count);
					foreach (int costumeItem in AvatarLock_CostumeItem)
					{
						ns.Write(costumeItem);
					}
					ns.Write((byte)(flag ? num2 : 0));
					ns.Write(flag);
				}
				else
				{
					value = 64;
				}
			}
			ns.Write(last);
			ns.Seek(num, SeekOrigin.Begin);
			ns.Write(value);
		}
	}
}
