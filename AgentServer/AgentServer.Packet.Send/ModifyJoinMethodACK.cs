using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyJoinMethodACK : NetPacket
	{
		public ModifyJoinMethodACK(short joinMethod, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MODIFY_JOIN_METHOD_ACK);
			ns.Write(joinMethod);
			_ = last;
		}
	}
}
