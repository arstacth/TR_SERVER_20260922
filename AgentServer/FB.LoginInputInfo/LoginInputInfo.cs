using System;
using FlatBuffers;

namespace FB.LoginInputInfo
{
	public struct LoginInputInfo : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int Unk1
		{
			get
			{
				int num = __p.__offset(4);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk2
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public string Userid
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

		public int PasswordLength
		{
			get
			{
				int num = __p.__offset(10);
				if (num == 0)
				{
					return 0;
				}
				return __p.__vector_len(num);
			}
		}

		public int Unk3
		{
			get
			{
				int num = __p.__offset(12);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk4
		{
			get
			{
				int num = __p.__offset(14);
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

		public static LoginInputInfo GetRootAsLoginInputInfo(ByteBuffer _bb)
		{
			return GetRootAsLoginInputInfo(_bb, default(LoginInputInfo));
		}

		public static LoginInputInfo GetRootAsLoginInputInfo(ByteBuffer _bb, LoginInputInfo obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginInputInfo __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public ArraySegment<byte>? GetUseridBytes()
		{
			return __p.__vector_as_arraysegment(8);
		}

		public byte[] GetUseridArray()
		{
			return __p.__vector_as_array<byte>(8);
		}

		public byte Password(int j)
		{
			int num = __p.__offset(10);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.Get(__p.__vector(num) + j);
		}

		public ArraySegment<byte>? GetPasswordBytes()
		{
			return __p.__vector_as_arraysegment(10);
		}

		public byte[] GetPasswordArray()
		{
			return __p.__vector_as_array<byte>(10);
		}

		public static Offset<LoginInputInfo> CreateLoginInputInfo(FlatBufferBuilder builder, int unk1 = 0, int unk2 = 0, StringOffset useridOffset = default(StringOffset), VectorOffset passwordOffset = default(VectorOffset), int unk3 = 0, int unk4 = 0)
		{
			builder.StartTable(6);
			AddUnk4(builder, unk4);
			AddUnk3(builder, unk3);
			AddPassword(builder, passwordOffset);
			AddUserid(builder, useridOffset);
			AddUnk2(builder, unk2);
			AddUnk1(builder, unk1);
			return EndLoginInputInfo(builder);
		}

		public static void StartLoginInputInfo(FlatBufferBuilder builder)
		{
			builder.StartTable(6);
		}

		public static void AddUnk1(FlatBufferBuilder builder, int unk1)
		{
			builder.AddInt(0, unk1, 0);
		}

		public static void AddUnk2(FlatBufferBuilder builder, int unk2)
		{
			builder.AddInt(1, unk2, 0);
		}

		public static void AddUserid(FlatBufferBuilder builder, StringOffset useridOffset)
		{
			builder.AddOffset(2, useridOffset.Value, 0);
		}

		public static void AddPassword(FlatBufferBuilder builder, VectorOffset passwordOffset)
		{
			builder.AddOffset(3, passwordOffset.Value, 0);
		}

		public static VectorOffset CreatePasswordVector(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddByte(data[num]);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreatePasswordVectorBlock(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartPasswordVector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(1, numElems, 1);
		}

		public static void AddUnk3(FlatBufferBuilder builder, int unk3)
		{
			builder.AddInt(4, unk3, 0);
		}

		public static void AddUnk4(FlatBufferBuilder builder, int unk4)
		{
			builder.AddInt(5, unk4, 0);
		}

		public static Offset<LoginInputInfo> EndLoginInputInfo(FlatBufferBuilder builder)
		{
			return new Offset<LoginInputInfo>(builder.EndTable());
		}

		public static void FinishLoginInputInfoBuffer(FlatBufferBuilder builder, Offset<LoginInputInfo> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginInputInfoBuffer(FlatBufferBuilder builder, Offset<LoginInputInfo> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
