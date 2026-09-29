using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MachineGiveItem : NetPacket
	{
		public MachineGiveItem(Account User, int ResultItemNum, bool isGift, string NickName, byte last)
		{
			ns.WriteOP(Opcodes.eServer_CAPSULE_MACHINE_GIVE_ACK);
			ns.Write(isGift);
			ns.Write(ResultItemNum);
			if (isGift)
			{
				ns.WriteAnsiFixed_intSize(NickName);
			}
			_ = last;
		}
	}
}
