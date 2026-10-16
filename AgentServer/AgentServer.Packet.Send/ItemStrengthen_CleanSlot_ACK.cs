using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ItemStrengthen_CleanSlot_ACK : NetPacket
	{
		public ItemStrengthen_CleanSlot_ACK(int itemNum, short slotNum, int maxSlotCount, long TR, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STRENGTHEN_PURIFY_ITEM_OK_ACK);
			ns.Write(itemNum);
			ns.Write(slotNum);
			ns.Write(maxSlotCount);
			ns.Write(0);
			ns.Write(TR);
			_ = last;
		}
	}
}
