using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetDiceBoardList_ACK : NetPacket
	{
		public GetDiceBoardList_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALES_MARBLE_PROTOCOL);
			ns.Write((int)eTalesMarbleProtocol.GET_LIST_REQ);
			ns.Write(0);
			ns.Write(EventPickBoardHolder.DiceBoardList.Count);
			foreach (int diceBoard in EventPickBoardHolder.DiceBoardList)
			{
				ns.Write(diceBoard);
			}
			_ = last;
		}
	}
}
