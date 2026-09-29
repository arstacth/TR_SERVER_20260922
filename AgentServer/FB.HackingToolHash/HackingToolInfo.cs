using System;
using FlatBuffers;

namespace FB.HackingToolHash
{
	public struct HackingToolInfo : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public string Hash
		{
			get
			{
				int num = __p.__offset(4);
				if (num == 0)
				{
					return null;
				}
				return __p.__string(num + __p.bb_pos);
			}
		}

		public bool Enable
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return false;
				}
				return __p.bb.Get(num + __p.bb_pos) != 0;
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static HackingToolInfo GetRootAsHackingToolInfo(ByteBuffer _bb)
		{
			return GetRootAsHackingToolInfo(_bb, default(HackingToolInfo));
		}

		public static HackingToolInfo GetRootAsHackingToolInfo(ByteBuffer _bb, HackingToolInfo obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public HackingToolInfo __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public ArraySegment<byte>? GetHashBytes()
		{
			return __p.__vector_as_arraysegment(4);
		}

		public byte[] GetHashArray()
		{
			return __p.__vector_as_array<byte>(4);
		}

		public static Offset<HackingToolInfo> CreateHackingToolInfo(FlatBufferBuilder builder, StringOffset HashOffset = default(StringOffset), bool enable = false)
		{
			builder.StartTable(2);
			AddHash(builder, HashOffset);
			AddEnable(builder, enable);
			return EndHackingToolInfo(builder);
		}

		public static void StartHackingToolInfo(FlatBufferBuilder builder)
		{
			builder.StartTable(2);
		}

		public static void AddHash(FlatBufferBuilder builder, StringOffset HashOffset)
		{
			builder.AddOffset(0, HashOffset.Value, 0);
		}

		public static void AddEnable(FlatBufferBuilder builder, bool enable)
		{
			builder.AddBool(1, enable, d: false);
		}

		public static Offset<HackingToolInfo> EndHackingToolInfo(FlatBufferBuilder builder)
		{
			return new Offset<HackingToolInfo>(builder.EndTable());
		}
	}
}
