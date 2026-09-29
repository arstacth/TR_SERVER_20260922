using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class BuyFarmSlotOK_Ack : NetPacket
	{
		public BuyFarmSlotOK_Ack(int SlotNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.BuyFarmSlot_ACK);
			ns.Write(0);
			ns.Write(SlotNum);
			_ = last;
		}
	}
}
