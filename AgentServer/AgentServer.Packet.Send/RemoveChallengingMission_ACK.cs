using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RemoveChallengingMission_ACK : NetPacket
	{
		public RemoveChallengingMission_ACK(int kind, int MissionNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_REMOVE_ACK);
			ns.Write(kind);
			ns.Write(MissionNum);
			_ = last;
		}
	}
}
