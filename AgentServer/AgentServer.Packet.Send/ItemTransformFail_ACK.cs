using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ItemTransformFail_ACK : NetPacket
	{
		public ItemTransformFail_ACK(short err, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TRANSFORM_ITEM_FAILED_ACK);
			ns.Write(err);
			_ = last;
		}
	}
}
