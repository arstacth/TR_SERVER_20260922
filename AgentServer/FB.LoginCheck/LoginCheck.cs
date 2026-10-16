using FlatBuffers;

namespace FB.LoginCheck
{
	public struct LoginCheck : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int Error
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

		public int Unk1
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

		public int Unk2
		{
			get
			{
				int num = __p.__offset(8);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int LoginResultLength
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

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginCheck GetRootAsLoginCheck(ByteBuffer _bb)
		{
			return GetRootAsLoginCheck(_bb, default(LoginCheck));
		}

		public static LoginCheck GetRootAsLoginCheck(ByteBuffer _bb, LoginCheck obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginCheck __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public LoginString? LoginResult(int j)
		{
			int num = __p.__offset(10);
			if (num == 0)
			{
				return null;
			}
			return default(LoginString).__assign(__p.__indirect(__p.__vector(num) + j * 4), __p.bb);
		}

		public static Offset<LoginCheck> CreateLoginCheck(FlatBufferBuilder builder, int error = 0, int unk1 = 0, int unk2 = 0, VectorOffset loginResultOffset = default(VectorOffset))
		{
			builder.StartTable(4);
			AddLoginResult(builder, loginResultOffset);
			AddUnk2(builder, unk2);
			AddUnk1(builder, unk1);
			AddError(builder, error);
			return EndLoginCheck(builder);
		}

		public static void StartLoginCheck(FlatBufferBuilder builder)
		{
			builder.StartTable(4);
		}

		public static void AddError(FlatBufferBuilder builder, int error)
		{
			builder.AddInt(0, error, 0);
		}

		public static void AddUnk1(FlatBufferBuilder builder, int unk1)
		{
			builder.AddInt(1, unk1, 0);
		}

		public static void AddUnk2(FlatBufferBuilder builder, int unk2)
		{
			builder.AddInt(2, unk2, 0);
		}

		public static void AddLoginResult(FlatBufferBuilder builder, VectorOffset loginResultOffset)
		{
			builder.AddOffset(3, loginResultOffset.Value, 0);
		}

		public static VectorOffset CreateLoginResultVector(FlatBufferBuilder builder, Offset<LoginString>[] data)
		{
			builder.StartVector(4, data.Length, 4);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddOffset(data[num].Value);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreateLoginResultVectorBlock(FlatBufferBuilder builder, Offset<LoginString>[] data)
		{
			builder.StartVector(4, data.Length, 4);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartLoginResultVector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(4, numElems, 4);
		}

		public static Offset<LoginCheck> EndLoginCheck(FlatBufferBuilder builder)
		{
			return new Offset<LoginCheck>(builder.EndTable());
		}

		public static void FinishLoginCheckBuffer(FlatBufferBuilder builder, Offset<LoginCheck> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginCheckBuffer(FlatBufferBuilder builder, Offset<LoginCheck> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
