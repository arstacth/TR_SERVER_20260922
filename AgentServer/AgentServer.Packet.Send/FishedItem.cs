using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishedItem : NetPacket
	{
		public FishedItem(int itemid, int size, int usingbait, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_CATCH_FISH_NOTIFY);
			ns.Write(0);
			ns.Write((byte)1);
			ns.Write(itemid);
			ns.Write(size);
			ns.Write(1);
			ns.Write(usingbait);
			_ = last;
		}
	}
}
