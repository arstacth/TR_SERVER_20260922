using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishedItemFail : NetPacket
	{
		public FishedItemFail(int err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_CATCH_FISH_NOTIFY);
			ns.Write(err);
			_ = last;
		}
	}
}
