using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetUserPoint : NetPacket
	{
		public GetUserPoint(int type, int totlapoint, int currentpoint, byte last)
		{
			ns.WriteOP(Opcodes.eServer_USERPOINT_VER2_ACK);
			ns.Write(type);
			ns.Write(totlapoint);
			ns.Write(currentpoint);
			_ = last;
		}
	}
}
