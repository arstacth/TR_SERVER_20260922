using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Park;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMachineInfoResetting : NetPacket
	{
		public GetMachineInfoResetting(Account User, int MachineNum, CapsuleMachineData MachineInfo, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_INFO__VER2_ACK);
			ns.Write(MachineInfo.RealMachineNum);
			int wireKind = MachineInfo.RealMachineNumKind == 1011 ? 1003 : MachineInfo.RealMachineNumKind;
			ns.Write(wireKind);
			if (MachineInfo.isRotate)
			{
				ns.Write(MachineNum);
			}
			else
			{
				ns.Write(-1);
			}
			ns.Write((byte)0);
			ns.Write(9);
			ns.Write(0);
			GetMachineInfo.WriteUserMachineTrailer(ns, User, MachineInfo.RealMachineNumKind);
		}
	}
}
