using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ApplyHotTimeEventFail : NetPacket
	{
		public ApplyHotTimeEventFail(byte err, long hottimeid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_HOTTIME_EVENT_APPLY_FAILD_ACK);
			ns.Write(err);
			ns.Write(hottimeid);
			_ = last;
		}
	}
}
