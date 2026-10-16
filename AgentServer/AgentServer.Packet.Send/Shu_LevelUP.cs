using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_LevelUP : NetPacket
	{
		public Shu_LevelUP(long shucharitemid, int beforelv, int afterlv, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.LEVEL_UP_ACK);
			ns.Write(0);
			ns.Write(beforelv);
			ns.Write(afterlv);
			ns.Write(shucharitemid);
			_ = last;
		}
	}
}
