using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.Room;

namespace RoomServer.Packet.Send
{
	public sealed class SetObjectInfo_Ack : NetPacket
	{
		public SetObjectInfo_Ack(NormalRoom room, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_SET_OBJECT_INFO_ACK);
			ns.Write(room.AnubisObjectBoss.Count);
			foreach (KeyValuePair<int, ObjectBoss> item in room.AnubisObjectBoss)
			{
				ns.Write(9269960);
				ns.Write(item.Key);
				ns.Write(item.Value.HP);
				ns.Write(1L);
			}
			_ = last;
		}
	}
}
