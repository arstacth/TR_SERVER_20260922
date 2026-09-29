using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_SetSlotItemSettingFail : NetPacket
	{
		public Myroom_SetSlotItemSettingFail(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_SET_SLOTITEM_SETTING_FAILED_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
