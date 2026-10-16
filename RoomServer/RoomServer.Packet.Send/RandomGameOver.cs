using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RandomGameOver : NetPacket
	{
		public RandomGameOver(byte channelCheckPoint, byte byGameIndex, List<byte> ranklist, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SURVIVAL_RANDOM_GAMEOVER_CHECK_POINT_ACK);
			ns.Write(channelCheckPoint);
			ns.Write(byGameIndex);
			ns.Write(ranklist.Count);
			foreach (byte item in ranklist)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
