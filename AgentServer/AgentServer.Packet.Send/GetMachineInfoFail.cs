using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMachineInfoFail : NetPacket
	{
		public GetMachineInfoFail(Account User, int MachineItemNum, int MachineID, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_INFO__VER2_ACK);
			ns.Write(MachineItemNum);
			ns.Write(MachineID);
			ns.Write(-1);
			ns.Write((byte)0);
			ns.Write(2);
			ns.Write(0);
			ns.Fill(20);
			_ = last;
		}
	}
}
