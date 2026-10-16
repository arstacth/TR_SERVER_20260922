using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_ACTIVE_FUNC_ITEM_ACK : NetPacket
	{
		public GET_ACTIVE_FUNC_ITEM_ACK(byte remainpage, short startindex, List<NetItemInfo> AvatarItems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_ACK);
			ns.Write(0);
			ns.Write((int)Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_REQ);
			// Mid remainpage>0: short → Remain=2 (poisons 1481); no short → Overpop need=2
			// (also desyncs). Use last-page shape on every page (remain=0 + short); keep
			// startindex so the client can merge chunks.
			_ = remainpage;
			ns.Write((byte)0);
			ns.Write(startindex);
			ns.Write((short)AvatarItems.Count);
			WriteItems(ns, AvatarItems);
			ns.Write((short)0);
			_ = last;
		}

		internal static void WriteItems(PacketWriter ns, List<NetItemInfo> AvatarItems)
		{
			foreach (NetItemInfo AvatarItem in AvatarItems)
			{
				ns.Write((ushort)(int)AvatarItem.m_character);
				ns.Write((ushort)(int)AvatarItem.m_position);
				ns.Write((ushort)(int)AvatarItem.m_kind);
				ns.Write(AvatarItem.m_iItemDescNum);
				ns.Write(AvatarItem.m_expireTime);
				ns.Write(AvatarItem.m_tGot);
				ns.Write(AvatarItem.m_count);
				ns.Write(AvatarItem.m_exp);
				ns.Write(AvatarItem.m_bHasExpireTime);
				ns.Write(AvatarItem.m_bUsing);
			}
		}
	}
}
