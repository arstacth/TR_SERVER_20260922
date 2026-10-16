using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ITEM_ONOFF_ACK : NetPacket
	{
		public ITEM_ONOFF_ACK(int itemnum, int OnOffType, int Position, bool isOn, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_ONOFF_ACK);
			ns.Write(0);
			ns.Write(itemnum);
			ns.Write(OnOffType);
			ns.Write(Position);
			ns.Write(isOn);
			_ = last;
		}
	}
}
