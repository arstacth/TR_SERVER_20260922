using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class StoneMountFail : NetPacket
	{
		public StoneMountFail(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_MOUNT_ACK);
			ns.Write(15);
			_ = last;
		}

		public StoneMountFail(eENCHANT_SYSTEM_RESULT result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENCHANT_SYSTEM_MOUNT_ACK);
			ns.Write((int)result);
			_ = last;
		}
	}
}
