using LocalCommons.Network;
using NetMsg.Room;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_PlayingUpdate : NetPacket
	{
		public RM_To_AG_PlayingUpdate(int roomid, bool isPlaying, int bonusLV)
		{
			ns.WriteOP(RMProtocol.RM_StartGame_REQ);
			ns.Write(roomid);
			ns.Write(isPlaying);
			ns.Write(bonusLV);
		}
	}
}
