using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_SaveDefaultChar : NetPacket
	{
		public Myroom_SaveDefaultChar(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_SAVE_DEFAULT_CHARACTER_ACK);
			_ = last;
		}
	}
}
