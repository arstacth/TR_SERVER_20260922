using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CorunMode_SetBossEnergy_Ack : NetPacket
	{
		public CorunMode_SetBossEnergy_Ack(int boss, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_SET_BOSS_ENERGY_ACK);
			ns.Write(boss);
			_ = last;
		}
	}
}
