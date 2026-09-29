using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnightsAdd_MaxLevel_Ack : NetPacket
	{
		public TalesKnightsAdd_MaxLevel_Ack(TalesKnightsAdd_MaxLevelInfo Info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_REINFORCE_UNIT_ACK);
			ns.Write(Info.UnitNum);
			ns.Write(Info.MaxLevel);
			ns.Write(Info.LevelUpScucess);
			_ = last;
		}
	}
}
