using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnights_AttackCheck_AddExp_Ack : NetPacket
	{
		public TalesKnights_AttackCheck_AddExp_Ack(int GroupNum, int StageNum, List<TalesKnightsUnitExpInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_ADVENTURE_RESULT_ACK);
			ns.Write(0);
			ns.Write(Infos.Count);
			ns.Write(GroupNum);
			ns.Write(StageNum);
			foreach (TalesKnightsUnitExpInfo Info in Infos)
			{
				ns.Write(Info.BeforeExp);
				ns.Write(Info.NowExp);
				ns.Write(Info.UnitNum);
				ns.Write(Info.BeforeLevel);
				ns.Write(Info.NowLevel);
				ns.Write(Info.MaxLevel);
			}
			_ = last;
		}
	}
}
