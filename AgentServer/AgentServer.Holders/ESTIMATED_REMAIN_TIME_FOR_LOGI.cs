using System;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Holders
{
	public sealed class ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK : NetPacket
	{
		public ESTIMATED_REMAIN_TIME_FOR_LOGIN_ACK(int waitUserNum, int totalWaitLoginNum, byte last)
		{
			_ = totalWaitLoginNum;
			// Wire 332 expects OP + remainCount + estimateSec + last (size 11).
			// ROOM_LIST is size 15 — if mis-dispatched, RemainSize=5.
			ns.Write((ushort)332);
			ns.Write(waitUserNum);
			ns.Write(Math.Max(1, waitUserNum * 3));
			ns.Write(last);
		}
	}
}
