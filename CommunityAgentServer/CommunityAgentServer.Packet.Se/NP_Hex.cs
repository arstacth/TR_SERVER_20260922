using LocalCommons.Network;

namespace CommunityAgentServer.Packet.Send
{
	public sealed class NP_Hex : NetPacket
	{
		public NP_Hex(string value)
			: base(3, 0)
		{
			ns.WriteHex(value);
		}
	}
}
