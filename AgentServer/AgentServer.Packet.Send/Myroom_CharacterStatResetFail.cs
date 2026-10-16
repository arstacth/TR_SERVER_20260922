using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_CharacterStatResetFail : NetPacket
	{
		public Myroom_CharacterStatResetFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_CHARACTER_STAT_RESET_FAILED_ACK);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
