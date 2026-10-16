using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_ACTIVE_FUNC_ITEM_LIST_ACK : NetPacket
	{
		public GET_ACTIVE_FUNC_ITEM_LIST_ACK(Dictionary<int, NetItemInfo> AvatarItems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_ACK);
			ns.Write(0);
			ns.Write((int)Opcodes.eServer_GET_ACTIVE_FUNC_ITEM_LIST_REQ);
			// Thai + client: int count (not short). Short left header at 12,
			// client at offset 14 tried item(36) on size 48 → Overpop 14 48 36.
			ns.Write(AvatarItems.Count);
			GET_ACTIVE_FUNC_ITEM_ACK.WriteItems(ns, AvatarItems.Values.ToList());
			// LIST path has no extra-list short / last (Thai Write(last) → Remain 1).
			_ = last;
		}
	}
}
