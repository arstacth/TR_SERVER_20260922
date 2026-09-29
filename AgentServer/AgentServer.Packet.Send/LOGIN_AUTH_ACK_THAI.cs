using AgentServer;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using Serilog;

namespace AgentServer.Packet.Send
{
	public sealed class LOGIN_AUTH_ACK_THAI : NetPacket
	{
		public LOGIN_AUTH_ACK_THAI(string UserID, bool LoginCheckOK)
		{
			ns.WriteOP(Opcodes.eServer_LOGIN_AUTH_ACK);
			ns.Write(LoginCheckOK ? 0 : 13);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize("strTangID");
			ns.WriteAnsiFixed_intSize(UserID ?? string.Empty);
			ns.Write(0);
			if (Conf.ProtocolDebug)
			{
				Log.Information("LOGIN_AUTH_ACK ok:{0} user:{1} ns:{2}", LoginCheckOK, UserID, ns.Length);
			}
		}
	}
}
