using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardUserInfoFail_ACK : NetPacket
	{
		public EventPickBoardUserInfoFail_ACK(int PickBoardNum, int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_USER_INFO_ACK);
			ns.Write(PickBoardNum);
			ns.Write(err);
			_ = last;
		}
	}
}
