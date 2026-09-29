using CommunityAgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace CommunityAgentServer.Packet.Send
{
	public sealed class SetMyProfile : NetPacket
	{
		public SetMyProfile()
			: base(3, 0)
		{
			ns.WriteOP(eCommunityAgentOpcode.PROFILE_ACK);
			ns.WriteOP(eCommunityAgentProfile.SET_PROFILE_ACK);
			ns.Write((byte)0);
		}
	}
}
