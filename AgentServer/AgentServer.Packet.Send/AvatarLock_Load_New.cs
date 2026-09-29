using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AvatarLock_Load_New : NetPacket
	{
		public AvatarLock_Load_New(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_AVATAR_LOCK_LOAD_ACK);
			ns.Write(0);
			User.avatarLock.encode(ns);
			_ = last;
		}
	}
}
