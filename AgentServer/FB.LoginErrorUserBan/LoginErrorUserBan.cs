using FlatBuffers;

namespace FB.LoginErrorUserBan
{
	public struct LoginErrorUserBan : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int ErrorCode
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

		public sbyte Suberrcode
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

		public int Unk
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

		public long Start
		{
			get
			{
				int num = __p.__offset(10);
				if (num == 0)
				{
					return 0L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public long End
		{
			get
			{
				int num = __p.__offset(12);
				if (num == 0)
				{
					return 0L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginErrorUserBan GetRootAsLoginErrorUserBan(ByteBuffer _bb)
		{
			return GetRootAsLoginErrorUserBan(_bb, default(LoginErrorUserBan));
		}

		public static LoginErrorUserBan GetRootAsLoginErrorUserBan(ByteBuffer _bb, LoginErrorUserBan obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginErrorUserBan __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public static Offset<LoginErrorUserBan> CreateLoginErrorUserBan(FlatBufferBuilder builder, int ErrorCode = 0, sbyte suberrcode = 0, int Unk = 0, long start = 0L, long end = 0L)
		{
			builder.StartTable(5);
			AddEnd(builder, end);
			AddStart(builder, start);
			AddUnk(builder, Unk);
			AddErrorCode(builder, ErrorCode);
			AddSuberrcode(builder, suberrcode);
			return EndLoginErrorUserBan(builder);
		}

		public static void StartLoginErrorUserBan(FlatBufferBuilder builder)
		{
			builder.StartTable(5);
		}

		public static void AddErrorCode(FlatBufferBuilder builder, int ErrorCode)
		{
			builder.AddInt(0, ErrorCode, 0);
		}

		public static void AddSuberrcode(FlatBufferBuilder builder, sbyte suberrcode)
		{
			builder.AddSbyte(1, suberrcode, 0);
		}

		public static void AddUnk(FlatBufferBuilder builder, int Unk)
		{
			builder.AddInt(2, Unk, 0);
		}

		public static void AddStart(FlatBufferBuilder builder, long start)
		{
			builder.AddLong(3, start, 0L);
		}

		public static void AddEnd(FlatBufferBuilder builder, long end)
		{
			builder.AddLong(4, end, 0L);
		}

		public static Offset<LoginErrorUserBan> EndLoginErrorUserBan(FlatBufferBuilder builder)
		{
			return new Offset<LoginErrorUserBan>(builder.EndTable());
		}

		public static void FinishLoginErrorUserBanBuffer(FlatBufferBuilder builder, Offset<LoginErrorUserBan> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginErrorUserBanBuffer(FlatBufferBuilder builder, Offset<LoginErrorUserBan> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
