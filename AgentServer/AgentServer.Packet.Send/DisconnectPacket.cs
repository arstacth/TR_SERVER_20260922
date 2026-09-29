using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DisconnectPacket : NetPacket
	{
		public DisconnectPacket(int msgid, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DISCONNECT_FROM_SERVER_ACK);
			ns.Write(msgid);
			_ = last;
		}
	}

	public sealed class PackedDisconnectFromServerAck : NetPacket
	{
		public PackedDisconnectFromServerAck(byte last)
		{
			ns.WriteOP(Opcodes.eServer_DISCONNECT_FROM_SERVER_ACK);
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
