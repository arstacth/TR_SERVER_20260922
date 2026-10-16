using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TRCommon
{
	[Serializable]
	public class CItemAttrNormal : IEnumerable<KeyValuePair<short, float>>, IEnumerable
	{
		public Dictionary<short, float> m_mapAttr;

		public float this[short i]
		{
			get
			{
				if (m_mapAttr.ContainsKey(i))
				{
					return m_mapAttr[i];
				}
				return 0f;
			}
			set
			{
				m_mapAttr[i] = value;
			}
		}

		public bool empty => m_mapAttr.Count == 0;

		public int size => m_mapAttr.Count;

		public CItemAttrNormal()
		{
			m_mapAttr = new Dictionary<short, float>();
			clear();
		}

		public static CItemAttrNormal operator +(CItemAttrNormal a, CItemAttrNormal b)
		{
			if (b.m_mapAttr.Count != 0)
			{
				foreach (KeyValuePair<short, float> item in b)
				{
					if (a.m_mapAttr.ContainsKey(item.Key))
					{
						a.m_mapAttr[item.Key] += item.Value;
					}
					else
					{
						a.m_mapAttr.Add(item.Key, item.Value);
					}
				}
				return a;
			}
			return a;
		}

		public static CItemAttrNormal operator -(CItemAttrNormal a, CItemAttrNormal b)
		{
			if (!b.empty)
			{
				foreach (KeyValuePair<short, float> item in b)
				{
					a.m_mapAttr[item.Key] -= item.Value;
				}
				return a;
			}
			return a;
		}

		public void clear()
		{
			m_mapAttr.Clear();
		}

		public void erase(short key)
		{
			m_mapAttr.Remove(key);
		}

		public bool isExists(short key)
		{
			return m_mapAttr.ContainsKey(key);
		}

		public void applyLimit(Dictionary<short, float> limit)
		{
			foreach (KeyValuePair<short, float> item in limit)
			{
				if (m_mapAttr.ContainsKey(item.Key) && m_mapAttr[item.Key] > item.Value)
				{
					m_mapAttr[item.Key] = item.Value;
				}
			}
		}

		public int count(short key)
		{
			return m_mapAttr.Count((KeyValuePair<short, float> k) => k.Key == key);
		}

		public IEnumerator<KeyValuePair<short, float>> GetEnumerator()
		{
			return m_mapAttr.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
