using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SetFarmPortalInfoOK_Ack : NetPacket
	{
		public SetFarmPortalInfoOK_Ack(long OID, int FarmUniqueNum, string memo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.SetFarmPortalInfo_ACK);
			ns.Write(OID);
			ns.Write(FarmUniqueNum);
			ns.WriteAnsiFixed_intSize(memo);
			_ = last;
		}
	}
}
