using AgentServer.Structuring;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon.Protocol;

namespace AgentServer.Packet.RoomServer
{
	public sealed class eRoom_UPDATE_AVATAR_INFO : NetPacket
	{
		public eRoom_UPDATE_AVATAR_INFO(Account User, bool bSaveCharInPark, byte last)
		{
			ns.WriteOP(eRoomAgentProtocol.eRoomAgentProtocol_WRAP_ROOM_REQ);
			ns.WriteOP(RoomOpcodes.eRoomProtocol_WRAP);
			ns.WriteOP(RoomOpcodes.eRoom_UPDATE_AVATAR_INFO);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			for (byte b = 0; b < 15; b = (byte)(b + 1))
			{
				ns.Write(User.advancedAvatarInfo.m_realAvatarInfo.GetWear(b));
			}
			for (int i = 0; i < 12; i++)
			{
				ns.Write(User.AvatarItemDyeing[i].DyeingPart);
				ns.Write(User.AvatarItemDyeing[i].Color1, 0, 3);
				ns.Write(User.AvatarItemDyeing[i].Color2, 0, 3);
				ns.Write(User.AvatarItemDyeing[i].Color3, 0, 3);
			}
			for (byte b2 = 0; b2 < 15; b2 = (byte)(b2 + 1))
			{
				ns.Write(User.advancedAvatarInfo.m_costumeAvatarInfo.GetWear(b2));
			}
			for (int j = 12; j < 24; j++)
			{
				ns.Write(User.AvatarItemDyeing[j].DyeingPart);
				ns.Write(User.AvatarItemDyeing[j].Color1, 0, 3);
				ns.Write(User.AvatarItemDyeing[j].Color2, 0, 3);
				ns.Write(User.AvatarItemDyeing[j].Color3, 0, 3);
			}
			ns.Write(User.advancedAvatarInfo.isUseCostume);
			ns.Write(value: false);
			User.activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			ns.Write(bSaveCharInPark);
			ns.Write(last);
		}
	}
}
