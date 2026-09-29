using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MyRoom_GetMyAlchemistCards : NetPacket
	{
		public MyRoom_GetMyAlchemistCards(bool flag, Dictionary<int, int> cards, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_GET_MY_CARDS_LIST_ACK);
			ns.Write(flag);
			ns.Write(cards.Count);
			foreach (KeyValuePair<int, int> card in cards)
			{
				ns.Write(card.Key);
				ns.Write(card.Value);
			}
			_ = last;
		}
	}
}
