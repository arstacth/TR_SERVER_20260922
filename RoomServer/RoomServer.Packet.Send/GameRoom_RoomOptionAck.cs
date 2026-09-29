using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_RoomOptionAck : NetPacket
	{
		public GameRoom_RoomOptionAck(ushort ackOp, string name, byte last)
		{
			ns.WriteOP(ackOp);
			ns.WriteAnsiFixed_intSize(name ?? string.Empty);
			_ = last;
		}

		public GameRoom_RoomOptionAck(ushort ackOp, int itemType, byte last)
		{
			ns.WriteOP(ackOp);
			ns.Write(itemType);
			_ = last;
		}

		public GameRoom_RoomOptionAck(ushort ackOp, bool stepOn, byte last)
		{
			ns.WriteOP(ackOp);
			ns.Write(stepOn);
			_ = last;
		}
	}
}
