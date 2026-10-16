using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnightsUseExpUpItem_Ack : NetPacket
	{
		public TalesKnightsUseExpUpItem_Ack(int ItemNum, TalesKnightsUnitExpInfo Info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_USE_EXPUP_ITEM_ACK);
			ns.Write(Info.BeforeExp);
			ns.Write(Info.NowExp);
			ns.Write(Info.UnitNum);
			ns.Write(Info.BeforeLevel);
			ns.Write(Info.NowLevel);
			ns.Write(Info.MaxLevel);
			ns.Write(ItemNum);
			_ = last;
		}
	}
}
