using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CorunMode_DecreaseBossEnergy_Ack : NetPacket
	{
		public CorunMode_DecreaseBossEnergy_Ack(int boss, int hp, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_DECREASE_BOSS_ENERGY_ACK);
			ns.Write(hp);
			ns.Write(boss);
			_ = last;
		}
	}
}
