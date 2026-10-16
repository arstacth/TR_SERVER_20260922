using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PlayerFishedItem : NetPacket
	{
		public PlayerFishedItem(Account User, int itemid, int size, bool isFarmReward, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_FISHING_CATCH_FISH_NOTIFY);
			ns.Write(User.RoomPos);
			ns.Write((byte)1);
			ns.Write(itemid);
			ns.Write(size);
			ns.Write(isFarmReward);
			_ = last;
		}
	}
}
