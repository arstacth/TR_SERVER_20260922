using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class TowerEvent_Notify_ACK : NetPacket
	{
		public TowerEvent_Notify_ACK(byte type, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_NOTYFY_TIME);
			ns.Write(type);
			_ = last;
		}
	}
}
