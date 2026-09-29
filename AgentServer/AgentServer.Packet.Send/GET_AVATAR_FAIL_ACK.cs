using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_FAIL_ACK : NetPacket
	{
		public GET_AVATAR_FAIL_ACK(eServerResult result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ACK);
			ns.Write((int)result);
		}
	}
}
