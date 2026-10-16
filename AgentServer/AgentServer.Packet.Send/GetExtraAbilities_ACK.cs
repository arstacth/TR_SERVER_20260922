using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GetExtraAbilities_ACK : NetPacket
	{
		public GetExtraAbilities_ACK(Dictionary<int, ExtraAbilityInfo> m_ExtraAbilities, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EXTRA_ABILITY_LIST_ASK);
			ns.Write(0);
			ns.Write(m_ExtraAbilities.Count);
			foreach (ExtraAbilityInfo value2 in m_ExtraAbilities.Values)
			{
				ns.Write(value2.iItemDescNum);
				ns.Write(value2.iLimitTime);
				ns.Write(value2.mapAttributes.Count);
				foreach (KeyValuePair<short, float> mapAttribute in value2.mapAttributes)
				{
					short key = mapAttribute.Key;
					float value = mapAttribute.Value;
					ns.Write((int)key);
					ns.Write(value);
				}
			}
			_ = last;
		}
	}
}
