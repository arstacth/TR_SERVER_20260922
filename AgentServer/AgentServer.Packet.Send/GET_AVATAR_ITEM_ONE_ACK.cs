using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_ITEM_ONE_ACK : NetPacket
	{
		public GET_AVATAR_ITEM_ONE_ACK(NetItemInfo item, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ITEM_ONE_ACK);
			ns.Write((ushort)(int)item.m_character);
			ns.Write((ushort)(int)item.m_position);
			ns.Write((ushort)(int)item.m_kind);
			ns.Write(item.m_iItemDescNum);
			ns.Write(item.m_expireTime);
			ns.Write(item.m_tGot);
			ns.Write(item.m_count);
			ns.Write(item.m_exp);
			ns.Write(item.m_bHasExpireTime);
			ns.Write(item.m_bUsing);
			_ = last;
		}
	}
}
