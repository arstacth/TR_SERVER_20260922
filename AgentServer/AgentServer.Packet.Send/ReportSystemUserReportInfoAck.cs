using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ReportSystemUserReportInfoAck : NetPacket
	{
		public ReportSystemUserReportInfoAck(byte last)
		{
			_ = last;
			// Packed eServer_REPORT_SYSTEM_USER_REPORT_INFO_ACK (wire 1158).
			ns.Write((ushort)1158);
			ns.Write(0);
			ns.Write((ushort)0);
		}
	}
}
