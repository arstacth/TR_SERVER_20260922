using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ChangeFarmPassword_Ack : NetPacket
	{
		public ChangeFarmPassword_Ack(int FarmUniqueNum, string pw, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FARM_MODIFY_OPTION_PUBLIC_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(pw == string.Empty);
			ns.WriteAnsiFixed_intSize(pw);
			_ = last;
		}
	}
}
