using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class StartChallengeOK : NetPacket
	{
		public StartChallengeOK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_CHALLENGE_MAP_START_ACK);
			ns.Write(0);
			// Live trgame Overpop 6 7 4: packed pops a second int after result.
			ns.Write(0);
			_ = last;
		}
	}
}
