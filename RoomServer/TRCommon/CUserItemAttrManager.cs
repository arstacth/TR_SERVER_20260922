using System.Collections.Generic;
using LocalCommons.Network;

namespace TRCommon
{
	public class CUserItemAttrManager
	{
		private Dictionary<int, UserItemAttrInfo> m_mapItemAttr;

		private Dictionary<int, UserItemAttrInfo> m_mapCharAttr;

		public Dictionary<int, UserItemAttrInfo> getItemAttrList => m_mapItemAttr;

		public CUserItemAttrManager()
		{
			m_mapItemAttr = new Dictionary<int, UserItemAttrInfo>();
			m_mapCharAttr = new Dictionary<int, UserItemAttrInfo>();
		}

		public CUserItemAttrManager(CUserItemAttrManager clone)
		{
			m_mapItemAttr = new Dictionary<int, UserItemAttrInfo>(clone.m_mapItemAttr);
			m_mapCharAttr = new Dictionary<int, UserItemAttrInfo>(clone.m_mapCharAttr);
		}

		public void reset()
		{
			m_mapItemAttr.Clear();
			m_mapCharAttr.Clear();
		}

		public void clear()
		{
			m_mapItemAttr.Clear();
		}

		public void clearChar()
		{
			m_mapCharAttr.Clear();
		}

		public void encodeUserItemAttr(PacketWriter encoder)
		{
			encoder.Write(m_mapItemAttr.Count);
			foreach (KeyValuePair<int, UserItemAttrInfo> item in m_mapItemAttr)
			{
				encoder.Write(item.Key);
				encoder.Write(item.Value.m_ItemAttr.Count);
				foreach (KeyValuePair<short, float> item2 in item.Value.m_ItemAttr)
				{
					encoder.Write(item2.Key);
					encoder.Write(item2.Value);
				}
			}
		}

		public void decodeUserItemAttr(PacketReader decoder)
		{
			int num = decoder.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo();
				userItemAttrInfo.m_iItemDescNum = decoder.ReadLEInt32();
				int num2 = decoder.ReadLEInt32();
				for (int j = 0; j < num2; j++)
				{
					short key = decoder.ReadLEInt16();
					float value = decoder.ReadLESingle();
					userItemAttrInfo.m_ItemAttr[key] = value;
				}
				insertItemAttr(userItemAttrInfo);
			}
		}

		public void encodeUserCharAttr(PacketWriter encoder)
		{
			encoder.Write(m_mapCharAttr.Count);
			foreach (KeyValuePair<int, UserItemAttrInfo> item in m_mapCharAttr)
			{
				encoder.Write(item.Key);
				encoder.Write(item.Value.m_ItemAttr.Count);
				foreach (KeyValuePair<short, float> item2 in item.Value.m_ItemAttr)
				{
					encoder.Write(item2.Key);
					encoder.Write(item2.Value);
				}
			}
		}

		public void decodeUserCharAttr(PacketReader decoder)
		{
			int num = decoder.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo();
				userItemAttrInfo.m_iItemDescNum = decoder.ReadLEInt32();
				int num2 = decoder.ReadLEInt32();
				for (int j = 0; j < num2; j++)
				{
					short key = decoder.ReadLEInt16();
					float value = decoder.ReadLESingle();
					if (!userItemAttrInfo.m_ItemAttr.ContainsKey(key))
					{
						userItemAttrInfo.m_ItemAttr.Add(key, value);
					}
				}
				insertCharAttr(userItemAttrInfo);
			}
		}

		public bool getItemAttr(int iItemDescNum, out CItemAttr rAttr)
		{
			rAttr = new CItemAttr();
			if (m_mapItemAttr.TryGetValue(iItemDescNum, out var value))
			{
				UserItemAttrInfo src = value;
				return convertTo(src, out rAttr);
			}
			return false;
		}

		public bool getItemAttr(int iItemDescNum, out Dictionary<short, float> rOut)
		{
			rOut = new Dictionary<short, float>();
			if (m_mapItemAttr.TryGetValue(iItemDescNum, out var value))
			{
				UserItemAttrInfo src = value;
				return convertTo(src, out rOut);
			}
			return false;
		}

		public void fromVector(List<UserItemAttrInfo> info)
		{
			m_mapItemAttr.Clear();
			foreach (UserItemAttrInfo item in info)
			{
				insertItemAttr(item, allowDupplication: true);
			}
		}

		public void fromMap(Dictionary<int, UserItemAttrInfo> info)
		{
			m_mapItemAttr.Clear();
			m_mapItemAttr = info;
		}

		public bool deleteItemAttr(int iItemDescNum)
		{
			return m_mapItemAttr.Remove(iItemDescNum);
		}

		public void deleteItemAttr(List<int> items)
		{
			foreach (int item in items)
			{
				m_mapItemAttr.Remove(item);
			}
		}

		public bool insertItemAttr(UserItemAttrInfo itemInfo, bool allowDupplication = false)
		{
			if (!allowDupplication)
			{
				m_mapItemAttr[itemInfo.m_iItemDescNum] = itemInfo;
			}
			else
			{
				m_mapItemAttr[itemInfo.m_iItemDescNum] += itemInfo;
			}
			return true;
		}

		public bool addUpItemAttr(int itemDescNum, Dictionary<short, float> attributes)
		{
			foreach (KeyValuePair<short, float> attribute in attributes)
			{
				short key = attribute.Key;
				float value = attribute.Value;
				if (!m_mapItemAttr.ContainsKey(itemDescNum))
				{
					m_mapItemAttr.Add(itemDescNum, new UserItemAttrInfo());
				}
				m_mapItemAttr[itemDescNum].m_iItemDescNum = itemDescNum;
				if (!m_mapItemAttr[itemDescNum].m_ItemAttr.ContainsKey(key))
				{
					m_mapItemAttr[itemDescNum].m_ItemAttr.Add(key, value);
				}
				else
				{
					m_mapItemAttr[itemDescNum].m_ItemAttr[key] += value;
				}
			}
			return true;
		}

		public bool subtractItemAttr(int itemDescNum, Dictionary<short, float> attributes)
		{
			foreach (KeyValuePair<short, float> attribute in attributes)
			{
				short key = attribute.Key;
				float value = attribute.Value;
				if (m_mapItemAttr[itemDescNum].m_ItemAttr[key] <= value)
				{
					m_mapItemAttr[itemDescNum].m_ItemAttr.Remove(key);
				}
				else
				{
					m_mapItemAttr[itemDescNum].m_ItemAttr[key] -= value;
				}
			}
			return true;
		}

		public List<float> getAttrValueByAttrType(List<NetItemInfo> itemList, eItemAttr attrType)
		{
			List<float> list = new List<float>();
			foreach (NetItemInfo item in itemList)
			{
				if (getItemAttr(item.m_iItemDescNum, out Dictionary<short, float> rOut) && rOut.ContainsKey((short)attrType))
				{
					list.Add(rOut[(short)attrType]);
				}
				else
				{
					list.Add(0f);
				}
			}
			return list;
		}

		public List<int> getItemListByAttrType(eItemAttr attrType)
		{
			List<int> list = new List<int>();
			foreach (KeyValuePair<int, UserItemAttrInfo> item in m_mapItemAttr)
			{
				UserItemAttrInfo value = item.Value;
				if (value.m_ItemAttr.ContainsKey((short)attrType))
				{
					list.Add(value.m_iItemDescNum);
				}
			}
			return list;
		}

		public void dbgDisplayAllAbilities()
		{
		}

		public bool getCharAttr(int iItemDescNum, out CItemAttr rAttr)
		{
			rAttr = new CItemAttr();
			if (m_mapCharAttr.TryGetValue(iItemDescNum, out var value))
			{
				return convertTo(value, out rAttr);
			}
			return false;
		}

		public bool getCharAttr(int iItemDescNum, out Dictionary<short, float> rOut)
		{
			rOut = new Dictionary<short, float>();
			if (m_mapCharAttr.TryGetValue(iItemDescNum, out var value))
			{
				return convertTo(value, out rOut);
			}
			return false;
		}

		public void fromCharMap(Dictionary<int, UserItemAttrInfo> info)
		{
			m_mapCharAttr.Clear();
			m_mapCharAttr = info;
		}

		public bool insertCharAttr(UserItemAttrInfo itemInfo, bool allowDupplication = false)
		{
			if (!allowDupplication)
			{
				m_mapCharAttr[itemInfo.m_iItemDescNum] = itemInfo;
			}
			else
			{
				m_mapCharAttr[itemInfo.m_iItemDescNum] += itemInfo;
			}
			return true;
		}

		private bool convertTo(UserItemAttrInfo src, out Dictionary<short, float> dest)
		{
			dest = new Dictionary<short, float>(src.m_ItemAttr);
			return true;
		}

		private bool convertTo(UserItemAttrInfo src, out CItemAttr rAttr)
		{
			rAttr = new CItemAttr();
			rAttr.m_attr.m_mapAttr = new Dictionary<short, float>(src.m_ItemAttr);
			return true;
		}
	}
}
