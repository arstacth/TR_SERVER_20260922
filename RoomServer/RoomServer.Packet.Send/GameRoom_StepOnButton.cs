using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_StepOnButton : NetPacket
	{
		public GameRoom_StepOnButton(byte[] unk, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ONE_IN_FOUR_KEY_IN_DOOR_ACK);
			ns.Write(unk, 0, 12);
			_ = last;
		}
	}
}
