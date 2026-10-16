using FlatBuffers;

namespace FB.ServerStatus
{
	public struct ServerStatus : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int StatusType
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

		public static ServerStatus GetRootAsServerStatus(ByteBuffer _bb)
		{
			return GetRootAsServerStatus(_bb, default(ServerStatus));
		}

		public static ServerStatus GetRootAsServerStatus(ByteBuffer _bb, ServerStatus obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public ServerStatus __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public static Offset<ServerStatus> CreateServerStatus(FlatBufferBuilder builder, int statusType = 0)
		{
			builder.StartTable(1);
			AddStatusType(builder, statusType);
			return EndServerStatus(builder);
		}

		public static void StartServerStatus(FlatBufferBuilder builder)
		{
			builder.StartTable(1);
		}

		public static void AddStatusType(FlatBufferBuilder builder, int statusType)
		{
			builder.AddInt(0, statusType, 0);
		}

		public static Offset<ServerStatus> EndServerStatus(FlatBufferBuilder builder)
		{
			return new Offset<ServerStatus>(builder.EndTable());
		}

		public static void FinishServerStatusBuffer(FlatBufferBuilder builder, Offset<ServerStatus> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedServerStatusBuffer(FlatBufferBuilder builder, Offset<ServerStatus> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
