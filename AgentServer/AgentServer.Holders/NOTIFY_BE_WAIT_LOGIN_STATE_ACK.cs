using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Holders
{
	public sealed class NOTIFY_BE_WAIT_LOGIN_STATE_ACK : NetPacket
	{
		public NOTIFY_BE_WAIT_LOGIN_STATE_ACK(int waitUserNum, int totalWaitLoginNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_NOTIFY_BE_WAIT_LOGIN_STATE_ACK);
			ns.Write(waitUserNum);
			ns.Write(1);
			ns.Write(last);
		}
	}
}
