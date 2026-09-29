using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GuildPlant_FailGetGivePossibleUserList : NetPacket
	{
		public GuildPlant_FailGetGivePossibleUserList(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILD_PLANT_OPERATION_REQ);
			ns.WriteOP(GuildPlantProtocol.GET_GIVE_POSSIBLE_USER_LIST_FAIL_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
