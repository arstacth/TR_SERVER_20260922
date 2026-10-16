using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_ITEM_COLLECTION_ADD_ACK : NetPacket
	{
		public eServer_ITEM_COLLECTION_ADD_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_COLLECTION_ADD_ACK);
			ns.Write(0);
			_ = last;
		}
	}
}
