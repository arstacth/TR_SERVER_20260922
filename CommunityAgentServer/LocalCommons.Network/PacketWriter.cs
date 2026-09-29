using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace LocalCommons.Network
{
	public class PacketWriter : IDisposable
	{
		private static Stack<PacketWriter> m_Pool = new Stack<PacketWriter>();

		private bool m_LittleEndian;

		private MemoryStream m_Stream;

		private int m_Capacity;

		private bool disposedValue;

		public long Length => m_Stream.Length;

		public long Position
		{
			get
			{
				return m_Stream.Position;
			}
			set
			{
				m_Stream.Position = value;
			}
		}

		public MemoryStream UnderlyingStream => m_Stream;

		public static PacketWriter CreateInstance()
		{
			return CreateInstance(32, LittleEndian: false);
		}

		public static PacketWriter CreateInstance(int capacity, bool LittleEndian)
		{
			PacketWriter packetWriter = null;
			lock (m_Pool)
			{
				if (m_Pool.Count > 0)
				{
					packetWriter = m_Pool.Pop();
					if (packetWriter != null)
					{
						packetWriter.m_Capacity = capacity;
						packetWriter.m_Stream.SetLength(0L);
					}
				}
			}
			if (packetWriter == null)
			{
				packetWriter = new PacketWriter(capacity);
			}
			packetWriter.m_LittleEndian = LittleEndian;
			return packetWriter;
		}

		public static void ReleaseInstance(PacketWriter pw)
		{
			lock (m_Pool)
			{
				if (!m_Pool.Contains(pw))
				{
					m_Pool.Push(pw);
					return;
				}
				try
				{
					using StreamWriter streamWriter = new StreamWriter("neterr.log");
					streamWriter.WriteLine("{0}\tInstance pool contains writer", DateTime.Now);
				}
				catch
				{
					Console.WriteLine("net error");
				}
			}
		}

		public PacketWriter()
			: this(32)
		{
		}

		public PacketWriter(int capacity)
		{
			m_Stream = new MemoryStream(capacity);
			m_Capacity = capacity;
		}

		public void Write(bool value)
		{
			m_Stream.WriteByte((byte)(value ? 1u : 0u));
		}

		public void Write(byte value)
		{
			m_Stream.WriteByte(value);
		}

		public void Write(sbyte value)
		{
			m_Stream.WriteByte((byte)value);
		}

		public void Write(short value)
		{
			byte[] array = new byte[4];
			if (m_LittleEndian)
			{
				array[1] = (byte)(value >> 8);
				array[0] = (byte)value;
			}
			else
			{
				array[0] = (byte)(value >> 8);
				array[1] = (byte)value;
			}
			m_Stream.Write(array, 0, 2);
		}

		public void Write(ushort value)
		{
			byte[] array = new byte[4];
			if (m_LittleEndian)
			{
				array[1] = (byte)(value >> 8);
				array[0] = (byte)value;
			}
			else
			{
				array[0] = (byte)(value >> 8);
				array[1] = (byte)value;
			}
			m_Stream.Write(array, 0, 2);
		}

		public void WriteOP(object value)
		{
			short value2 = Convert.ToInt16(value);
			Write(value2);
		}

		public void Write(int value)
		{
			byte[] array = new byte[4];
			if (m_LittleEndian)
			{
				array[3] = (byte)(value >> 24);
				array[2] = (byte)(value >> 16);
				array[1] = (byte)(value >> 8);
				array[0] = (byte)value;
			}
			else
			{
				array[0] = (byte)(value >> 24);
				array[1] = (byte)(value >> 16);
				array[2] = (byte)(value >> 8);
				array[3] = (byte)value;
			}
			m_Stream.Write(array, 0, 4);
		}

		public void Write(uint value)
		{
			byte[] array = new byte[4];
			if (m_LittleEndian)
			{
				array[3] = (byte)(value >> 24);
				array[2] = (byte)(value >> 16);
				array[1] = (byte)(value >> 8);
				array[0] = (byte)value;
			}
			else
			{
				array[0] = (byte)(value >> 24);
				array[1] = (byte)(value >> 16);
				array[2] = (byte)(value >> 8);
				array[3] = (byte)value;
			}
			m_Stream.Write(array, 0, 4);
		}

		public void Writec(float value, bool c)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!m_LittleEndian)
			{
				Array.Reverse(bytes);
			}
			m_Stream.Write(bytes, 0, bytes.Length);
		}

		public void WriteLEBE(float value, bool le)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!le)
			{
				Array.Reverse(bytes);
			}
			m_Stream.Write(bytes, 0, bytes.Length);
		}

		public void Write(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!m_LittleEndian)
			{
				Array.Reverse(bytes);
			}
			m_Stream.Write(bytes, 0, 4);
		}

		public void Write(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!m_LittleEndian)
			{
				Array.Reverse(bytes);
			}
			m_Stream.Write(bytes, 0, 8);
		}

		public void Write(byte[] buffer, int offset, int size)
		{
			m_Stream.Write(buffer, offset, size);
		}

		public void Write(byte[] buffer, int offset)
		{
			int num = buffer.Length;
			Write((short)num);
			m_Stream.Write(buffer, offset, num);
		}

		public void WriteASCIIFixedNoSize(string value, int size)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			m_Stream.SetLength(m_Stream.Length + size);
			if (length >= size)
			{
				m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += size;
		}

		public void WriteASCIIFixed(string value, int size)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			Write((short)size);
			m_Stream.SetLength(m_Stream.Length + size);
			if (length >= size)
			{
				m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += size;
		}

		public void WriteASCIIFixed_intSize(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteAsciiFixed() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			int length2 = value.Length;
			Write(length2);
			m_Stream.SetLength(m_Stream.Length + length2);
			if (length >= length2)
			{
				m_Stream.Position += Encoding.ASCII.GetBytes(value, 0, length2, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += length2;
		}

		public void WriteUTF8Fixed(string value, int size)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteUTF8Fixed() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			Write((short)size);
			m_Stream.SetLength(m_Stream.Length + size);
			if (length >= size)
			{
				m_Stream.Position += Encoding.UTF8.GetBytes(value, 0, size, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.UTF8.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += size * 3;
		}

		public void WriteAnsiFixed_intSize(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteAnsiFixed_intSize() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			short num = (short)Encoding.Default.GetBytes(value).Length;
			Write(num);
			m_Stream.SetLength(m_Stream.Length + num);
			if (length >= num)
			{
				m_Stream.Position += Encoding.Default.GetBytes(value, 0, num, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.Default.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += num;
		}

		public void WriteBIG5Fixed_shortSize(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteBIG5Fixed_shortSize() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			short num = (short)Encoding.Default.GetBytes(value).Length;
			Write(num);
			m_Stream.SetLength(m_Stream.Length + num);
			if (length >= num)
			{
				m_Stream.Position += Encoding.Default.GetBytes(value, 0, num, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.Default.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += num;
		}

		public void WriteHex(string value)
		{
			if (value.Length % 2 != 0)
			{
				Console.Write("Network: Attempted to WriteHex() the binary key cannot have an odd number of digits");
				return;
			}
			int num = value.Length / 2;
			m_Stream.SetLength(m_Stream.Length + num);
			byte[] array = new byte[value.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			array.CopyTo(m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += num;
		}

		public void WriteHex(string value, int size)
		{
			if (value.Length % 2 != 0)
			{
				Console.Write("Network: Attempted to WriteHex() the binary key cannot have an odd number of digits");
				return;
			}
			if (value.Length / 2 != size / 2)
			{
				Console.WriteLine("Network: Attempted to WriteHex(value, size) with not equ value.Length and size value");
			}
			int num = value.Length / 2;
			Write((short)num);
			m_Stream.SetLength(m_Stream.Length + num);
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			array.CopyTo(m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += num;
		}

		public void WriteDynamicASCII(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteAsciiNull() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			m_Stream.SetLength(m_Stream.Length + length + 1);
			Encoding.ASCII.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += length + 1;
		}

		public void WriteDynamicLittleUni(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteLittleUniNull() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			m_Stream.SetLength(m_Stream.Length + (length + 1) * 2);
			m_Stream.Position += Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += 2L;
		}

		public void WriteFixedLittleEndian(string value, int size)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteLittleUniFixed() with null value");
				value = string.Empty;
			}
			size *= 2;
			int length = value.Length;
			Write((short)length);
			m_Stream.SetLength(m_Stream.Length + size);
			if (length * 2 >= size)
			{
				m_Stream.Position += Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.Unicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += size;
		}

		public void WriteDynamicBigUnicode(string value)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteBigUniNull() with null value");
				value = string.Empty;
			}
			int length = value.Length;
			Write((short)length);
			m_Stream.SetLength(m_Stream.Length + (length + 1) * 2);
			m_Stream.Position += Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += 2L;
		}

		public void WriteFixedBigUnicode(string value, int size)
		{
			if (value == null)
			{
				Console.WriteLine("Network: Attempted to WriteBigUniFixed() with null value");
				value = string.Empty;
			}
			size *= 2;
			int length = value.Length;
			Write((short)length);
			m_Stream.SetLength(m_Stream.Length + size);
			if (length * 2 >= size)
			{
				m_Stream.Position += Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
				return;
			}
			Encoding.BigEndianUnicode.GetBytes(value, 0, length, m_Stream.GetBuffer(), (int)m_Stream.Position);
			m_Stream.Position += size;
		}

		public void Fill()
		{
			Fill((int)(m_Capacity - m_Stream.Length));
		}

		public void Fill(int length)
		{
			if (m_Stream.Position == m_Stream.Length)
			{
				m_Stream.SetLength(m_Stream.Length + length);
				m_Stream.Seek(0L, SeekOrigin.End);
			}
			else
			{
				m_Stream.Write(new byte[length], 0, length);
			}
		}

		public long Seek(long offset, SeekOrigin origin)
		{
			return m_Stream.Seek(offset, origin);
		}

		public byte[] ToArray()
		{
			return m_Stream.ToArray();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					m_Stream.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
