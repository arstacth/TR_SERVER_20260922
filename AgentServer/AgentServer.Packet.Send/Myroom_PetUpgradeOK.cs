using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_PetUpgradeOK : NetPacket
	{
		public Myroom_PetUpgradeOK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_PET_UPGRADE_OK_ACK);
			_ = last;
		}
	}
}
