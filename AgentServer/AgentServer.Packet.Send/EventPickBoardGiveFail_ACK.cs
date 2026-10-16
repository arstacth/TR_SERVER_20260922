using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardGiveFail_ACK : NetPacket
	{
		public EventPickBoardGiveFail_ACK(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_GIVE_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
