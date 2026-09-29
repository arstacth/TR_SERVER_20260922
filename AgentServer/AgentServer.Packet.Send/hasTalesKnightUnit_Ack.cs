using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class hasTalesKnightUnit_Ack : NetPacket
	{
		public hasTalesKnightUnit_Ack(List<TalesKnightUnitInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_UNITCHECK_ACK);
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
