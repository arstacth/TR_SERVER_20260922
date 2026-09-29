using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ClearFarmSlot_Ack : NetPacket
	{
		public ClearFarmSlot_Ack(int SlotNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ClearFarmSlotInfo_ACK);
			ns.Write(0);
			ns.Write(SlotNum);
			_ = last;
		}
	}
}
