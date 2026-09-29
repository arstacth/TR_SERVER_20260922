using LocalCommons.Network;
using RoomServer.Structuring;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_GameResult2 : NetPacket
	{
		public GameRoom_GameResult2(Account User, byte[] result)
		{
			ns.Write(result, 0, result.Length);
		}
	}
}
