using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishingPacket : NetPacket
	{
		public FishingPacket(byte action, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_PROC_FISHING_ACK);
			ns.Write(0);
			ns.Write(action);
			// Size 8 left Remain=1; size 7 was Overpop +1 on stop only.
			// Match start (works at 7). Stop: append byte only when action==0.
			if (action == 0)
			{
				ns.Write((byte)0);
			}
		}
	}
}
