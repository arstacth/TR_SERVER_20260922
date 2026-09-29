using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HuMongPickBoard_GiveItem_Fail : NetPacket
	{
		public HuMongPickBoard_GiveItem_Fail(eServerResult result, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_HUMONG_PICKBOARD_CONFIRM_ACK);
			ns.Write((int)result);
		}
	}
}
