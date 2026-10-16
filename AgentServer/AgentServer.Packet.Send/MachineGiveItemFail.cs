using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MachineGiveItemFail : NetPacket
	{
		public MachineGiveItemFail(byte err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_GIVE_FAILED_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
