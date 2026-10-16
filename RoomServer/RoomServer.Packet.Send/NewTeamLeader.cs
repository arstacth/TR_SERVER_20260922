using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class NewTeamLeader : NetPacket
	{
		public NewTeamLeader(int leaderpos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RABBIT_TURTLE_CHANGE_LEADER);
			ns.Write(leaderpos);
			_ = last;
		}
	}
}
