using LocalCommons.Network;

namespace LoadBalanceServer.Packet.Send
{
	public sealed class NP_Byte : NetPacket
	{
		public NP_Byte(byte[] value)
			: base(3, 0)
		{
			ns.Write(value, 0, value.Length);
		}
	}
}
