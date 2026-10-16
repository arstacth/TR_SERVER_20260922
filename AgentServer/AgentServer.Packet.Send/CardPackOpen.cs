using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CardPackOpen : NetPacket
	{
		public CardPackOpen(int cardpacknum, short opentype, List<CardPackResultInfo> cardpackinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CARDPACK_OPENCARDPACK_ACK);
			ns.Write(cardpacknum);
			ns.Write(opentype);
			ns.Write(cardpackinfos.Count);
			foreach (CardPackResultInfo cardpackinfo in cardpackinfos)
			{
				ns.Write(cardpackinfo.RewardType);
				ns.Write(cardpackinfo.RewardItem);
				ns.Write(cardpackinfo.RewardCount);
				ns.Write(int.MaxValue);
			}
			_ = last;
		}
	}
}
