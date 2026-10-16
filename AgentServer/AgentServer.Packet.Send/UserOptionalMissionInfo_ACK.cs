using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UserOptionalMissionInfo_ACK : NetPacket
	{
		public UserOptionalMissionInfo_ACK(Dictionary<int, int> UserOptionalMissionInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_OPTIONAL_MISSION_GET_USER_INFO_ACK);
			ns.Write(0);
			ns.Write(UserOptionalMissionInfo.Count);
			foreach (KeyValuePair<int, int> item in UserOptionalMissionInfo)
			{
				ns.Write(item.Key);
				ns.Write(item.Value);
			}
			_ = last;
		}
	}
}
