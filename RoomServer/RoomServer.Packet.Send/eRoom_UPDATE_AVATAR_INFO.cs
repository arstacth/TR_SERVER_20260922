using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Item;
using RoomServer.Structuring.Opcode;
using TRCommon;

namespace RoomServer.Packet.Send
{
	public sealed class eRoom_UPDATE_AVATAR_INFO : NetPacket
	{
		public eRoom_UPDATE_AVATAR_INFO(Account User, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_UPDATE_AVATAR_INFO);
			ns.Write(User.RoomPos);
			WriteAvatarHalf(User.advancedAvatarInfo.m_realAvatarInfo, User, 0);
			WriteAvatarHalf(User.advancedAvatarInfo.m_costumeAvatarInfo, User, 12);
			ns.Write(User.advancedAvatarInfo.isUseCostume);
			ns.Write(value: false);
			User.userItemAttr.encodeUserItemAttr(ns);
			User.userItemAttr.encodeUserCharAttr(ns);
			// RemainSize=2 with two ints — trailer is int + short.
			ns.Write(0);
			ns.Write((short)0);
			_ = last;
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
