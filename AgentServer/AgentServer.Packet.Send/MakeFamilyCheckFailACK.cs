using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MakeFamilyCheckFailACK : NetPacket
	{
		public MakeFamilyCheckFailACK(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FAMILY_CHECK_PROPOSE_CONDITION_FAILED_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
