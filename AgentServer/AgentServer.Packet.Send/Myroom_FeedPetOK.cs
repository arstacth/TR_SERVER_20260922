using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_FeedPetOK : NetPacket
	{
		public Myroom_FeedPetOK(int petitemnum, int feeditemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_PET_FEED_ACK);
			ns.Write(petitemnum);
			ns.Write(feeditemnum);
			_ = last;
		}
	}
}
