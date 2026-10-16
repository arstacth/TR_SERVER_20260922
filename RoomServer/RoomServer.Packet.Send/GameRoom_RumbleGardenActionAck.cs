using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RumbleGardenActionAck : NetPacket
	{
		public GameRoom_RumbleGardenActionAck(byte[] payload, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RUMBLE_GARDEN_ACTION_ACK);
			if (payload != null && payload.Length > 0)
			{
				ns.Write(payload, 0, payload.Length);
			}
			_ = last;
		}
	}

	public sealed class GameRoom_RumbleGardenGoalInAck : NetPacket
	{
		public GameRoom_RumbleGardenGoalInAck(byte roomPos, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_RUMBLE_GARDEN_GOALIN_ACK);
			ns.Write(roomPos);
			_ = last;
		}
	}
}
