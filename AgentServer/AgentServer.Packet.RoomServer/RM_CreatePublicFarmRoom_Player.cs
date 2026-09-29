using AgentServer.Structuring;
using AgentServer.Structuring.Room;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_CreatePublicFarmRoom_PlayerInfo : NetPacket
	{
		public RM_CreatePublicFarmRoom_PlayerInfo(Account User, RoomSettings roomsetting, int FarmUniqueNum, byte last)
		{
			ns.WriteOP(RMProtocol.RM_EnterPublicFarm_PlayerInfo_REQ);
			ns.Write(User.Session);
			ns.Write(ServerStatus.MyAgentID);
			ns.Write(User.UserNum);
			ns.WriteAnsiFixed_intSize(User.NickName);
			ns.WriteAnsiFixed_intSize(User.UserID);
			ns.Write(User.UDPInfo, 0, 48);
			ns.WriteAnsiFixed_intSize(User.LastIp);
			if (User.GuildNum > 0)
			{
				ns.WriteAnsiFixed_intSize(User.GuildInfo.guildName);
				ns.Write((long)User.GuildNum);
				ns.Write((short)User.GuildInfo.level);
				ns.Write((short)User.GuildUserInfo.grade);
			}
			else
			{
				ns.Fill(14);
			}
			ns.Write((byte)100);
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
			ns.Write((short)0);
			ns.Write(User.Attribute);
			ns.Write(User.TR);
			ns.Write(User.Exp);
			ns.Write((float)User.Luck);
			ns.Write(User.CoupleInfo.CoupleNum);
			ns.Write(User.CoupleInfo.CoupleType);
			ns.WriteAnsiFixed_intSize(User.CoupleInfo.MateName);
			ns.Write(User.CoupleInfo.CreateTime);
			ns.Write(User.CoupleInfo.MarriedTime);
			ns.Write(User.CoupleInfo.RingChangedTime);
			ns.Write(User.CoupleInfo.CoupleRingNum);
			ns.Write(User.CoupleInfo.MaxRingDays);
			ns.Write(User.CoupleInfo.CoupleLevel);
			ns.Write((short)0);
			ns.Write(User.CoupleInfo.CondDays);
			ns.Write(User.CoupleInfo.AccumulateExp);
			ns.Write(User.CoupleInfo.CoupleRank);
			ns.Write(User.CoupleInfo.CouplePoint);
			ns.Write(0);
			ns.Write((byte)0);
			ns.Write(value: false);
			User.activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			User.encodeUserItemAttr(ns);
			User.encodeUserCharAttr(ns);
			if (roomsetting.RoomKindID == 75)
			{
				ns.Write(User.MyFarmUniqueNum);
				ns.Write(User.MyFarmInfo.FarmWeatherTypeNum);
				ns.Write(User.MyFarmInfo.FarmSkyTypeNum);
				ns.Write(User.MyFarmInfo.FarmTypeNum);
				ns.WriteAnsiFixed_intSize(User.MyFarmInfo.FarmName);
				ns.WriteAnsiFixed_intSize(User.MyFarmInfo.MasterName);
				ns.Write(User.MyFarmInfo.ExpireTime);
				ns.Write(User.MyFarmInfo.CreateTime);
				ns.Write((byte)1);
				ns.Write(User.MyFarmInfo.isPublic);
				ns.Write(User.MyFarmInfo.TotalCount);
				ns.Write(User.MyFarmInfo.TodaysVisitorCount);
				ns.Write(User.MyFarmInfo.PremiumFarmUsing);
				ns.Write(User.MyFarmInfo.PremiumFarmExpireDateTime);
				ns.Write(User.MyFarmInfo.farmExp);
				ns.Write(0);
				ns.Write((byte)0);
			}
			else
			{
				ns.Fill(64);
			}
			ns.Write((int)User.PartyType);
			ns.Write(User.SubPartyType);
			bool flag = User.CurrentShuID != -1;
			ns.Write(flag);
			if (flag)
			{
				ns.WriteAnsiFixed_intSize(User.UserShuInfo.ShuName);
				ns.Write(User.UserShuInfo.ShuItemNum);
				ns.Write(User.UserShuInfo.Statusinfo[3]);
				ns.Write(1);
				ns.Write((short)12);
				foreach (short item in User.UserShuInfo.ShuAvatarKind)
				{
					ns.Write(item);
				}
				ns.Write((short)16);
				foreach (int item2 in User.UserShuInfo.Statusinfo)
				{
					ns.Write(item2);
				}
				ns.Write(User.UserShuInfo.MotionList);
			}
			ns.Write(User.TopRank);
			User.avatarLock.encode(ns);
			ns.Write(FarmUniqueNum);
			ns.Write(roomsetting.RoomKindID);
			ns.WriteAnsiFixed_intSize(roomsetting.Name);
			ns.WriteAnsiFixed_intSize(roomsetting.Password);
			ns.Write(roomsetting.IsTeamPlay);
			ns.Write(roomsetting.IsStepOn);
			ns.Write(roomsetting.ItemType);
			ns.Write(roomsetting.MapNum);
			ns.Write(roomsetting.FarmRoomInfo.Type);
			ns.Write(roomsetting.FarmRoomInfo.MaxUserLimit);
			ns.Write(roomsetting.FarmRoomInfo.unk1);
			ns.WriteAnsiFixed_intSize(roomsetting.FarmRoomInfo.FarmName);
			ns.Write(last);
		}
	}
}
