using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_RepairItemOK : NetPacket
	{
		public Myroom_RepairItemOK(int itemnum, int repairitemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_REPAIR_ITEM_OK_ACK);
			ns.Write(itemnum);
			ns.Write(repairitemnum);
			_ = last;
		}
	}
}
