using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_GetBox_ACK : NetPacket
	{
		public TowerEvent_GetBox_ACK(int BoxID, int BoxType, int ResultItem, int Result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_GETITEM_ACK);
			ns.Write(BoxID);
			ns.Write(BoxType);
			ns.Write(ResultItem);
			ns.Write(Result);
			_ = last;
		}
	}
}
