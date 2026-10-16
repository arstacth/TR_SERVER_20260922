using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TowerEvent_UserJoinInfo_ACK : NetPacket
	{
		public TowerEvent_UserJoinInfo_ACK(int remainfree, int remaincash, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_MYINFO_ACK);
			ns.Write(remainfree);
			ns.Write(remaincash);
			ns.Write(0);
			_ = last;
		}
	}
}
