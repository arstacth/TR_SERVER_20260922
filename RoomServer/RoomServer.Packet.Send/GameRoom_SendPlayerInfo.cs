using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Item;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.Shu;
using RoomServer.Structuring.User;
using TRCommon;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_SendPlayerInfo : NetPacket
	{
		public GameRoom_SendPlayerInfo(Account User, int RoomKindID, byte last)
		{
			_ = last;
			if (User == null)
			{
				return;
			}
			if (User.advancedAvatarInfo is null)
			{
				User.advancedAvatarInfo = new AdvancedAvatarInfo();
			}
			User.advancedAvatarInfo.EnsureVisibleStarterClothes();
			if (User.CoupleInfo == null)
			{
				User.CoupleInfo = new UserCoupleInfo();
			}
			if (User.activeItem == null)
			{
				User.activeItem = new CActiveItems();
			}
			if (User.userItemAttr == null)
			{
				User.userItemAttr = new CUserItemAttrManager();
			}
			if (User.avatarLock == null)
			{
				User.avatarLock = new CAvatarLock();
			}
			if (User.MyFarmInfo == null)
			{
				User.MyFarmInfo = new MyFarmInfo();
			}
			if (User.UserShuInfo == null)
			{
				User.UserShuInfo = new UserShuInfo();
			}
			ns.WriteOP(Opcodes.eServer_NEW_ROOM_USER_ACK);
			ns.Write(User.Session);
			ns.Write(User.RoomPos);
			if (User.UDPInfo != null && User.UDPInfo.Length >= 48)
			{
				ns.Write(User.UDPInfo, 0, 48);
			}
			else
			{
				ns.Fill(48);
			}
			ns.WriteAnsiFixed_intSize(User.NickName ?? string.Empty);
			if (User.GuildNum > 0 && User.GuildInfo != null)
			{
				ns.WriteAnsiFixed_intSize(User.GuildInfo.guildName ?? string.Empty);
				ns.Write((long)User.GuildNum);
				ns.Write((short)User.GuildInfo.level);
				ns.Write((short)(User.GuildUserInfo != null ? User.GuildUserInfo.grade : 0));
			}
			else
			{
				ns.WriteAnsiFixed_intSize(string.Empty);
				ns.Write(0L);
				ns.Write((short)0);
				ns.Write((short)0);
			}
			int lv = User.Level;
			if (lv < 1)
			{
				lv = 1;
			}
			if (lv > 255)
			{
				lv = 255;
			}
			ns.Write((byte)lv);
			ns.Write(User.IsReady);
			// CAvatarInfo half is 0xB0: 15 parts + 7 acc + 1 EF + 13 dye slots.
			// KR Fill(16)+12 dyes is 20 bytes short; packed then overpops NEW_ROOM_USER.
			WriteAvatarHalf(User.advancedAvatarInfo.m_realAvatarInfo, User, 0);
			WriteAvatarHalf(User.advancedAvatarInfo.m_costumeAvatarInfo, User, 12);
			ns.Write(User.advancedAvatarInfo.isUseCostume);
			// Packed NEW_ROOM_USER (trRelease 0x82F660): after 353-byte avatar,
			// ushort, int64 exp, team/relay/pos, couple, flag, THEN items/farm.
			// KR items-before-exp made 20:33 Overpop 671 672 4; EOF ints
			// only moved that hole (668→672→676).
			ns.Write((short)0);
			ns.Write(User.Exp);
			ns.Write(User.Team);
			ns.Write(User.RelayTeamPos);
			ns.Write(User.RoomPos);
			ns.Write(User.CoupleInfo.CoupleNum);
			ns.Fill(16);
			ns.Write(User.CoupleInfo.CoupleType);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(User.CoupleInfo.MateName);
			ns.Write(User.CoupleInfo.CreateTime);
			ns.Write(User.CoupleInfo.MarriedTime);
			ns.Write(User.CoupleInfo.RingChangedTime);
			ns.Write(User.CoupleInfo.CoupleRingNum);
			ns.Write(User.CoupleInfo.MaxRingDays);
			ns.Write(User.CoupleInfo.CoupleLevel);
			ns.Write((short)0);
			ns.Write(User.CoupleInfo.CondDays);
			ns.Write((long)User.CoupleInfo.AccumulateExp);
			ns.Write(User.CoupleInfo.CoupleRank);
			ns.Write(User.CoupleInfo.CouplePoint);
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
			ns.Write((byte)0);
			User.activeItem.encodeActiveItemsForRoom(ns, User.advancedAvatarInfo, User.avatarLock);
			User.userItemAttr.encodeUserItemAttr(ns);
			User.userItemAttr.encodeUserCharAttr(ns);
			WritePackedFarm(User, RoomKindID);
			ns.Write((byte)0);
			ns.Write((short)User.PartyType);
			ns.Write((short)User.SubPartyType);
			ns.Write(User.UseShu);
			if (User.UseShu)
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
			ns.Write(0);
			ns.Write((byte)0);
			// No last and no extra int — last alone Overpop+4; pad int left RemainSize=4.
		}

		private void WritePackedFarm(Account User, int RoomKindID)
		{
			if (RoomKindID == 75)
			{
				ns.Write(Utility.PackFarmUnique(User.MyFarmUniqueNum));
				ns.Write(0);
				ns.Write(0);
				ns.Write(0);
				ns.WriteAnsiFixed_intSize(User.MyFarmInfo.FarmName);
				ns.WriteAnsiFixed_intSize(User.MyFarmInfo.MasterName);
				ns.Write(User.MyFarmInfo.ExpireTime);
				ns.Write(User.MyFarmInfo.CreateTime);
				int cur = User.CurrentFarmUniqueNum > 0 ? User.CurrentFarmUniqueNum : User.MyFarmUniqueNum;
				bool ownFarm = User.MyFarmUniqueNum > 0 && User.MyFarmUniqueNum == cur;
				ns.Write((byte)1);
				ns.Write(User.MyFarmInfo.isPublic);
				ns.Write((byte)(ownFarm ? 1 : 0));
				ns.Write(User.MyFarmInfo.TotalCount);
				ns.Write(User.MyFarmInfo.TodaysVisitorCount);
				ns.Write((byte)(ownFarm ? 1 : 0));
				ns.Fill(2);
				ns.Write(User.MyFarmInfo.PremiumFarmUsing);
				ns.Write(User.MyFarmInfo.PremiumFarmExpireDateTime > 0L
					? User.MyFarmInfo.PremiumFarmExpireDateTime
					: 1842465389770955L);
				ns.Write(Utility.FarmHudExp(User.MyFarmInfo.farmExp));
				ns.Write(0);
				return;
			}
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.Write(0L);
			ns.Write(0L);
			// Park/lobby NEW_ROOM stub — keep exact pre-farm layout (byte ownFarm + long farmExp).
			// Changing this caused Overpop 689 and blocked park enter.
			ns.Write((byte)1);
			ns.Write(value: false);
			ns.Write((byte)0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0L);
			ns.Write(0L);
		}

		private void WriteAvatarHalf(AvatarInfo info, Account user, int dyeStart)
		{
			for (int i = 0; i < 15; i++)
			{
				ns.Write(info.GetWear(i));
			}
			for (int i = 0; i < 7; i++)
			{
				ns.Write(info.GetAcc(i));
			}
			ns.Write(info.GetEF());
			for (int i = 0; i < 12; i++)
			{
				UserItemDyeing dye = null;
				int idx = dyeStart + i;
				if (user != null && user.AvatarItemDyeing != null && idx >= 0 && idx < user.AvatarItemDyeing.Count)
				{
					dye = user.AvatarItemDyeing[idx];
				}
				if (dye == null)
				{
					dye = new UserItemDyeing();
				}
				ns.Write(dye.DyeingPart);
				byte[] c1 = dye.Color1 ?? new byte[3];
				byte[] c2 = dye.Color2 ?? new byte[3];
				byte[] c3 = dye.Color3 ?? new byte[3];
				ns.Write(c1, 0, 3);
				ns.Write(c2, 0, 3);
				ns.Write(c3, 0, 3);
			}
			ns.Fill(10);
		}
	}
}
