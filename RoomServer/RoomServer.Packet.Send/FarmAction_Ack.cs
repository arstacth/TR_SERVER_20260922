using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FarmAction_Ack : NetPacket
	{
		public FarmAction_Ack(byte pos, long FarmItemID, long unk1, long unk2, bool isOn, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_MODIFY_OBJECT_LOCK_INFO_ACK);
			ns.Write(0);
			ns.Write(FarmItemID);
			ns.Write(unk1);
			ns.Write(unk2);
			ns.Write(pos);
			ns.Write(isOn);
			_ = last;
		}
	}
}
