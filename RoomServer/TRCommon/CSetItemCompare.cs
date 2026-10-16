using System;
using System.Collections;
using System.Collections.Generic;

namespace TRCommon
{
	public class CSetItemCompare
	{
		private BitArray m_bits;

		private BitArray m_chunk;

		public int getBitsCount => m_bits.Count;

		public CSetItemCompare(int element_count)
		{
			m_bits = new BitArray(element_count);
			m_chunk = new BitArray(element_count);
		}

		public void reset()
		{
			m_bits.SetAll(value: false);
			m_chunk.SetAll(value: false);
		}

		public void set(int idx)
		{
			if (0 <= idx && idx < m_bits.Count)
			{
				m_bits.Set(idx, value: true);
			}
		}

		public bool parseComplexKeys(out List<int> complexKeys)
		{
			m_chunk.SetAll(value: false);
			complexKeys = new List<int>();
			for (int i = 0; i < m_bits.Count; i++)
			{
				if (!m_bits[i])
				{
					continue;
				}
				m_chunk.SetAll(value: false);
				if (m_chunk[i])
				{
					m_chunk[i] = false;
				}
				else
				{
					m_chunk[i] = true;
				}
				for (int j = i + 1; j < m_bits.Count; j++)
				{
					if (!m_bits[j])
					{
						continue;
					}
					if (m_chunk[j])
					{
						m_chunk[j] = false;
					}
					else
					{
						m_chunk[j] = true;
					}
					if (2 <= m_chunk.Count)
					{
						complexKeys.Add((int)BitArrayToU64(m_chunk));
						if (m_chunk[j])
						{
							m_chunk[j] = false;
						}
						else
						{
							m_chunk[j] = true;
						}
					}
				}
			}
			return true;
		}

		private ulong BitArrayToU64(BitArray ba)
		{
			int num = Math.Min(64, ba.Count);
			ulong num2 = 0uL;
			for (int i = 0; i < num; i++)
			{
				if (ba.Get(i))
				{
					num2 |= (ulong)(1L << i);
				}
			}
			return num2;
		}
	}
}
