using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;
using TRCommon;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_PlayerEnterRoom : NetPacket
	{
		public RM_PlayerEnterRoom(Account User, string pw, int roomid, int roomkind, int FarmUniqueNum, int guildmatchroomid, byte last)
		{
			if (User.advancedAvatarInfo is null)
			{
				User.advancedAvatarInfo = new AdvancedAvatarInfo();
			}
			User.EnsureAvatarDyeingSlots();
			User.advancedAvatarInfo.EnsureVisibleStarterClothes();
			ns.WriteOP(RMProtocol.RM_UserEnterRoom_REQ);
			ns.Write(User.Session);
			ns.Write(ServerStatus.MyAgentID);
			ns.Write(User.UserNum);
			ns.WriteAnsiFixed_intSize(User.NickName ?? string.Empty);
			ns.WriteAnsiFixed_intSize(User.UserID ?? string.Empty);
			if (User.UDPInfo != null && User.UDPInfo.Length >= 48)
			{
				ns.Write(User.UDPInfo, 0, 48);
			}
			else
			{
				ns.Fill(48);
			}
			ns.WriteAnsiFixed_intSize(User.LastIp ?? string.Empty);
			if (User.GuildNum > 0 && User.GuildInfo != null)
			{
				ns.WriteAnsiFixed_intSize(User.GuildInfo.guildName ?? string.Empty);
				ns.Write((long)User.GuildNum);
				ns.Write((short)User.GuildInfo.level);
				ns.Write((short)(User.GuildUserInfo != null ? User.GuildUserInfo.grade : 0));
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
			if (roomkind == 75)
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
			ns.Write(roomid);
			ns.WriteAnsiFixed_intSize(pw);
			ns.Write(roomkind);
			ns.Write(FarmUniqueNum);
			ns.Write(guildmatchroomid);
			ns.Write(last);
		}
	}
}
