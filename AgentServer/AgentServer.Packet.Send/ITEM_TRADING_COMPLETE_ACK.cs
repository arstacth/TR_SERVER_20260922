using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ITEM_TRADING_COMPLETE_ACK : NetPacket
	{
		public ITEM_TRADING_COMPLETE_ACK(eTRADE_ACTION result, int source_item, int target_item, int ticket, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_TRADING_COMPLETE);
			ns.Write((byte)result);
			ns.Write(source_item);
			ns.Write(target_item);
			ns.Write(ticket);
			_ = last;
		}
	}
}
