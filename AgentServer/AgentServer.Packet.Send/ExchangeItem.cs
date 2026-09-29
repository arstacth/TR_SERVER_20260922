using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ExchangeItem : NetPacket
	{
		public ExchangeItem(List<ExchangeItemInfo> exinfo, int count, byte last)
		{
			bool flag = exinfo.Count > 0;
			ns.WriteOP(Opcodes.eServer_EXCHANGE_SYSTEM_EXCHANGE_ACK);
			ns.Write((!flag) ? 2 : 0);
			if (flag)
			{
				ns.Write(exinfo.Count);
				foreach (ExchangeItemInfo item in exinfo)
				{
					ns.Write(item.type);
					ns.Write(item.id);
					ns.Write(item.count);
					ns.Write(int.MaxValue);
				}
				ns.Write(count);
				ns.Write(0);
			}
			_ = last;
		}
	}
}
