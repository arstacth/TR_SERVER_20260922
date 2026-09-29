using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CapsuleMachineNotice : NetPacket
	{
		public CapsuleMachineNotice(byte level, int MachineItemNum, int ItemNum, string NickName)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_NOTIFY);
			ns.Write(level);
			ns.Write(MachineItemNum);
			ns.Write(ItemNum);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(NickName);
			ns.Write((byte)1);
		}
	}
}
