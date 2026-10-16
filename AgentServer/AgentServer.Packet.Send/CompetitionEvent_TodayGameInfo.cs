using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_TodayGameInfo : NetPacket
	{
		public CompetitionEvent_TodayGameInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_TODAY_GAME_ACK);
			ns.Write(1);
			ns.Write(1);
			ns.Write(8);
			ns.Write(56);
			ns.Write(0);
			ns.Write(1);
			ns.Write(0);
			ns.Write((byte)0);
			ns.Write(Utility.ConvertToTimestamp(DateTime.Today));
			ns.Write(Utility.ConvertToTimestamp(DateTime.Today.AddHours(24.0)));
			ns.Write((short)1);
			ns.Write(48);
			ns.Write(0L);
			_ = last;
		}
	}
	public sealed class CompetitionEvent_TodayGameInfo2 : NetPacket
	{
		public CompetitionEvent_TodayGameInfo2(byte last)
		{
			ns.WriteHex("AF06000000000000000000000000");
			_ = last;
		}
	}
}
