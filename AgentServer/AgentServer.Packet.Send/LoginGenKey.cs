using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginGenKey : NetPacket
	{
		public LoginGenKey(byte[] key)
		{
			ns.WriteOP(Opcodes.eServer_ECC_SESSIONKEY_EXCHANGE_ACK);
			ns.Write(key, 0, key.Length);
			ns.Write((byte)1);
		}
	}
}
