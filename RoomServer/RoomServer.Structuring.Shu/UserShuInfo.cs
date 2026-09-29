using System.Collections.Generic;
using System.Linq;
using TRCommon;

namespace RoomServer.Structuring.Shu
{
	public class UserShuInfo
	{
		public string ShuName;

		public int ShuItemNum;

		public List<short> ShuAvatarKind = new List<short>(6) { 0, 0, 0, 0, 0, 0 };

		public List<int> Statusinfo = new List<int>(4) { 0, 0, 0, 0 };

		public long MotionList;

		public void updateavatar(List<ShuAvatarInfo> avatarinfos)
		{
			foreach (ShuAvatarInfo item in avatarinfos.OrderBy((ShuAvatarInfo o) => o.Position))
			{
				if (ShopItemTable.isShuItem(item.avatarItemNum))
				{
					cpk_type shuItemKind = ShopItemTable.getShuItemKind(item.avatarItemNum);
					cpk_type shuItemCharacter = ShopItemTable.getShuItemCharacter(item.avatarItemNum);
					ShuAvatarKind[item.Position] = ((item.Position == 0) ? shuItemCharacter : shuItemKind);
				}
				else
				{
					ShuAvatarKind[item.Position] = 0;
				}
			}
		}

		public void updatecharinfo(ShuCharInfo charinfo)
		{
			ShuName = charinfo.Name;
			ShuItemNum = charinfo.avatarItemNum;
			MotionList = charinfo.MotionList;
		}

		public void updatestatusinfo(List<ShuStatusInfo> statusinfo)
		{
			foreach (ShuStatusInfo item in statusinfo)
			{
				Statusinfo[item.statustype] = item.value;
			}
		}

		public void UpdateEachstatusinfo(List<ShuStatusInfo> statusinfo)
		{
			foreach (ShuStatusInfo item in statusinfo)
			{
				Statusinfo[item.statustype] = item.value;
			}
		}
	}
}
