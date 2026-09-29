using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class ItemTransform_ACK : NetPacket
	{
		public ItemTransform_ACK(ItemTransformInfo info, int itemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TRANSFORM_ITEM_ACK);
			ns.Write(info.character);
			ns.Write(info.position);
			ns.Write(info.transKind);
			ns.Write(itemNum);
			_ = last;
		}

		public ItemTransform_ACK(CItemTransformInfo info, int itemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TRANSFORM_ITEM_ACK);
			ns.Write(info.m_iCharacter);
			ns.Write(info.m_iPosition);
			ns.Write(info.m_iTransKind);
			ns.Write(itemNum);
			_ = last;
		}
	}
}
