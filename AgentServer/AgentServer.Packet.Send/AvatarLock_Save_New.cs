using System.Collections.Generic;
using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AvatarLock_Save_New : NetPacket
	{
		public AvatarLock_Save_New(Account User, List<int> m_lookItems, byte last)
		{
			ns.WriteOP(Opcodes.eServer_AVATAR_LOCK_SAVE_ACK);
			ns.Write(0);
			User.avatarLock.encode(ns);
			ns.Write(m_lookItems.Count);
			foreach (int m_lookItem in m_lookItems)
			{
				ns.Write(m_lookItem);
			}
			_ = last;
		}
	}
}
