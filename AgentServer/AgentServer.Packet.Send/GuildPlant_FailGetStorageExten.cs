using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetStorageExtend : NetPacket
	{
		public GuildPlant_FailGetStorageExtend(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_STORAGE_EXTEND_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
