using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_GetStorageExtend : NetPacket
	{
		public GuildPlant_GetStorageExtend(int extendCount, int extendValue, int userInvestCount, int userInvestValue, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_STORAGE_EXTEND_ACK);
			ns.Write(extendCount);
			ns.Write(extendValue);
			ns.Write(userInvestCount);
			ns.Write(userInvestValue);
			_ = last;
		}
	}
}
