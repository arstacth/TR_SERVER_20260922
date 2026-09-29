using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CorunMode_SetObjectBossEnergy_Ack : NetPacket
	{
		public CorunMode_SetObjectBossEnergy_Ack(int boss, long bossid, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_SET_OBJECT_BOSS_ENERGY_ACK);
			ns.Write(boss);
			ns.Write(bossid);
			_ = last;
		}
	}
}
