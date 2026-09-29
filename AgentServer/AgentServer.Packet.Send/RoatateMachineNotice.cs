using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class RoatateMachineNotice : NetPacket
	{
		public RoatateMachineNotice(string MachineItemNum)
		{
			ns.WriteOP(Opcodes.eServer_NOTICE_MSG_ACK);
			ns.Write(3);
			ns.Write(3);
			ns.WriteAnsiFixed_intSize(MachineItemNum);
			ns.Write((byte)1);
		}
	}
}
