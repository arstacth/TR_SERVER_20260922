using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMachineSelectItem : NetPacket
	{
		public GetMachineSelectItem(int MachineItemNum, int ResultItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_SELECT_ACK);
			ns.Write(MachineItemNum);
			ns.Write(ResultItemNum);
			_ = last;
		}
	}
}
