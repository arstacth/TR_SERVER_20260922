using System.Collections.Generic;

namespace TRCommon
{
	public static class SetItemAttrTable
	{
		private static Dictionary<int, Dictionary<int, Dictionary<short, float>>> m_setItemAttrTable = new Dictionary<int, Dictionary<int, Dictionary<short, float>>>();

		public static List<KeyValuePair<short, float>> getSetItemAttrList(int groupItemDescNum, List<int> complexKeys)
		{
			List<KeyValuePair<short, float>> list = new List<KeyValuePair<short, float>>();
			if (m_setItemAttrTable.TryGetValue(groupItemDescNum, out var value))
			{
				foreach (int complexKey in complexKeys)
				{
					if (value.TryGetValue(complexKey, out var value2))
					{
						foreach (KeyValuePair<short, float> item in value2)
						{
							list.Add(item);
						}
					}
				}
				return list;
			}
			return list;
		}

		public static int reloadSetItemAttrTable(Dictionary<int, Dictionary<int, Dictionary<short, float>>> setItemAttrTable)
		{
			m_setItemAttrTable = setItemAttrTable;
			return m_setItemAttrTable.Count;
		}

		public static CItemAttr getSpecificItemAttributes(int groupItemDescNum, int complexKey)
		{
			CItemAttr cItemAttr = new CItemAttr();
			if (m_setItemAttrTable.TryGetValue(groupItemDescNum, out var value) && value.TryGetValue(complexKey, out var value2))
			{
				foreach (KeyValuePair<short, float> item in value2)
				{
					cItemAttr.m_attr[item.Key] += item.Value;
				}
				return cItemAttr;
			}
			return cItemAttr;
		}

		public static Dictionary<int, Dictionary<short, float>> getSetItemAttrListByGroupItemDescNum(int groupItemDescNum)
		{
			Dictionary<int, Dictionary<short, float>> result = new Dictionary<int, Dictionary<short, float>>();
			if (m_setItemAttrTable.TryGetValue(groupItemDescNum, out var value))
			{
				result = value;
			}
			return result;
		}
	}
}
