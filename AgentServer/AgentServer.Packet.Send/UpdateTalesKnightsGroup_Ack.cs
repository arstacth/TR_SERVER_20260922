using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.TalesKnight;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UpdateTalesKnightsGroup_Ack : NetPacket
	{
		public UpdateTalesKnightsGroup_Ack(int GroupNum, List<TalesKnightsGroupInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALESKNIGHT_MYGROUP_UNIT_UPDATE_ACK);
			ns.Write(Infos.Count);
			ns.Write((short)GroupNum);
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
