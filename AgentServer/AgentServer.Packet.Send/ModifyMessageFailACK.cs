using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ModifyMessageFailACK : NetPacket
	{
		public ModifyMessageFailACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_OPERATION_REQ);
			ns.WriteOP(eGuildProtocol.MODIFY_MESSAGE_FAIL_ACK);
			ns.Write((short)8);
			_ = last;
		}
	}
}
