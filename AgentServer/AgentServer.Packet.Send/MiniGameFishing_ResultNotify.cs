using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MiniGameFishing_ResultNotify : NetPacket
	{
		public MiniGameFishing_ResultNotify(byte roompos, bool success, int resultitem, int resultsize, bool farmmasterreward, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FISHING_MINIGAME_RESULT_NOTIFY);
			ns.Write(roompos);
			ns.Write(success);
			ns.Write(resultitem);
			ns.Write(resultsize);
			ns.Write(farmmasterreward);
			_ = last;
		}
	}
}
