using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ExpiredItemInfo : NetPacket
	{
		public ExpiredItemInfo(int[] itemlist, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ACTIVE_ITEM_TIME_OUT_ACK);
			ns.Write(itemlist.Length);
			foreach (int value in itemlist)
			{
				ns.Write(value);
			}
			_ = last;
		}
	}
}
