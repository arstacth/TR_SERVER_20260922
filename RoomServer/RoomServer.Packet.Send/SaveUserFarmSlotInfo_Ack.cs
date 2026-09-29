using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SaveUserFarmSlotInfo_Ack : NetPacket
	{
		public SaveUserFarmSlotInfo_Ack(int SlotNum, int FarmTypeNum, string title, string memo, long dt, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SaveUserFarmSlotInfo_ACK);
			ns.Write(0);
			ns.Write(SlotNum);
			ns.Write(SlotNum);
			ns.Write(FarmTypeNum);
			ns.WriteAnsiFixed_intSize(title);
			ns.WriteAnsiFixed_intSize(memo);
			ns.Write(dt);
			_ = last;
		}
	}
}
