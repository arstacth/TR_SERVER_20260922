using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardUse_ACK : NetPacket
	{
		public EventPickBoardUse_ACK(int PickBoardNum, byte PickBoardStep, byte OrderNum, byte nextStep, bool IsReset, ExchangeItemInfo exinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_USE_ACK);
			ns.Write(PickBoardNum);
			ns.Write(0);
			ns.Write(PickBoardStep);
			ns.Write(OrderNum);
			ns.Write(exinfo.type);
			ns.Write(exinfo.id);
			ns.Write(exinfo.count);
			ns.Write(int.MaxValue);
			ns.Write(nextStep);
			ns.Write(IsReset);
			_ = last;
		}
	}
}
