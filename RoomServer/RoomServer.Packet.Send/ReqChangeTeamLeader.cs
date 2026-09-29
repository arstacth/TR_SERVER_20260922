using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ReqChangeTeamLeader : NetPacket
	{
		public ReqChangeTeamLeader(int type, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RABBIT_TURTLE_TAG_ACK);
			ns.Write(type);
			ns.Write((type == 1) ? 2 : 0);
			_ = last;
		}
	}
}
