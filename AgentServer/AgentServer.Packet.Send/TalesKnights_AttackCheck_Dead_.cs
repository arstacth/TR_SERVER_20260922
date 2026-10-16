using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnights_AttackCheck_Dead_Ack : NetPacket
	{
		public TalesKnights_AttackCheck_Dead_Ack(int GroupNum, int StageNum, List<int> DeadList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_ADVENTURE_RESULT_ACK);
			ns.Write(2);
			ns.Write(DeadList.Count);
			ns.Write(GroupNum);
			ns.Write(StageNum);
			foreach (int Dead in DeadList)
			{
				ns.Write(Dead);
			}
			_ = last;
		}
	}
}
