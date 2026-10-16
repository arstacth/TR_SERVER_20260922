using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_MiniGame_RoundTime : NetPacket
	{
		public GameRoom_MiniGame_RoundTime(Account User, int currentround, int isnextround, float RoundTime, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_GAMEROUND_INFO_NOTIFY);
			ns.Write(currentround);
			ns.Write(isnextround);
			ns.Write(RoundTime);
			_ = last;
		}
	}
}
