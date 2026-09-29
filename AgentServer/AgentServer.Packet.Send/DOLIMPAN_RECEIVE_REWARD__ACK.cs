using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DOLIMPAN_RECEIVE_REWARD__ACK : NetPacket
	{
		public DOLIMPAN_RECEIVE_REWARD__ACK(DolimpanHandle.Dolimpan result, Tuple<int, int> ret, byte idx, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DOLIMPAN_RECEIVE_REWARD__ACK);
			ns.Write((int)result);
			if (result == DolimpanHandle.Dolimpan.eServerResult_OK_ACK)
			{
				ns.Write(ret.Item1);
				ns.Write(ret.Item2);
				ns.Write(idx);
			}
			_ = last;
		}
	}
}
