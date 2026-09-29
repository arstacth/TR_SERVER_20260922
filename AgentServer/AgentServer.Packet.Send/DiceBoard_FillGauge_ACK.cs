using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DiceBoard_FillGauge_ACK : NetPacket
	{
		public DiceBoard_FillGauge_ACK(int DiceBoardNum, int Gauge, int ItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALES_MARBLE_PROTOCOL);
			ns.Write((int)eTalesMarbleProtocol.FILL_GAUGE_REQ);
			ns.Write(0);
			ns.Write(DiceBoardNum);
			ns.Write(Gauge);
			ns.Write(ItemNum);
			_ = last;
		}
	}
}
