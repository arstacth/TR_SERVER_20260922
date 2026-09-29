using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class DecreaseObjectEnergy_Ack : NetPacket
	{
		public DecreaseObjectEnergy_Ack(Account User, NormalRoom room, int unk, int bossid, int reducehp, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_ASSAULT_MODE_DECREASE_OBJECT_ENERGY_ACK);
			ns.Write((int)User.RoomPos);
			ns.Write(unk);
			ns.Write(bossid);
			ns.Write(room.AnubisObjectBoss[bossid].HP);
			ns.Write(reducehp);
			_ = last;
		}
	}
}
