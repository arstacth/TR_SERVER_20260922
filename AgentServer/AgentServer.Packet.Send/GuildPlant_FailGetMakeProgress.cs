using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetMakeProgressItem : NetPacket
	{
		public GuildPlant_FailGetMakeProgressItem(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_MAKE_PROGRESS_ITEM_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
