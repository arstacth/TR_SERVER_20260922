using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DiceBoard_Draw_ACK : NetPacket
	{
		public DiceBoard_Draw_ACK(int DiceBoardNum, byte DrawNum, int RewardPos, int UserGauge, int FinishedItem, bool isFinished, byte PauseRound, List<int> RewardItem, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALES_MARBLE_PROTOCOL);
			ns.Write((int)eTalesMarbleProtocol.DRAW_REQ);
			ns.Write(0);
			ns.Write(DiceBoardNum);
			ns.Write(RewardPos);
			ns.Write(UserGauge);
			ns.Write(FinishedItem);
			ns.Write(0);
			ns.Write(isFinished);
			ns.Write((byte)0);
			ns.Write(PauseRound);
			ns.Write(DrawNum);
			ns.Write(RewardItem.Count);
			foreach (int item in RewardItem)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
