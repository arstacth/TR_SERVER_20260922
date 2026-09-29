using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_RepairItemFail : NetPacket
	{
		public Myroom_RepairItemFail(int itemnum, int repairitemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_REPAIR_ITEM_FAILED_ACK);
			ns.Write(itemnum);
			ns.Write(repairitemnum);
			_ = last;
		}
	}
}
