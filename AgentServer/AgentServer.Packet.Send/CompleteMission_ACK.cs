using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CompleteMission_ACK : NetPacket
	{
		public CompleteMission_ACK(int kind, int missionNum, short unk, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_COMPLETE_CHECK_ACK);
			ns.Write(kind);
			ns.Write(missionNum);
			ns.Write(unk);
			_ = last;
		}
	}
}
