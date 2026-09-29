using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetPartyJoinRequestList_Ack : NetPacket
	{
		public GetPartyJoinRequestList_Ack(Party party, int page, int getnum, byte last)
		{
			IEnumerable<KeyValuePair<string, int>> enumerable = party.JoinRequestList.Skip((page - 1) * getnum).Take(getnum);
			int num = Convert.ToInt32(Math.Ceiling((double)enumerable.Count() / Convert.ToDouble(getnum)));
			if (num == 0)
			{
				page = 0;
			}
			ns.WriteOP(Opcodes.eServer_PARTY_SYSTEM_PROTOCOL);
			ns.Write(1);
			ns.Write(26L);
			ns.Write(num);
			ns.Write(page);
			ns.Write(enumerable.Count());
			foreach (KeyValuePair<string, int> item in enumerable)
			{
				ns.WriteAnsiFixed_intSize(item.Key);
				ns.Write(5);
				ns.Write(item.Value);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
