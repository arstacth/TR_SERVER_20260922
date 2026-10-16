using FlatBuffers;

namespace FB.LoginError
{
	public struct LoginError : IFlatbufferObject
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

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginError GetRootAsLoginError(ByteBuffer _bb)
		{
			return GetRootAsLoginError(_bb, default(LoginError));
		}

		public static LoginError GetRootAsLoginError(ByteBuffer _bb, LoginError obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginError __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public static Offset<LoginError> CreateLoginError(FlatBufferBuilder builder, int ErrorCode = 0)
		{
			builder.StartTable(1);
			AddErrorCode(builder, ErrorCode);
			return EndLoginError(builder);
		}

		public static void StartLoginError(FlatBufferBuilder builder)
		{
			builder.StartTable(1);
		}

		public static void AddErrorCode(FlatBufferBuilder builder, int ErrorCode)
		{
			builder.AddInt(0, ErrorCode, 0);
		}

		public static Offset<LoginError> EndLoginError(FlatBufferBuilder builder)
		{
			return new Offset<LoginError>(builder.EndTable());
		}

		public static void FinishLoginErrorBuffer(FlatBufferBuilder builder, Offset<LoginError> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginErrorBuffer(FlatBufferBuilder builder, Offset<LoginError> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
