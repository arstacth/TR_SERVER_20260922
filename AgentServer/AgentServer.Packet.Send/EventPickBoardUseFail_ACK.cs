using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardUseFail_ACK : NetPacket
	{
		public EventPickBoardUseFail_ACK(int PickBoardNum, int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_USE_ACK);
			ns.Write(PickBoardNum);
			ns.Write(err);
			_ = last;
		}
	}
}
