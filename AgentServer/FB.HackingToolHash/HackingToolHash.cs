using FlatBuffers;

namespace FB.HackingToolHash
{
	public struct HackingToolHash : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int HacktoolistLength
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

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static HackingToolHash GetRootAsHackingToolHash(ByteBuffer _bb)
		{
			return GetRootAsHackingToolHash(_bb, default(HackingToolHash));
		}

		public static HackingToolHash GetRootAsHackingToolHash(ByteBuffer _bb, HackingToolHash obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public HackingToolHash __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public HackingToolInfo? Hacktoolist(int j)
		{
			int num = __p.__offset(4);
			if (num == 0)
			{
				return null;
			}
			return default(HackingToolInfo).__assign(__p.__indirect(__p.__vector(num) + j * 4), __p.bb);
		}

		public static Offset<HackingToolHash> CreateHackingToolHash(FlatBufferBuilder builder, VectorOffset hacktoolistOffset = default(VectorOffset))
		{
			builder.StartTable(1);
			AddHacktoolist(builder, hacktoolistOffset);
			return EndHackingToolHash(builder);
		}

		public static void StartHackingToolHash(FlatBufferBuilder builder)
		{
			builder.StartTable(1);
		}

		public static void AddHacktoolist(FlatBufferBuilder builder, VectorOffset hacktoolistOffset)
		{
			builder.AddOffset(0, hacktoolistOffset.Value, 0);
		}

		public static VectorOffset CreateHacktoolistVector(FlatBufferBuilder builder, Offset<HackingToolInfo>[] data)
		{
			builder.StartVector(4, data.Length, 4);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddOffset(data[num].Value);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreateHacktoolistVectorBlock(FlatBufferBuilder builder, Offset<HackingToolInfo>[] data)
		{
			builder.StartVector(4, data.Length, 4);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartHacktoolistVector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(4, numElems, 4);
		}

		public static Offset<HackingToolHash> EndHackingToolHash(FlatBufferBuilder builder)
		{
			return new Offset<HackingToolHash>(builder.EndTable());
		}

		public static void FinishHackingToolHashBuffer(FlatBufferBuilder builder, Offset<HackingToolHash> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedHackingToolHashBuffer(FlatBufferBuilder builder, Offset<HackingToolHash> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
