using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class NewAttendanceGetUserInfoAck : NetPacket
	{
		public NewAttendanceGetUserInfoAck(byte last)
		{
			ns.Write((ushort)695);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
