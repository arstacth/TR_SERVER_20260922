using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse5 : NetPacket
	{
		public LoginUnknownResponse5(byte last)
		{
			ns.WriteOP(Opcodes.DEPRECATED__eServer_PROFILE_GET_ACK);
			ns.Write((byte)1);
			ns.Write((byte)1);
			ns.Write((byte)1);
			ns.Write((byte)1);
			ns.Write((byte)1);
			ns.Write((byte)1);
			ns.Fill(9);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(string.Empty);
			_ = last;
		}
	}
}
