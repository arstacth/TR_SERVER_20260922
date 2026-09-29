using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ServerSettingChange : NetPacket
	{
		public ServerSettingChange(string key, string value, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_SERVERSETTING_INFO_CHANGED_ACK);
			ns.Write(1);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(key);
			ns.WriteAnsiFixed_intSize(value);
			_ = last;
		}
	}
}
