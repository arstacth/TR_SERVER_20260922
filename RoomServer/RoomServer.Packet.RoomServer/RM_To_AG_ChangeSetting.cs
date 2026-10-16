using LocalCommons.Network;
using NetMsg.Room;
using RoomServer.Structuring;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_ChangeSetting : NetPacket
	{
		public RM_To_AG_ChangeSetting(NormalRoom room)
		{
			ns.WriteOP(RMProtocol.RM_ChangeSetting_REQ);
			ns.Write(room.ID);
			ns.WriteAnsiFixed_intSize(room.Name);
			ns.WriteAnsiFixed_intSize(room.Password);
			ns.Write(room.IsStepOn);
			ns.Write(room.ItemType);
		}
	}
}
