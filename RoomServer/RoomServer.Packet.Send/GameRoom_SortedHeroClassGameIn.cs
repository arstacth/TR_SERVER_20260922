using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_SortedHeroClassGameIndices : NetPacket
	{
		public GameRoom_SortedHeroClassGameIndices(int type, byte topPos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_HERO_CLASS_SORTED_GAME_INDICES_NOTIFY);
			ns.Write(type);
			if (type == 1)
			{
				ns.Write(topPos);
			}
			_ = last;
		}
	}
}
