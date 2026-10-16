using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FreePassExpireNotify : NetPacket
	{
		public FreePassExpireNotify()
		{
			ns.WriteOP(Opcodes.eServer_FREE_PASS_EXPIRE_NOTIFY);
		}
	}
}
