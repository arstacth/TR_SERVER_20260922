using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class AloneRunGameOver_ACK : NetPacket
	{
		public AloneRunGameOver_ACK(int ms, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ALONERUN_GAME_OVER_ACK);
			ns.Write(ms);
			ns.WriteThaiLast(last);
		}
	}
}
