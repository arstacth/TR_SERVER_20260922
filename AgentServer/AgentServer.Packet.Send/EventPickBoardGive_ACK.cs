using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardGive_ACK : NetPacket
	{
		public EventPickBoardGive_ACK(ExchangeItemInfo exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_GIVE_ACK);
			ns.Write(0);
			ns.Write(value: false);
			ns.Write(exinfo.type);
			ns.Write(exinfo.id);
			ns.Write(exinfo.count);
			ns.Write(int.MaxValue);
			_ = last;
		}
	}
}
