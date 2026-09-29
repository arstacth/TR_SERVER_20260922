using System.Collections.Generic;

namespace RoomServer.Structuring.Item
{
	public class ExtraAbilityItemAttrInfo
	{
		public int itemnum;

		public int limit;

		public long gottime;

		public AvatarItemInfo ItemInfo;

		public List<ItemAttr> attrlist = new List<ItemAttr>();
	}
}
