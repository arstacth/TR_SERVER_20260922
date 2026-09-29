using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetCommunityAgentServer : NetPacket
	{
		public GetCommunityAgentServer(byte last)
		{
			string ip = Conf.ServerIP;
			if (string.IsNullOrWhiteSpace(ip))
			{
				ip = "127.0.0.1";
			}
			int port = Conf.CommunityAgentServerPort > 0 ? Conf.CommunityAgentServerPort : 9000;
			ns.WriteOP(Opcodes.eServer_COMMUNITY_SERVER_PROTOCOL);
			ns.WriteOP(eCommunityProtocol.GET_COMMUNITY_AGENT_SERVER_ACK);
			ns.Write((byte)1);
			ns.WriteAnsiFixed_intSize(ip);
			ns.Write(port);
			_ = last;
		}
	}
}
