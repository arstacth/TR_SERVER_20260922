using System.Linq;
using LocalCommons.Network;
using NetMsg.Room;
using RoomServer.Structuring;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_UpdateTeamInfo : NetPacket
	{
		public RM_To_AG_UpdateTeamInfo(NormalRoom room)
		{
			ns.WriteOP(RMProtocol.RM_UpdateTeamInfo);
			ns.Write(room.ID);
			ns.Write(room.PlayerList().Count((Account c) => c.Team == 1 && c.Attribute != 3));
			ns.Write(room.PlayerList().Count((Account c) => c.Team == 2 && c.Attribute != 3));
		}
	}
}
