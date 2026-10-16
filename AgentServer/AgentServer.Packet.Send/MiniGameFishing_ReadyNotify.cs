using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MiniGameFishing_ReadyNotify : NetPacket
	{
		public MiniGameFishing_ReadyNotify(byte roompos, int fisheditem, int size, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FISHING_START_MINIGAME_NOTIFY);
			ns.Write(roompos);
			ns.Write(fisheditem);
			ns.Write(size);
			_ = last;
		}
	}
}
