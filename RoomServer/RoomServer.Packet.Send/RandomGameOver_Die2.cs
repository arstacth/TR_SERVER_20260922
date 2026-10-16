using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RandomGameOver_Die2 : NetPacket
	{
		public RandomGameOver_Die2(byte channelCheckPoint, bool isGameOver, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SURVIVAL_RANDOM_GAMEOVER_NOTIFY);
			ns.Write(channelCheckPoint);
			ns.Write(isGameOver);
			_ = last;
		}
	}
}
