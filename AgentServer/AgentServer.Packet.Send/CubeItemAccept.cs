using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CubeItemAccept : NetPacket
	{
		public CubeItemAccept(eCUBE_ACCEPT_RESULT result, int cubeitemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEMCUBE_ACCEPT_ACK);
			ns.Write((int)result);
			ns.Write(cubeitemnum);
			_ = last;
		}
	}
}
