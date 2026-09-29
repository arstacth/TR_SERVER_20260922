using CommunityAgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace CommunityAgentServer.Packet.Send
{
	public sealed class NP_0x08 : NetPacket
	{
		public NP_0x08(string nickname, byte[] remain)
			: base(3, 0)
		{
			ns.WriteOP(eCommunityAgentOpcode.FORWARD_OFFLINE_ACK);
			ns.WriteBIG5Fixed_shortSize(nickname);
			ns.Write(remain, 0);
		}
	}
}
