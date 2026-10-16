using AgentServer.Structuring.GuildPlant;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetMakeProgressItem : NetPacket
	{
		public GuildPlant_GetMakeProgressItem(GuildPlantMakeInfo makeInfo, int myContributionPoint, int distributeKind, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_MAKE_PROGRESS_ITEM_ACK);
			if (makeInfo == null)
			{
				ns.Write(0);
				ns.Write(0);
				ns.WriteHex("745365727665724D");
				ns.Write(0);
				ns.Write(0);
			}
			else
			{
				ns.Write(makeInfo.ItemIndexNum);
				ns.Write(makeInfo.ItemDescNum);
				ns.Write(makeInfo.NeedPoint);
				ns.Write(makeInfo.AccumulatePoint);
				ns.Write(Utility.ConvertToTimestamp(makeInfo.FinishDate));
			}
			ns.Write(distributeKind);
			ns.Write(myContributionPoint);
			_ = last;
		}
	}
}
