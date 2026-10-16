using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMachineSelectItemFail : NetPacket
	{
		public GetMachineSelectItemFail(int MachineItemNum, byte err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_SELECT_FAILED_ACK);
			ns.Write(MachineItemNum);
			ns.Write(err);
			_ = last;
		}
	}
}
