namespace LocalCommons.Network
{
	public delegate void OnPacketReceive<T>(T net, PacketReader reader);
}
