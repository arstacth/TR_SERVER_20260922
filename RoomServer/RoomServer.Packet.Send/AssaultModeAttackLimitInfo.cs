using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AssaultModeAttackLimitInfo : NetPacket
	{
		public AssaultModeAttackLimitInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ROOMKIND_ENTRY_CONDITION_ACK);
			ns.Write(MapHolder.AssaultModeLimitInfos.Count);
			foreach (KeyValuePair<int, int> assaultModeLimitInfo in MapHolder.AssaultModeLimitInfos)
			{
				ns.Write(assaultModeLimitInfo.Key);
				ns.Write(0L);
				ns.Write(assaultModeLimitInfo.Value);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
