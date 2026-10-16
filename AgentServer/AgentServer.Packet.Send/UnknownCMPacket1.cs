using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class UnknownCMPacket1 : NetPacket
	{
		public UnknownCMPacket1(byte last, eCommunityProtocol ack = eCommunityProtocol.COMMUNITY_38_ACK)
		{
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(ack);
			if (ack == eCommunityProtocol.COMMUNITY_38_ACK)
			{
				ns.Write((short)0);
			}
			else if (ack == eCommunityProtocol.COMMUNITY_46_ACK)
			{
				ns.Write((byte)0);
				ns.Write(0);
			}
			else
			{
				ns.Write((byte)0);
			}
			_ = last;
		}
	}
}
