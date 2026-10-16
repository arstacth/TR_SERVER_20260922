using LocalCommons.Network;
using RoomServer.Packet.Send;

namespace RoomServer.Packet
{
	public class AvatarLockHandle
	{
		public static void Handle_CHANGE_USER_AVATAR_LOCK(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				value.avatarLock.decode(reader);
				value.charAbilityAttrMakeAttr();
				if (value.isInRoom(out var room))
				{
					room.BroadcastToAll(new eRoom_CHANGE_USER_AVATAR_LOCK(value, last));
				}
			}
		}
	}
}
