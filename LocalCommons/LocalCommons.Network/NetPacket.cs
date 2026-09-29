using System.IO;
using Force.Crc32;
using LocalCommons.Cryptography;

namespace LocalCommons.Network
{
	public abstract class NetPacket
	{
		protected PacketWriter ns;

		private readonly int _mPacketId;

		private readonly bool _mLittleEndian;

		private readonly bool _mIsArcheAge = true;

		private readonly byte _level;

		private readonly byte[] _EncryptKey;

		public static byte NumPckSc = 0;

		public static sbyte NumPckCs = -1;

		public bool isEncrypt;

		public byte[] EncryptKey { get; set; }

		public byte[] XorKey { get; set; }

		public PacketWriter UnderlyingStream => ns;

		protected NetPacket()
		{
			_mPacketId = 0;
			_level = 1;
			_mLittleEndian = true;
			_mIsArcheAge = true;
			ns = PacketWriter.CreateInstance(16, LittleEndian: true);
		}

		protected NetPacket(int packetId, bool isLittleEndian)
		{
			_mPacketId = packetId;
			_mLittleEndian = isLittleEndian;
			ns = PacketWriter.CreateInstance(16, isLittleEndian);
		}

		protected NetPacket(byte level, int packetId)
		{
			_mPacketId = packetId;
			_level = level;
			_mLittleEndian = true;
			_mIsArcheAge = true;
			ns = PacketWriter.CreateInstance(16, LittleEndian: true);
		}

		protected NetPacket(byte level, byte[] EncryptKey)
		{
			_mPacketId = 0;
			_level = level;
			_mLittleEndian = true;
			_mIsArcheAge = true;
			ns = PacketWriter.CreateInstance(16, LittleEndian: true);
		}

		protected NetPacket(byte level)
		{
			_mPacketId = 0;
			_level = level;
			_mLittleEndian = true;
			_mIsArcheAge = false;
			ns = PacketWriter.CreateInstance(16, LittleEndian: true);
		}

		public byte[] Compile()
		{
			PacketWriter packetWriter = PacketWriter.CreateInstance(10240, _mLittleEndian);
			if (_mIsArcheAge)
			{
				switch (_level)
				{
				case 0:
				{
					packetWriter.Write((ushort)(ns.Length + 3));
					packetWriter.Write((byte)_mPacketId);
					byte[] array4 = ns.ToArray();
					packetWriter.Write(array4, 0, array4.Length);
					break;
				}
				case 1:
				{
					int value = (int)(isEncrypt ? (ns.Length + 9) : (ns.Length + 8));
					packetWriter.Write(value);
					byte[] array3 = ((!isEncrypt) ? ns.ToArray() : Encrypt.newEncryptByte(EncryptKey, XorKey, ns.ToArray()));
					int num = (int)packetWriter.Position;
					packetWriter.Write(0);
					packetWriter.Write(array3, 0, array3.Length);
					uint value2 = CheckSum(array3);
					packetWriter.Seek(num, SeekOrigin.Begin);
					packetWriter.Write(value2);
					break;
				}
				case 2:
				{
					packetWriter.Write((int)(ns.Length + 8));
					byte[] array2 = ns.ToArray();
					packetWriter.Write(CheckSum(array2));
					packetWriter.Write(array2, 0, array2.Length);
					break;
				}
				case 3:
				{
					packetWriter.Write((int)(ns.Length + 8));
					byte[] array = ns.ToArray();
					packetWriter.Write(CheckSum(array));
					packetWriter.Write(array, 0, array.Length);
					break;
				}
				}
			}
			else
			{
				packetWriter.Write((ushort)(ns.Length + 3));
				byte[] array5 = ns.ToArray();
				packetWriter.Write(array5, 0, array5.Length);
				packetWriter.Write((byte)1);
			}
			byte[] result = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			packetWriter = null;
			return result;
		}

		private uint CheckSum(byte[] redata)
		{
			int length = ((redata.Length > 16) ? 16 : redata.Length);
			return Crc32Algorithm.Compute(redata, 0, length);
		}

		public byte[] CompileOld()
		{
			PacketWriter packetWriter = PacketWriter.CreateInstance(10240, _mLittleEndian);
			ushort value = (isEncrypt ? ((ushort)(ns.Length + 3)) : ((ushort)(ns.Length + 2)));
			if (_mIsArcheAge)
			{
				switch (_level)
				{
				case 0:
				{
					packetWriter.Write((ushort)(ns.Length + 3));
					packetWriter.Write((byte)_mPacketId);
					byte[] array4 = ns.ToArray();
					packetWriter.Write(array4, 0, array4.Length);
					break;
				}
				case 1:
				{
					packetWriter.Write(value);
					byte[] array3 = ((!isEncrypt) ? ns.ToArray() : Encrypt.newEncryptByte(EncryptKey, XorKey, ns.ToArray()));
					packetWriter.Write(array3, 0, array3.Length);
					if (isEncrypt)
					{
						packetWriter.Write((byte)1);
					}
					break;
				}
				case 2:
				{
					byte[] array2 = ns.ToArray();
					packetWriter.Write(array2, 0, array2.Length);
					break;
				}
				case 3:
				{
					packetWriter.Write((ushort)(ns.Length + 2));
					byte[] array = ns.ToArray();
					packetWriter.Write(array, 0, array.Length);
					break;
				}
				}
			}
			else
			{
				packetWriter.Write((ushort)(ns.Length + 3));
				byte[] array5 = ns.ToArray();
				packetWriter.Write(array5, 0, array5.Length);
				packetWriter.Write((byte)1);
			}
			byte[] result = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			packetWriter = null;
			return result;
		}

		public byte[] EncryptPacket(byte[] EncryptKey, byte[] XorKey)
		{
			PacketWriter packetWriter = PacketWriter.CreateInstance(10240, _mLittleEndian);
			ushort value = (isEncrypt ? ((ushort)(ns.Length + 3)) : ((ushort)(ns.Length + 2)));
			packetWriter.Write(value);
			byte[] array = Encrypt.newEncryptByte(EncryptKey, XorKey, ns.ToArray());
			packetWriter.Write(array, 0, array.Length);
			packetWriter.Write((byte)1);
			byte[] result = packetWriter.ToArray();
			PacketWriter.ReleaseInstance(packetWriter);
			return result;
		}

		public byte[] ToArray()
		{
			return ns.ToArray();
		}
	}
}
