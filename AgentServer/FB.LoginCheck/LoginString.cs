using System;
using FlatBuffers;

namespace FB.LoginCheck
{
	public struct LoginString : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public string StrID
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

		public string Userid
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return null;
				}
				return __p.__string(num + __p.bb_pos);
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginString GetRootAsLoginString(ByteBuffer _bb)
		{
			return GetRootAsLoginString(_bb, default(LoginString));
		}

		public static LoginString GetRootAsLoginString(ByteBuffer _bb, LoginString obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginString __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public ArraySegment<byte>? GetStrIDBytes()
		{
			return __p.__vector_as_arraysegment(4);
		}

		public byte[] GetStrIDArray()
		{
			return __p.__vector_as_array<byte>(4);
		}

		public ArraySegment<byte>? GetUseridBytes()
		{
			return __p.__vector_as_arraysegment(6);
		}

		public byte[] GetUseridArray()
		{
			return __p.__vector_as_array<byte>(6);
		}

		public static Offset<LoginString> CreateLoginString(FlatBufferBuilder builder, StringOffset strIDOffset = default(StringOffset), StringOffset useridOffset = default(StringOffset))
		{
			builder.StartTable(2);
			AddUserid(builder, useridOffset);
			AddStrID(builder, strIDOffset);
			return EndLoginString(builder);
		}

		public static void StartLoginString(FlatBufferBuilder builder)
		{
			builder.StartTable(2);
		}

		public static void AddStrID(FlatBufferBuilder builder, StringOffset strIDOffset)
		{
			builder.AddOffset(0, strIDOffset.Value, 0);
		}

		public static void AddUserid(FlatBufferBuilder builder, StringOffset useridOffset)
		{
			builder.AddOffset(1, useridOffset.Value, 0);
		}

		public static Offset<LoginString> EndLoginString(FlatBufferBuilder builder)
		{
			return new Offset<LoginString>(builder.EndTable());
		}
	}
}
