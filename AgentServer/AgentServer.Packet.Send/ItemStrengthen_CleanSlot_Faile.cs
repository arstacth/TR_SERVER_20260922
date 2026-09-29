using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ItemStrengthen_CleanSlot_Failed_ACK : NetPacket
	{
		public ItemStrengthen_CleanSlot_Failed_ACK(int err, int itemNum, short slotNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_PURIFY_ITEM_FAILED_ACK);
			ns.Write(err);
			ns.Write(itemNum);
			ns.Write(slotNum);
			_ = last;
		}
	}
}
