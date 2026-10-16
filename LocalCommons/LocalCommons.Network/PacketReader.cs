using System;
using System.IO;
using System.Text;
using LocalCommons.Cryptography;

namespace LocalCommons.Network
{
	public class PacketReader
	{
		private byte[] m_Data;

		private int m_Size;

		private int m_Index;

		public byte[] Buffer => m_Data;

		public int Size => m_Size;

		public int Offset
		{
			get
			{
				return m_Index;
			}
			set
			{
				m_Index = value;
			}
		}

		public int Remaining => m_Size - m_Index;

		public PacketReader(byte[] data, int offset)
		{
			m_Data = data;
			m_Size = ((m_Data != null) ? data.Length : 0);
			m_Index = offset;
		}

		public byte[] Clear()
		{
			for (int i = 0; i < m_Size; i++)
			{
				m_Data[i] = 0;
			}
			return m_Data;
		}

		public bool Decrypt(byte[] inkey, byte[] xorkey)
		{
			m_Data = Encrypt.DecryptByte(inkey, xorkey, m_Data);
			return true;
		}

		public byte[] ReadByteArray(int length)
		{
			if (length > m_Size)
			{
				return new byte[0];
			}
			byte[] array = new byte[length];
			Array.Copy(m_Data, m_Index, array, 0, array.Length);
			m_Index += length;
			return array;
		}

		public int Seek(int offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				m_Index = offset;
				break;
			case SeekOrigin.Current:
				m_Index += offset;
				break;
			case SeekOrigin.End:
				m_Index = m_Size - offset;
				break;
			}
			return m_Index;
		}

		public long ReadInt64()
		{
			if (m_Index + 8 > m_Size)
			{
				return 0L;
			}
			uint num = (uint)(m_Data[m_Index++] | (m_Data[m_Index++] << 8) | (m_Data[m_Index++] << 16) | (m_Data[m_Index++] << 24));
			return (long)(((ulong)(uint)(m_Data[m_Index++] | (m_Data[m_Index++] << 8) | (m_Data[m_Index++] << 16) | (m_Data[m_Index++] << 24)) << 32) | num);
		}

		public int ReadInt32()
		{
			if (m_Index + 4 > m_Size)
			{
				return 0;
			}
			return (m_Data[m_Index++] << 24) | (m_Data[m_Index++] << 16) | (m_Data[m_Index++] << 8) | m_Data[m_Index++];
		}

		public short ReadInt16()
		{
			if (m_Index + 2 > m_Size)
			{
				return 0;
			}
			return (short)((m_Data[m_Index++] << 8) | m_Data[m_Index++]);
		}

		public byte ReadByte()
		{
			if (m_Index + 1 > m_Size)
			{
				return 0;
			}
			return m_Data[m_Index++];
		}

		public uint ReadUInt32()
		{
			if (m_Index + 4 > m_Size)
			{
				return 0u;
			}
			return (uint)((m_Data[m_Index++] << 24) | (m_Data[m_Index++] << 16) | (m_Data[m_Index++] << 8) | m_Data[m_Index++]);
		}

		public ushort ReadUInt16()
		{
			if (m_Index + 2 > m_Size)
			{
				return 0;
			}
			return (ushort)((m_Data[m_Index++] << 8) | m_Data[m_Index++]);
		}

		public sbyte ReadSByte()
		{
			if (m_Index + 1 > m_Size)
			{
				return 0;
			}
			return (sbyte)m_Data[m_Index++];
		}

		public bool ReadBoolean()
		{
			if (m_Index + 1 > m_Size)
			{
				return false;
			}
			return m_Data[m_Index++] != 0;
		}

		public string ReadUnicodeStringLE()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index + 1 < m_Size && (num = m_Data[m_Index++] | (m_Data[m_Index++] << 8)) != 0)
			{
				stringBuilder.Append((char)num);
			}
			return stringBuilder.ToString();
		}

		public string ReadUnicodeStringLESafe(int fixedLength)
		{
			int num = m_Index + (fixedLength << 1);
			int index = num;
			if (num > m_Size)
			{
				num = m_Size;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num2;
			while (m_Index + 1 < num && (num2 = m_Data[m_Index++] | (m_Data[m_Index++] << 8)) != 0)
			{
				if (IsSafeChar(num2))
				{
					stringBuilder.Append((char)num2);
				}
			}
			m_Index = index;
			return stringBuilder.ToString();
		}

		public string ReadUnicodeStringLESafe()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index + 1 < m_Size && (num = m_Data[m_Index++] | (m_Data[m_Index++] << 8)) != 0)
			{
				if (IsSafeChar(num))
				{
					stringBuilder.Append((char)num);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadUnicodeStringSafe()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index + 1 < m_Size && (num = (m_Data[m_Index++] << 8) | m_Data[m_Index++]) != 0)
			{
				if (IsSafeChar(num))
				{
					stringBuilder.Append((char)num);
				}
			}
			return stringBuilder.ToString();
		}

		public short ReadLEInt16()
		{
			if (m_Index + 2 > m_Size)
			{
				return 0;
			}
			short result = BitConverter.ToInt16(m_Data, m_Index);
			m_Index += 2;
			return result;
		}

		public ushort ReadLEUInt16()
		{
			if (m_Index + 2 > m_Size)
			{
				return 0;
			}
			ushort result = BitConverter.ToUInt16(m_Data, m_Index);
			m_Index += 2;
			return result;
		}

		public uint ReadLEUInt32()
		{
			if (m_Index + 4 > m_Size)
			{
				return 0u;
			}
			uint result = BitConverter.ToUInt32(m_Data, m_Index);
			m_Index += 4;
			return result;
		}

		public int ReadLEInt32()
		{
			if (m_Index + 4 > m_Size)
			{
				return 0;
			}
			int result = BitConverter.ToInt32(m_Data, m_Index);
			m_Index += 4;
			return result;
		}

		public long ReadLEInt64()
		{
			if (m_Index + 8 > m_Size)
			{
				return 0L;
			}
			long result = BitConverter.ToInt64(m_Data, m_Index);
			m_Index += 8;
			return result;
		}

		public float ReadLESingle()
		{
			if (m_Index + 4 > m_Size)
			{
				return 0f;
			}
			float result = BitConverter.ToSingle(m_Data, m_Index);
			m_Index += 4;
			return result;
		}

		public string ReadUnicodeString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index + 1 < m_Size && (num = (m_Data[m_Index++] << 8) | m_Data[m_Index++]) != 0)
			{
				stringBuilder.Append((char)num);
			}
			return stringBuilder.ToString();
		}

		private bool IsSafeChar(int c)
		{
			if (c >= 32)
			{
				return c < 65534;
			}
			return false;
		}

		private bool IsSafeChar2(int c)
		{
			if (c >= 10)
			{
				return c < 65534;
			}
			return false;
		}

		public string ReadUTF8StringSafe(int fixedLength)
		{
			if (m_Index >= m_Size)
			{
				m_Index += fixedLength;
				return string.Empty;
			}
			int num = m_Index + fixedLength;
			if (num > m_Size)
			{
				num = m_Size;
			}
			int num2 = 0;
			int index = m_Index;
			int index2 = m_Index;
			while (index < num && m_Data[index++] != 0)
			{
				num2++;
			}
			index = 0;
			byte[] array = new byte[num2];
			int num3 = 0;
			while (m_Index < num && (num3 = m_Data[m_Index++]) != 0)
			{
				array[index++] = (byte)num3;
			}
			string @string = Encoding.UTF8.GetString(array);
			bool flag = true;
			int num4 = 0;
			while (flag && num4 < @string.Length)
			{
				flag = IsSafeChar(@string[num4]);
				num4++;
			}
			m_Index = index2 + fixedLength;
			if (flag)
			{
				return @string;
			}
			StringBuilder stringBuilder = new StringBuilder(@string.Length);
			for (int i = 0; i < @string.Length; i++)
			{
				if (IsSafeChar(@string[i]))
				{
					stringBuilder.Append(@string[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadBig5StringSafe(int fixedLength)
		{
			if (m_Index >= m_Size)
			{
				m_Index += fixedLength;
				return string.Empty;
			}
			int num = m_Index + fixedLength;
			if (num > m_Size)
			{
				num = m_Size;
			}
			int num2 = 0;
			int index = m_Index;
			int index2 = m_Index;
			while (index < num && m_Data[index++] != 0)
			{
				num2++;
			}
			index = 0;
			byte[] array = new byte[num2];
			int num3 = 0;
			while (m_Index < num && (num3 = m_Data[m_Index++]) != 0)
			{
				array[index++] = (byte)num3;
			}
			string @string = Encoding.Default.GetString(array);
			bool flag = true;
			int num4 = 0;
			while (flag && num4 < @string.Length)
			{
				flag = IsSafeChar(@string[num4]);
				num4++;
			}
			m_Index = index2 + fixedLength;
			if (flag)
			{
				return @string;
			}
			StringBuilder stringBuilder = new StringBuilder(@string.Length);
			for (int i = 0; i < @string.Length; i++)
			{
				if (IsSafeChar(@string[i]))
				{
					stringBuilder.Append(@string[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadBig5StringSafeNL(int fixedLength)
		{
			if (m_Index >= m_Size)
			{
				m_Index += fixedLength;
				return string.Empty;
			}
			int num = m_Index + fixedLength;
			if (num > m_Size)
			{
				num = m_Size;
			}
			int num2 = 0;
			int index = m_Index;
			int index2 = m_Index;
			while (index < num && m_Data[index++] != 0)
			{
				num2++;
			}
			index = 0;
			byte[] array = new byte[num2];
			int num3 = 0;
			while (m_Index < num && (num3 = m_Data[m_Index++]) != 0)
			{
				array[index++] = (byte)num3;
			}
			string @string = Encoding.Default.GetString(array);
			bool flag = true;
			int num4 = 0;
			while (flag && num4 < @string.Length)
			{
				flag = IsSafeChar2(@string[num4]);
				num4++;
			}
			m_Index = index2 + fixedLength;
			if (flag)
			{
				return @string;
			}
			StringBuilder stringBuilder = new StringBuilder(@string.Length);
			for (int i = 0; i < @string.Length; i++)
			{
				if (IsSafeChar2(@string[i]))
				{
					stringBuilder.Append(@string[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadUTF8StringSafe()
		{
			if (m_Index >= m_Size)
			{
				return string.Empty;
			}
			int num = 0;
			int index = m_Index;
			while (index < m_Size && m_Data[index++] != 0)
			{
				num++;
			}
			index = 0;
			byte[] array = new byte[num];
			int num2 = 0;
			while (m_Index < m_Size && (num2 = m_Data[m_Index++]) != 0)
			{
				array[index++] = (byte)num2;
			}
			string @string = Encoding.UTF8.GetString(array);
			bool flag = true;
			int num3 = 0;
			while (flag && num3 < @string.Length)
			{
				flag = IsSafeChar(@string[num3]);
				num3++;
			}
			if (flag)
			{
				return @string;
			}
			StringBuilder stringBuilder = new StringBuilder(@string.Length);
			for (int i = 0; i < @string.Length; i++)
			{
				if (IsSafeChar(@string[i]))
				{
					stringBuilder.Append(@string[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadUTF8String()
		{
			if (m_Index >= m_Size)
			{
				return string.Empty;
			}
			int num = 0;
			int index = m_Index;
			while (index < m_Size && m_Data[index++] != 0)
			{
				num++;
			}
			index = 0;
			byte[] array = new byte[num];
			int num2 = 0;
			while (m_Index < m_Size && (num2 = m_Data[m_Index++]) != 0)
			{
				array[index++] = (byte)num2;
			}
			return Encoding.UTF8.GetString(array);
		}

		public string ReadDynamicString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index < m_Size && (num = m_Data[m_Index++]) != 0)
			{
				stringBuilder.Append((char)num);
			}
			return stringBuilder.ToString();
		}

		public string ReadBig5StringSafe()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while (m_Index < m_Size && (num = m_Data[m_Index++]) != 0)
			{
				if (IsSafeChar(num))
				{
					stringBuilder.Append((char)num);
				}
			}
			return stringBuilder.ToString();
		}

		public string ReadUnicodeStringSafe(int fixedLength)
		{
			int num = m_Index + (fixedLength << 1);
			int index = num;
			if (num > m_Size)
			{
				num = m_Size;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num2;
			while (m_Index + 1 < num && (num2 = (m_Data[m_Index++] << 8) | m_Data[m_Index++]) != 0)
			{
				if (IsSafeChar(num2))
				{
					stringBuilder.Append((char)num2);
				}
			}
			m_Index = index;
			return stringBuilder.ToString();
		}

		public string ReadUnicodeString(int fixedLength)
		{
			int num = m_Index + (fixedLength << 1);
			int index = num;
			if (num > m_Size)
			{
				num = m_Size;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num2;
			while (m_Index + 1 < num && (num2 = (m_Data[m_Index++] << 8) | m_Data[m_Index++]) != 0)
			{
				stringBuilder.Append((char)num2);
			}
			m_Index = index;
			return stringBuilder.ToString();
		}

		public string ReadHexString(int fixedLength)
		{
			int num = m_Index + fixedLength;
			int index = num;
			if (num > m_Size)
			{
				num = m_Size;
			}
			StringBuilder stringBuilder = new StringBuilder();
			int num2;
			while (m_Index < num && (num2 = m_Data[m_Index++]) != 0)
			{
				stringBuilder.Append(num2.ToString("x0").PadLeft(2, '0'));
			}
			m_Index = index;
			return stringBuilder.ToString();
		}
	}
}
