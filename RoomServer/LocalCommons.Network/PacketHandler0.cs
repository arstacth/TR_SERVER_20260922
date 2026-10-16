namespace LocalCommons.Network
{
	public class PacketHandler0<T>
	{
		private readonly int m_PacketID;

		private readonly OnPacketReceive<T> m_OnReceive;

		public int PacketID => m_PacketID;

		public OnPacketReceive<T> OnReceive => m_OnReceive;

		public PacketHandler0(int packetID, OnPacketReceive<T> onReceive)
		{
			m_PacketID = packetID;
			m_OnReceive = onReceive;
		}
	}
}
