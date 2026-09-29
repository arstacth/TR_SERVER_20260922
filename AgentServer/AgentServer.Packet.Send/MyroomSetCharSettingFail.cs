using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyroomSetCharSettingFail : NetPacket
	{
		public MyroomSetCharSettingFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_SAVE_CHARACTER_SETTING_FAILED_ACK);
			_ = last;
		}
	}
}
