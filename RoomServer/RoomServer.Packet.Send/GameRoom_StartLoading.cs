using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StartLoading : NetPacket
	{
		public GameRoom_StartLoading(int mapid, int randseed, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_START_LOADING_ACK);
			ns.Write(mapid);
			ns.Write(randseed);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
