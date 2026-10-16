using System;
using FlatBuffers;

namespace FB.LoginSuccessInfo
{
	public struct LoginSuccessInfo : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public sbyte Unk1
		{
			get
			{
				int num = __p.__offset(4);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetSbyte(num + __p.bb_pos);
			}
		}

		public short ClientVer
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetShort(num + __p.bb_pos);
			}
		}

		public string UserIP
		{
			get
			{
				int num = __p.__offset(8);
				if (num == 0)
				{
					return null;
				}
				return __p.__string(num + __p.bb_pos);
			}
		}

		public string Version
		{
			get
			{
				int num = __p.__offset(10);
				if (num == 0)
				{
					return null;
				}
				return __p.__string(num + __p.bb_pos);
			}
		}

		public string UserID
		{
			get
			{
				int num = __p.__offset(12);
				if (num == 0)
				{
					return null;
				}
				return __p.__string(num + __p.bb_pos);
			}
		}

		public long Unk2
		{
			get
			{
				int num = __p.__offset(14);
				if (num == 0)
				{
					return 0L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public int Unk3Length
		{
			get
			{
				int num = __p.__offset(16);
				if (num == 0)
				{
					return 0;
				}
				return __p.__vector_len(num);
			}
		}

		public int Unk4
		{
			get
			{
				int num = __p.__offset(18);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginSuccessInfo GetRootAsLoginSuccessInfo(ByteBuffer _bb)
		{
			return GetRootAsLoginSuccessInfo(_bb, default(LoginSuccessInfo));
		}

		public static LoginSuccessInfo GetRootAsLoginSuccessInfo(ByteBuffer _bb, LoginSuccessInfo obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginSuccessInfo __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public ArraySegment<byte>? GetUserIPBytes()
		{
			return __p.__vector_as_arraysegment(8);
		}

		public byte[] GetUserIPArray()
		{
			return __p.__vector_as_array<byte>(8);
		}

		public ArraySegment<byte>? GetVersionBytes()
		{
			return __p.__vector_as_arraysegment(10);
		}

		public byte[] GetVersionArray()
		{
			return __p.__vector_as_array<byte>(10);
		}

		public ArraySegment<byte>? GetUserIDBytes()
		{
			return __p.__vector_as_arraysegment(12);
		}

		public byte[] GetUserIDArray()
		{
			return __p.__vector_as_array<byte>(12);
		}

		public int Unk3(int j)
		{
			int num = __p.__offset(16);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.GetInt(__p.__vector(num) + j * 4);
		}

		public ArraySegment<byte>? GetUnk3Bytes()
		{
			return __p.__vector_as_arraysegment(16);
		}

		public int[] GetUnk3Array()
		{
			return __p.__vector_as_array<int>(16);
		}

		public static Offset<LoginSuccessInfo> CreateLoginSuccessInfo(FlatBufferBuilder builder, sbyte unk1 = 0, short clientVer = 0, StringOffset userIPOffset = default(StringOffset), StringOffset versionOffset = default(StringOffset), StringOffset userIDOffset = default(StringOffset), long unk2 = 0L, VectorOffset unk3Offset = default(VectorOffset), int unk4 = 0)
		{
			builder.StartTable(8);
			AddUnk2(builder, unk2);
			AddUnk4(builder, unk4);
			AddUnk3(builder, unk3Offset);
			AddUserID(builder, userIDOffset);
			AddVersion(builder, versionOffset);
			AddUserIP(builder, userIPOffset);
			AddClientVer(builder, clientVer);
			AddUnk1(builder, unk1);
			return EndLoginSuccessInfo(builder);
		}

		public static void StartLoginSuccessInfo(FlatBufferBuilder builder)
		{
			builder.StartTable(8);
		}

		public static void AddUnk1(FlatBufferBuilder builder, sbyte unk1)
		{
			builder.AddSbyte(0, unk1, 0);
		}

		public static void AddClientVer(FlatBufferBuilder builder, short clientVer)
		{
			builder.AddShort(1, clientVer, 0);
		}

		public static void AddUserIP(FlatBufferBuilder builder, StringOffset userIPOffset)
		{
			builder.AddOffset(2, userIPOffset.Value, 0);
		}

		public static void AddVersion(FlatBufferBuilder builder, StringOffset versionOffset)
		{
			builder.AddOffset(3, versionOffset.Value, 0);
		}

		public static void AddUserID(FlatBufferBuilder builder, StringOffset userIDOffset)
		{
			builder.AddOffset(4, userIDOffset.Value, 0);
		}

		public static void AddUnk2(FlatBufferBuilder builder, long unk2)
		{
			builder.AddLong(5, unk2, 0L);
		}

		public static void AddUnk3(FlatBufferBuilder builder, VectorOffset unk3Offset)
		{
			builder.AddOffset(6, unk3Offset.Value, 0);
		}

		public static VectorOffset CreateUnk3Vector(FlatBufferBuilder builder, int[] data)
		{
			builder.StartVector(4, data.Length, 4);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddInt(data[num]);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreateUnk3VectorBlock(FlatBufferBuilder builder, int[] data)
		{
			builder.StartVector(4, data.Length, 4);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartUnk3Vector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(4, numElems, 4);
		}

		public static void AddUnk4(FlatBufferBuilder builder, int unk4)
		{
			builder.AddInt(7, unk4, 0);
		}

		public static Offset<LoginSuccessInfo> EndLoginSuccessInfo(FlatBufferBuilder builder)
		{
			return new Offset<LoginSuccessInfo>(builder.EndTable());
		}

		public static void FinishLoginSuccessInfoBuffer(FlatBufferBuilder builder, Offset<LoginSuccessInfo> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginSuccessInfoBuffer(FlatBufferBuilder builder, Offset<LoginSuccessInfo> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
