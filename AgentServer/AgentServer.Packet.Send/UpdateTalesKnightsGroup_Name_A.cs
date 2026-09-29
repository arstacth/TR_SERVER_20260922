using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UpdateTalesKnightsGroup_Name_Ack : NetPacket
	{
		public UpdateTalesKnightsGroup_Name_Ack(TalesKnightsGroupName GPInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGNT_MYGROUP_NAME_UPDATE_ACK);
			ns.Write(GPInfo.OrderNumber);
			ns.WriteAnsiFixed_intSize(GPInfo.OrderName);
			_ = last;
		}
	}
}
