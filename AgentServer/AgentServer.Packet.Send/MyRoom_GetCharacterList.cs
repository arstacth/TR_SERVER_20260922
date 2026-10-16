using System;
using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using MySql.Data.MySqlClient;

namespace AgentServer.Packet.Send
{
	public sealed class MyRoom_GetCharacterList : NetPacket
	{
		public MyRoom_GetCharacterList(Account User, byte last)
		{
			List<ushort> list = new List<ushort>();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					@"SELECT DISTINCT IFNULL(NULLIF(t2.fdChar,0), d.fdCharacter) AS `character`
FROM tblAvatarUserCharacter t1
LEFT JOIN EssenAvatarItemCPKRef t2 ON t1.fdItemDescNum = t2.fdItemNum
LEFT JOIN tblAvatarItemDesc d ON t1.fdItemDescNum = d.fdItemNum
WHERE t1.fdUserNum=@u
  AND ((t2.fdPos=0 AND t2.fdChar<>0) OR (d.fdPosition=0 AND IFNULL(d.fdCharacter,0)<>0))
ORDER BY 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", User.UserNum);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					if (mySqlDataReader.IsDBNull(0))
					{
						continue;
					}
					list.Add(Convert.ToUInt16(mySqlDataReader["character"]));
				}
			}
			if (list.Count > 255)
			{
				list.RemoveRange(255, list.Count - 255);
			}
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_GET_MY_CHARACTER_LIST_ACK);
			ns.Write((byte)list.Count);
			foreach (ushort item in list)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
