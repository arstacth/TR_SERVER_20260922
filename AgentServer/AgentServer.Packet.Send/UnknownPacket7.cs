using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket7 : NetPacket
	{
		public UnknownPacket7(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_GET_USER_MISSION_FINISH_COUNT_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
