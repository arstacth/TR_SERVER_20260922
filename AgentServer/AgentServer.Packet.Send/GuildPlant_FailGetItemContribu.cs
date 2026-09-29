using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetItemContributionRankList : NetPacket
	{
		public GuildPlant_FailGetItemContributionRankList(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_ITEM_CONTRIBUTION_RANK_LIST_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
