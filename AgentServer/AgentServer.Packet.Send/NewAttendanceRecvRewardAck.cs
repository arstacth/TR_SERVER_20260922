using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// Wire 943. After status+day, client popRawData(20) for reward blob (Overpop 11 11 20).
	/// </summary>
	public sealed class NewAttendanceRecvRewardAck : NetPacket
	{
		public NewAttendanceRecvRewardAck(int error, int day, byte last)
		{
			ns.Write((ushort)943);
			ns.Write(error);
			ns.Write(day);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
