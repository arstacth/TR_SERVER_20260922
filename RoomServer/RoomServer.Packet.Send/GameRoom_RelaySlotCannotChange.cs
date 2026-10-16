using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RelaySlotCannotChange : NetPacket
	{
		public GameRoom_RelaySlotCannotChange(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_SLOT_STATE_RELAY_TEAM_FAILED_ACK);
			_ = last;
		}
	}
}
