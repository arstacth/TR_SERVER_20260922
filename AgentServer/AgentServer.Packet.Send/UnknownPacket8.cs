using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownPacket8 : NetPacket
	{
		public UnknownPacket8(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ONEDAY_MISSION_GET_EVENT_MISSION_STATUS_FAILED_ACK);
			ns.Write((short)8);
			_ = last;
		}
	}
}
