using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class BounsItemMake_Ack : NetPacket
	{
		public BounsItemMake_Ack(int objectid, Dictionary<int, int> bonus, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_GET_OBJECT_REWARD_ACK);
			ns.Write(objectid);
			ns.Write(bonus.Count);
			foreach (KeyValuePair<int, int> bonu in bonus)
			{
				ns.Write(bonu.Key);
				ns.Write(0L);
				ns.Write(bonu.Value);
			}
			_ = last;
		}
	}
}
