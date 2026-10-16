using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyRoom_GetCharacterSetting_Fail_ACK : NetPacket
	{
		public MyRoom_GetCharacterSetting_Fail_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_GET_MY_CHARACTER_SETTING_FAILED_ACK);
			_ = last;
		}
	}
}
