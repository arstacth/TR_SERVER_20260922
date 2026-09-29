using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_SaveDefaultCharFail : NetPacket
	{
		public Myroom_SaveDefaultCharFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_SAVE_DEFAULT_CHARACTER_FAILED_ACK);
			_ = last;
		}
	}
}
