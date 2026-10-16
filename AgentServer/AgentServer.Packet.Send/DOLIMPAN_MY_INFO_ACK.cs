using System;
using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DOLIMPAN_MY_INFO_ACK : NetPacket
	{
		public DOLIMPAN_MY_INFO_ACK(DolimpanHandle.Dolimpan result, short iDolimpanPoint, List<Tuple<int, int>> ret, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DOLIMPAN_MY_INFO_ACK);
			ns.Write((int)result);
			if (result == DolimpanHandle.Dolimpan.eServerResult_OK_ACK)
			{
				ns.Write(iDolimpanPoint);
				ns.Write(ret.Count);
				foreach (Tuple<int, int> item in ret)
				{
					ns.Write(item.Item1);
					ns.Write(item.Item2);
				}
			}
			_ = last;
		}
	}
}
