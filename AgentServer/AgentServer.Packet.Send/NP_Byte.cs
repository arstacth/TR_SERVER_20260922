using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class NP_Byte : NetPacket
	{
		public NP_Byte(byte[] value)
		{
			ns.Write(value, 0, value.Length);
		}
	}
}
