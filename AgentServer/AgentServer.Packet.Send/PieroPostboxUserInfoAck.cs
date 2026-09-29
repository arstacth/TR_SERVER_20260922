using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PieroPostboxUserInfoAck : NetPacket
	{
		public PieroPostboxUserInfoAck(byte last)
		{
			ns.Write((ushort)440);
			ns.Write(0);
			_ = last;
		}
	}
}
