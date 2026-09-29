using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_EXP_REQ : NetPacket
	{
		public eServer_GET_EXP_REQ(short type, long value, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_EXP_ACK);
			// Packed: int result, int64 iCurExp, short type. 19:34 Remain 1
			// size 17 with result+type+int64 still logged iCurExp garbage
			// (~1e14) > iExp(real). Same size, swapped type/int64.
			ns.Write(0);
			ns.Write(value);
			ns.Write(type);
			_ = last;
		}
	}
}
