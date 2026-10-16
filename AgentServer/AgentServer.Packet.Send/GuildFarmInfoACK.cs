using AgentServer.Structuring.Guild;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildFarmInfoACK : NetPacket
	{
		public GuildFarmInfoACK(GuildFarmInfo farminfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.GetGuildFarmInfo_ACK);
			ns.Write(0);
			ns.Write(farminfo.FarmIndex);
			ns.Write(0L);
			ns.Write(farminfo.FarmTypeNum);
			ns.WriteAnsiFixed_intSize(farminfo.FarmName);
			ns.Write((short)0);
			ns.Write(farminfo.ExpireDateTime);
			ns.Write(farminfo.CreateDateTime);
			ns.Write((byte)1);
			ns.Write(value: false);
			ns.Write((byte)0);
			ns.Write(farminfo.TotalVisitedCount);
			ns.Write(farminfo.TodaysVisitorCount);
			ns.Fill(3);
			ns.Write(value: false);
			ns.Write(0L);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
