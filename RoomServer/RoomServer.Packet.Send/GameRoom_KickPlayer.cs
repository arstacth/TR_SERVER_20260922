using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_KickPlayer : NetPacket
	{
		public GameRoom_KickPlayer(byte roompos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_KICK_PLAYER_REQ);
			ns.Write(roompos);
			_ = last;
		}
	}
}
