using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AddChallengingMission_ACK : NetPacket
	{
		public AddChallengingMission_ACK(int kind, int MissionNum, long challengeExpireTime, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_ADD_ACK);
			ns.Write(kind);
			ns.Write(MissionNum);
			ns.Write(challengeExpireTime);
			_ = last;
		}
	}
}
