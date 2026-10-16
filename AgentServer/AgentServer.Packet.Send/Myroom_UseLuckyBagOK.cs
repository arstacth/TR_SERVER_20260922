using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_UseLuckyBagOK : NetPacket
	{
		public Myroom_UseLuckyBagOK(int itemnum, byte opennum, Dictionary<int, int> itemlist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_LUCKY_BAG_ACK);
			ns.Write(0);
			ns.Write(itemlist.Count);
			foreach (KeyValuePair<int, int> item in itemlist)
			{
				ns.Write(item.Key);
				// Client expects count as byte. Writing int left RemainSize=3 and looped open-box UI.
				byte count = (byte)(item.Value > 255 ? 255 : (item.Value < 0 ? 0 : item.Value));
				ns.Write(count);
			}
			ns.Write(itemnum);
			ns.Write(opennum);
			// Trailing last left RemainSize=1 after a clean item parse.
			_ = last;
		}
	}
}
