using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMyTalesKnightUnitInfo_Ack : NetPacket
	{
		public GetMyTalesKnightUnitInfo_Ack(List<TalesKnightUnitInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_MYUNITINFO_ACK);
			ns.Write(Infos.Count);
			foreach (TalesKnightUnitInfo Info in Infos)
			{
				ns.Write(Info.UnitNum);
				ns.Write(Info.UnitReinForceCount);
				ns.Write(Info.UnitLevel);
				ns.Write(Info.UnitMaxLevel);
				ns.Write(Info.UnitExp);
				ns.Write(Info.UnitGotDate);
			}
			_ = last;
		}
	}
}
