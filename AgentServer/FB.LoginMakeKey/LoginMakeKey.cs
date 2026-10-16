using System;
using FlatBuffers;

namespace FB.LoginMakeKey
{
	public struct LoginMakeKey : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int KeyLength
		{
			get
			{
				int num = __p.__offset(4);
				if (num == 0)
				{
					return 0;
				}
				return __p.__vector_len(num);
			}
		}

		public sbyte Unk1
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetSbyte(num + __p.bb_pos);
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginMakeKey GetRootAsLoginMakeKey(ByteBuffer _bb)
		{
			return GetRootAsLoginMakeKey(_bb, default(LoginMakeKey));
		}

		public static LoginMakeKey GetRootAsLoginMakeKey(ByteBuffer _bb, LoginMakeKey obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginMakeKey __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public byte Key(int j)
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.Get(__p.__vector(num) + j);
		}

		public ArraySegment<byte>? GetKeyBytes()
		{
			return __p.__vector_as_arraysegment(4);
		}

		public byte[] GetKeyArray()
		{
			return __p.__vector_as_array<byte>(4);
		}

		public static Offset<LoginMakeKey> CreateLoginMakeKey(FlatBufferBuilder builder, VectorOffset keyOffset = default(VectorOffset), sbyte unk1 = 0)
		{
			builder.StartTable(2);
			AddKey(builder, keyOffset);
			AddUnk1(builder, unk1);
			return EndLoginMakeKey(builder);
		}

		public static void StartLoginMakeKey(FlatBufferBuilder builder)
		{
			builder.StartTable(2);
		}

		public static void AddKey(FlatBufferBuilder builder, VectorOffset keyOffset)
		{
			builder.AddOffset(0, keyOffset.Value, 0);
		}

		public static VectorOffset CreateKeyVector(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddByte(data[num]);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreateKeyVectorBlock(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartKeyVector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(1, numElems, 1);
		}

		public static void AddUnk1(FlatBufferBuilder builder, sbyte unk1)
		{
			builder.AddSbyte(1, unk1, 0);
		}

		public static Offset<LoginMakeKey> EndLoginMakeKey(FlatBufferBuilder builder)
		{
			return new Offset<LoginMakeKey>(builder.EndTable());
		}

		public static void FinishLoginMakeKeyBuffer(FlatBufferBuilder builder, Offset<LoginMakeKey> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginMakeKeyBuffer(FlatBufferBuilder builder, Offset<LoginMakeKey> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
