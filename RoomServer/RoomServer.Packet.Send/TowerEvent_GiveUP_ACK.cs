using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_GiveUP_ACK : NetPacket
	{
		public TowerEvent_GiveUP_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_GIVEUP_ACK);
			_ = last;
		}
	}
}
