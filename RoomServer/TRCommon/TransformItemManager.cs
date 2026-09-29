using System.Collections.Generic;

namespace TRCommon
{
	public static class TransformItemManager
	{
		private static Dictionary<(int, int, int), CItemTransformInfo> m_itemTransformMap;

		public static void onRecvItemTransformInfoFromDB(Dictionary<(int, int, int), CItemTransformInfo> itemTransfromMap)
		{
			m_itemTransformMap = itemTransfromMap;
		}

		public static bool getItemTransformInfo((int, int, int) itemQuery, ref CItemTransformInfo itemTransformInfo)
		{
			if (m_itemTransformMap == null)
			{
				return false;
			}
			(int, int, int) key = (0, itemQuery.Item2, itemQuery.Item3);
			if (m_itemTransformMap.TryGetValue(key, out var value))
			{
				itemTransformInfo = value;
				return true;
			}
			if (m_itemTransformMap.TryGetValue(itemQuery, out value))
			{
				itemTransformInfo = value;
				return true;
			}
			return false;
		}

		public static cpk_type getOriginKindNumFromCPK(cpk_type Character, cpk_type Position, cpk_type Kind)
		{
			if (20000 < (int)Kind && 30000 > (int)Kind && 65535 != (int)Kind)
			{
				CItemTransformInfo itemTransformInfo = new CItemTransformInfo();
				if (getItemTransformInfo((Character, Position, Kind), ref itemTransformInfo))
				{
					return itemTransformInfo.m_iOriginKind;
				}
			}
			return Kind;
		}
	}
}
