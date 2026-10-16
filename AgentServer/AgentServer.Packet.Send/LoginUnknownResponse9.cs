using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class LoginUnknownResponse9 : NetPacket
	{
		public LoginUnknownResponse9(byte last)
		{
			ns.WriteOP(Opcodes.eServer_QUEST_EVENT_INFO_NOTIFY);
			ns.Write((byte)1);
			ns.WriteHex("09000000800E0300810E0300820E0300CB0E0300CC0E0300CD0E0300CE0E0300D10E0300D20E0300");
			_ = last;
		}
	}
}
