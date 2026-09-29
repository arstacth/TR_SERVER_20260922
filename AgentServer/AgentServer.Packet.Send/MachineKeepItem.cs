using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MachineKeepItem : NetPacket
	{
		public MachineKeepItem(Account User, bool isSuccess, long uniqueNum, int itemNum, long dateTime, byte last)
		{
			ns.WriteOP(Opcodes.eServer_STORAGE_SAVE_ITEM_ACK);
			ns.Write((!isSuccess) ? 3 : 0);
			ns.Write(1);
			ns.Write(uniqueNum);
			ns.Write(itemNum);
			ns.Write(dateTime);
			ns.Write(0);
			_ = last;
		}
	}
}
