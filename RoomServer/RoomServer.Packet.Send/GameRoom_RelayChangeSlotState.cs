using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RelayChangeSlotState : NetPacket
	{
		public GameRoom_RelayChangeSlotState(byte slotid, bool isOFF, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CHANGE_SLOT_STATE_RELAY_TEAM_OK_ACK);
			ns.Write(slotid);
			ns.Write(isOFF);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
