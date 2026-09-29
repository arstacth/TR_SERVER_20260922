using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MiniGameFishing_Result : NetPacket
	{
		public MiniGameFishing_Result(bool success, int resultitem, int result_size, int decoy, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_MINIGAME_RESULT_NOTIFY);
			ns.Write(0);
			ns.Write(success);
			ns.Write(resultitem);
			ns.Write(result_size);
			ns.Write(1);
			ns.Write(decoy);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}
}
