using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_UserGetItemInfo_ACK : NetPacket
	{
		public TowerEvent_UserGetItemInfo_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_GetBoxList_ACK);
			ns.Write(3);
			ns.Write(1);
			ns.Write(0);
			ns.Write(2);
			ns.Write(0);
			ns.Write(3);
			ns.Write(0);
			_ = last;
		}
	}
}
