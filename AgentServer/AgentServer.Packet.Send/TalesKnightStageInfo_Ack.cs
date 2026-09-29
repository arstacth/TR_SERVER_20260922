using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TalesKnightStageInfo_Ack : NetPacket
	{
		public TalesKnightStageInfo_Ack(List<TalesKnightStageInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_STAGEINFO_ACK);
			ns.Write(Infos.Count);
			foreach (TalesKnightStageInfo Info in Infos)
			{
				ns.Write(Info.StageGroupNum);
				ns.Write(Info.StageNum);
				ns.Write(Info.Attribute);
				ns.Write(Info.HP);
				ns.Write(Info.StatusType);
				ns.Write(0L);
			}
			_ = last;
		}
	}
}
