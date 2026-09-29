using System;
using FlatBuffers;

namespace FB.LoginUserInfo
{
	public struct LoginCoupleInfo : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int CoupleNum
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

		public int CoupleType
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

		public string MateName
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

		public long CreateTime
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

		public long MarriedTime
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

		public long RingChangedTime
		{
			get
			{
				int num = __p.__offset(14);
				if (num == 0)
				{
					return 0L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public int CoupleRingNum
		{
			get
			{
				int num = __p.__offset(16);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int CondDays
		{
			get
			{
				int num = __p.__offset(18);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public long CoupleEXP
		{
			get
			{
				int num = __p.__offset(20);
				if (num == 0)
				{
					return -1L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public int CouplePoint
		{
			get
			{
				int num = __p.__offset(22);
				if (num == 0)
				{
					return -1;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public static void ValidateVersion()
		{
			FlatBufferConstants.FLATBUFFERS_1_12_0();
		}

		public static LoginCoupleInfo GetRootAsLoginCoupleInfo(ByteBuffer _bb)
		{
			return GetRootAsLoginCoupleInfo(_bb, default(LoginCoupleInfo));
		}

		public static LoginCoupleInfo GetRootAsLoginCoupleInfo(ByteBuffer _bb, LoginCoupleInfo obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginCoupleInfo __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public ArraySegment<byte>? GetMateNameBytes()
		{
			return __p.__vector_as_arraysegment(8);
		}

		public byte[] GetMateNameArray()
		{
			return __p.__vector_as_array<byte>(8);
		}

		public static Offset<LoginCoupleInfo> CreateLoginCoupleInfo(FlatBufferBuilder builder, int CoupleNum = 0, int CoupleType = 0, StringOffset MateNameOffset = default(StringOffset), long CreateTime = 0L, long MarriedTime = 0L, long RingChangedTime = 0L, int CoupleRingNum = 0, int CondDays = 0, long CoupleEXP = -1L, int CouplePoint = -1, short CoupleLevel = -1, int MaxRingDays = -1)
		{
			builder.StartTable(10);
			AddCoupleEXP(builder, CoupleEXP);
			AddRingChangedTime(builder, RingChangedTime);
			AddMarriedTime(builder, MarriedTime);
			AddCreateTime(builder, CreateTime);
			AddCouplePoint(builder, CouplePoint);
			AddCondDays(builder, CondDays);
			builder.AddShort(0);
			builder.AddShort(CoupleLevel);
			builder.AddInt(MaxRingDays);
			AddCoupleRingNum(builder, CoupleRingNum);
			AddMateName(builder, MateNameOffset);
			AddCoupleType(builder, CoupleType);
			AddCoupleNum(builder, CoupleNum);
			return EndLoginCoupleInfo(builder);
		}

		public static void StartLoginCoupleInfo(FlatBufferBuilder builder)
		{
			builder.StartTable(10);
		}

		public static void AddCoupleNum(FlatBufferBuilder builder, int CoupleNum)
		{
			builder.AddInt(0, CoupleNum, 0);
		}

		public static void AddCoupleType(FlatBufferBuilder builder, int CoupleType)
		{
			builder.AddInt(1, CoupleType, 0);
		}

		public static void AddMateName(FlatBufferBuilder builder, StringOffset MateNameOffset)
		{
			builder.AddOffset(2, MateNameOffset.Value, 0);
		}

		public static void AddCreateTime(FlatBufferBuilder builder, long CreateTime)
		{
			builder.AddLong(3, CreateTime, 0L);
		}

		public static void AddMarriedTime(FlatBufferBuilder builder, long MarriedTime)
		{
			builder.AddLong(4, MarriedTime, 0L);
		}

		public static void AddRingChangedTime(FlatBufferBuilder builder, long RingChangedTime)
		{
			builder.AddLong(5, RingChangedTime, 0L);
		}

		public static void AddCoupleRingNum(FlatBufferBuilder builder, int CoupleRingNum)
		{
			builder.AddInt(6, CoupleRingNum, 0);
		}

		public static void AddCondDays(FlatBufferBuilder builder, int CondDays)
		{
			builder.AddInt(7, CondDays, 0);
		}

		public static void AddCoupleEXP(FlatBufferBuilder builder, long CoupleEXP)
		{
			builder.AddLong(8, CoupleEXP, -1L);
		}

		public static void AddCouplePoint(FlatBufferBuilder builder, int CouplePoint)
		{
			builder.AddInt(9, CouplePoint, -1);
		}

		public static Offset<LoginCoupleInfo> EndLoginCoupleInfo(FlatBufferBuilder builder)
		{
			return new Offset<LoginCoupleInfo>(builder.EndTable());
		}
	}
}
