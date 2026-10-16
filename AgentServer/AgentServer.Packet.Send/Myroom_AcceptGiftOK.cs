using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_AcceptGiftOK : NetPacket
	{
		public Myroom_AcceptGiftOK(List<int> itemList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_ACCEPT_GIFT_ACK);
			ns.Write(0);
			ns.Write(itemList.Count);
			foreach (int item in itemList)
			{
				ns.Write(item);
			}
			// itemList must be itemDescNums the client can look up, not gift uniNums.
			// Do not write trailing last / pad — RemainSize=1 on OK was the pad byte.
			_ = last;
		}
	}
}
