using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CorunMode_IncreaseObjectBossEnergy_Ack : NetPacket
	{
		public CorunMode_IncreaseObjectBossEnergy_Ack(int boss, long bossid, int hp, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_CORUN_MODE_INCREASE_OBJECT_BOSS_ENERGY_ACK);
			ns.Write(boss);
			ns.Write(bossid);
			ns.Write(hp);
			_ = last;
		}
	}
}
