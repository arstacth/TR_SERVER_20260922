using FlatBuffers;

namespace FB.LoginErrorBan
{
	public struct LoginErrorBan : IFlatbufferObject
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

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginErrorBan GetRootAsLoginErrorBan(ByteBuffer _bb)
		{
			return GetRootAsLoginErrorBan(_bb, default(LoginErrorBan));
		}

		public static LoginErrorBan GetRootAsLoginErrorBan(ByteBuffer _bb, LoginErrorBan obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginErrorBan __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public static Offset<LoginErrorBan> CreateLoginErrorBan(FlatBufferBuilder builder, int ErrorCode = 0, sbyte suberrcode = 0)
		{
			builder.StartTable(2);
			AddErrorCode(builder, ErrorCode);
			AddSuberrcode(builder, suberrcode);
			return EndLoginErrorBan(builder);
		}

		public static void StartLoginErrorBan(FlatBufferBuilder builder)
		{
			builder.StartTable(2);
		}

		public static void AddErrorCode(FlatBufferBuilder builder, int ErrorCode)
		{
			builder.AddInt(0, ErrorCode, 0);
		}

		public static void AddSuberrcode(FlatBufferBuilder builder, sbyte suberrcode)
		{
			builder.AddSbyte(1, suberrcode, 0);
		}

		public static Offset<LoginErrorBan> EndLoginErrorBan(FlatBufferBuilder builder)
		{
			return new Offset<LoginErrorBan>(builder.EndTable());
		}

		public static void FinishLoginErrorBanBuffer(FlatBufferBuilder builder, Offset<LoginErrorBan> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginErrorBanBuffer(FlatBufferBuilder builder, Offset<LoginErrorBan> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
