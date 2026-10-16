using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_AddObjectValue_ACK : NetPacket
	{
		public Anniversary_AddObjectValue_ACK(int iObjectNum, int actionNum, int count, long iValue, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANNIVERSARY_ADD_OBJECT_VALUE_ACK);
			ns.Write(iObjectNum);
			ns.Write(actionNum);
			ns.Write(count);
			ns.Write(iValue);
			_ = last;
		}
	}
}
