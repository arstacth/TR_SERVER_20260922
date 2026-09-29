using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginCheck : NetPacket
	{
		public LoginCheck(string UserID, bool LoginCheckOK)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_AUTH_ACK);
			if (LoginCheckOK)
			{
				ns.Write(0);
			}
			else
			{
				ns.Write(13);
			}
			ns.Write(1);
			ns.WriteAnsiFixed_intSize("strTangID");
			ns.WriteAnsiFixed_intSize(UserID);
			if (LoginCheckOK)
			{
				ns.Write(0);
				ns.Write(9);
				byte[] buffer = new byte[9] { 204, 128, 196, 148, 246, 186, 140, 18, 204 };
				ns.Write(buffer, 0, 9);
			}
			else
			{
				ns.Write(1);
			}
		}
	}
}
