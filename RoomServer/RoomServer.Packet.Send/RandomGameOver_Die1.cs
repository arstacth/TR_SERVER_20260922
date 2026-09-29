using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class RandomGameOver_Die1 : NetPacket
	{
		public RandomGameOver_Die1(byte channelCheckPoint, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_SURVIVAL_RANDOM_GAMEOVER_RACE_LENGTH_ACK);
			ns.Write(channelCheckPoint);
			_ = last;
		}
	}
}
