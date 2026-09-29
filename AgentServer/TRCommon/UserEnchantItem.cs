using System.Collections.Generic;
using LocalCommons.Network;

namespace TRCommon
{
	public class UserEnchantItem
	{
		public int m_iItemNum;

		public Dictionary<byte, Socket> m_socket = new Dictionary<byte, Socket>();

		public UserEnchantItem()
		{
			clear();
		}

		public void clear()
		{
			m_iItemNum = 0;
			m_socket.Clear();
		}

		public void set(int iItemNum)
		{
			m_iItemNum = iItemNum;
			m_socket.Clear();
		}

		public void append(byte iSeq, byte iSocketNum, int iMountStoneNum, short attr, float fValue)
		{
			if (m_socket.ContainsKey(iSeq))
			{
				if (attr != 0)
				{
					m_socket[iSeq].m_attr[attr] = fValue;
				}
				return;
			}
			Socket socket = new Socket();
			socket.m_iSocketNum = iSocketNum;
			socket.m_iMountStoneNum = iMountStoneNum;
			if (attr != 0)
			{
				socket.m_attr[attr] = fValue;
			}
			m_socket.Add(iSeq, socket);
		}

		public bool mount(byte iSeq, byte iSocketNum, int iMountStoneNum, Dictionary<short, float> attr)
		{
			if (!m_socket.ContainsKey(iSeq))
			{
				m_socket.Add(iSeq, new Socket(iSocketNum, iMountStoneNum, attr));
			}
			else
			{
				m_socket[iSeq] = new Socket(iSocketNum, iMountStoneNum, attr);
			}
			return true;
		}

		public bool remove(byte iSeq)
		{
			if (!m_socket.ContainsKey(iSeq))
			{
				return false;
			}
			m_socket.Remove(iSeq);
			return true;
		}

		public bool removeStone(byte iSeq)
		{
			if (!m_socket.ContainsKey(iSeq))
			{
				return false;
			}
			m_socket[iSeq].removeStone();
			return true;
		}

		public int getItemNum()
		{
			return m_iItemNum;
		}

		public bool getSocketNum(byte iSeq, out byte iSocketNum)
		{
			iSocketNum = 0;
			if (!m_socket.TryGetValue(iSeq, out var value))
			{
				return false;
			}
			iSocketNum = value.m_iSocketNum;
			return true;
		}

		public bool getMountStoneNum(byte iSeq, out int iMountStoneNum)
		{
			iMountStoneNum = 0;
			if (!m_socket.TryGetValue(iSeq, out var value))
			{
				return false;
			}
			iMountStoneNum = value.m_iMountStoneNum;
			return true;
		}

		public bool isStoneMounted(byte iSeq)
		{
			if (!m_socket.TryGetValue(iSeq, out var value))
			{
				return false;
			}
			return value.m_iMountStoneNum != 0;
		}

		public bool getStoneAttr(out Dictionary<ushort, float> attr)
		{
			attr = new Dictionary<ushort, float>();
			foreach (KeyValuePair<byte, Socket> item in m_socket)
			{
				foreach (KeyValuePair<short, float> item2 in item.Value.m_attr)
				{
					attr[(ushort)item2.Key] += item2.Value;
				}
			}
			return true;
		}

		public bool getStoneAttr(out Dictionary<short, float> attr)
		{
			attr = new Dictionary<short, float>();
			foreach (KeyValuePair<byte, Socket> item in m_socket)
			{
				foreach (KeyValuePair<short, float> item2 in item.Value.m_attr)
				{
					if (!attr.ContainsKey(item2.Key))
					{
						attr.Add(item2.Key, item2.Value);
					}
					else
					{
						attr[item2.Key] += item2.Value;
					}
				}
			}
			return true;
		}

		public void encode(PacketWriter encoder)
		{
			encoder.Write(m_iItemNum);
			encoder.Write(m_socket.Count);
			foreach (KeyValuePair<byte, Socket> item in m_socket)
			{
				encoder.Write(item.Key);
				encoder.Write(item.Value.m_iSocketNum);
				encoder.Write(item.Value.m_iMountStoneNum);
				encoder.Write(item.Value.m_attr.Count);
				foreach (KeyValuePair<short, float> item2 in item.Value.m_attr)
				{
					encoder.Write(item2.Key);
					encoder.Write(item2.Value);
				}
			}
		}
	}
}
