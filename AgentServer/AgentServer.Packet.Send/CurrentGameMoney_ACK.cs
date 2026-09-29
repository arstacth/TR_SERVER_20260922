using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CurrentGameMoney_ACK : NetPacket
	{
		public CurrentGameMoney_ACK(Account User, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHOP_CURRENT_TR);
			ns.Write(User.TR);
			_ = last;
		}
	}
}
