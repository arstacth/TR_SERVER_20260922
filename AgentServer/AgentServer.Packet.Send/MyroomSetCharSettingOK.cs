using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyroomSetCharSettingOK : NetPacket
	{
		public MyroomSetCharSettingOK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_SAVE_CHARACTER_SETTING_ACK);
			_ = last;
		}
	}
}
