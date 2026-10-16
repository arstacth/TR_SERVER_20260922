using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Park;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Anniversary_ObjectAction_ACK : NetPacket
	{
		public Anniversary_ObjectAction_ACK(int iObjectNum, int actionNum, int count, AnniversaryAction info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANNIVERSARY_OBJECT_ACTION_ACK);
			ns.Write(iObjectNum);
			ns.Write(actionNum);
			ns.Write(count);
			ns.Write(info.ActionNum);
			ns.Write(info.ActionValue);
			ns.Write(info.ActionType);
			_ = last;
		}
	}
}
