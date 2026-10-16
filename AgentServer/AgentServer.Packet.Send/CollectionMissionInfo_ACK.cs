using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CollectionMissionInfo_ACK : NetPacket
	{
		public CollectionMissionInfo_ACK(int type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_COLLECTION_MISSION_GET_USER_MISSION_ACK);
			ns.Write(type);
			if (MissionHolder.CollectionMissionInfo.TryGetValue(type, out var value))
			{
				ns.Write(value.Count);
				foreach (int item in value)
				{
					ns.Write(type);
					ns.Write(item);
					ns.Write(0);
				}
			}
			else
			{
				ns.Write(0);
			}
			_ = last;
		}
	}
}
