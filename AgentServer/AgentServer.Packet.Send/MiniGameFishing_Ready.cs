using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MiniGameFishing_Ready : NetPacket
	{
		public MiniGameFishing_Ready(int fisheditem, int size, int decoy, byte randomfish, int minigametype, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FISHING_START_MINIGAME_NOTIFY);
			ns.Write(0);
			ns.Write(fisheditem);
			ns.Write(size);
			ns.Write(1);
			ns.Write(decoy);
			ns.Write(randomfish);
			ns.Write(minigametype);
			_ = last;
		}
	}
}
