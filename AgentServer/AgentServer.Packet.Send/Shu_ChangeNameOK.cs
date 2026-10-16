using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ChangeNameOK : NetPacket
	{
		public Shu_ChangeNameOK(long shuitemid, string name, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.CHANGE_NAME_REQ);
			ns.Write(0);
			ns.Write(shuitemid);
			ns.WriteAnsiFixed_intSize(name);
			_ = last;
		}
	}
}
