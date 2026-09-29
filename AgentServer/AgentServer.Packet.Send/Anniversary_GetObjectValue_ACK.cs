using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_GetObjectValue_ACK : NetPacket
	{
		public Anniversary_GetObjectValue_ACK(Dictionary<int, long> objectValueMap, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANNIVERSARY_GET_OBJECT_VALUE_ACK);
			ns.Write(0);
			ns.Write(objectValueMap.Count);
			foreach (KeyValuePair<int, long> item in objectValueMap)
			{
				ns.Write(item.Key);
				ns.Write(item.Value);
			}
			_ = last;
		}
	}
}
