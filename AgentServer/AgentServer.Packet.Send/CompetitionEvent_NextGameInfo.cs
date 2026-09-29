using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class CompetitionEvent_NextGameInfo : NetPacket
	{
		public CompetitionEvent_NextGameInfo(byte last)
		{
			ns.WriteOP(Opcodes.eServer_COMPETITION_EVENT_NEXT_GAMEINFO_ACK);
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
}
