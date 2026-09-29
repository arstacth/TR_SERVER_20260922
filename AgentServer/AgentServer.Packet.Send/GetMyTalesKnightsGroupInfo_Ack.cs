using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetMyTalesKnightsGroupInfo_Ack : NetPacket
	{
		public GetMyTalesKnightsGroupInfo_Ack(List<TalesKnightsGroupInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_MYGROUPINFO_ACK_UNIT);
			ns.Write(Infos.Count);
			foreach (TalesKnightsGroupInfo Info in Infos)
			{
				ns.Write(Info.UnitNum);
				ns.Write(Info.OrderNumber);
				ns.Write(Info.UnitSlot);
			}
			_ = last;
		}
	}
}
