using System.Collections.Generic;
using AgentServer.Structuring.Gacha;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Archives_Exchange_ACK : NetPacket
	{
		public Archives_Exchange_ACK(Archives.eArchives_Result result, List<ArchivesReward> RewardItem, int SupplyItemNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ARCHIVE_ARTIFACT_EXCHANGE_ACK);
			ns.Write(RewardItem.Count);
			ns.Write((byte)result);
			foreach (ArchivesReward item in RewardItem)
			{
				ns.Write(item.ItemNum);
				ns.Write(item.isGive);
			}
			ns.Write(SupplyItemNum);
			_ = last;
		}
	}
}
