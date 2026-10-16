using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_ItemOnOff_ACK : NetPacket
	{
		public Myroom_ItemOnOff_ACK(int itemnum, int OnOffType, int Position, bool isOn, byte last)
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
