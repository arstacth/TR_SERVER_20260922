using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ExploreStopOK : NetPacket
	{
		public Shu_ExploreStopOK(byte zoneid, long shuitemid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.EXPLORE_STOP_ACK);
			ns.Write(0);
			ns.Write(zoneid);
			ns.Write(shuitemid);
			_ = last;
		}
	}
}
