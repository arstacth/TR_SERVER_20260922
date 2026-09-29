using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnights_AttackCheck_RewardResult_Ack : NetPacket
	{
		public TalesKnights_AttackCheck_RewardResult_Ack(int StageGroupNum, int GroupNum, int StageNum, List<TalesKnights_AttackReward> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_ADVENTURE_RESULT_ACK);
			ns.Write(1);
			ns.Write(Infos.Count);
			ns.Write(StageGroupNum);
			ns.Write(GroupNum);
			ns.Write(StageNum);
			foreach (TalesKnights_AttackReward Info in Infos)
			{
				ns.Write(Info.RewardItem);
				ns.Write(Info.UnitNum);
			}
			_ = last;
		}
	}
}
