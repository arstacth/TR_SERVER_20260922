using System;
using FlatBuffers;

namespace FB.LoginUserInfo
{
	public struct LoginUserInfo : IFlatbufferObject
	{
		private Table __p;

		public ByteBuffer ByteBuffer => __p.bb;

		public int Session
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

		public short Unk1
		{
			get
			{
				int num = __p.__offset(6);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetShort(num + __p.bb_pos);
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

		public long TR
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

		public long EXP
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

		public int Attribute
		{
			get
			{
				int num = __p.__offset(14);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Playingtime
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

		public LoginCoupleInfo? Usercoupleinfo
		{
			get
			{
				int num = __p.__offset(18);
				if (num == 0)
				{
					return null;
				}
				return default(LoginCoupleInfo).__assign(__p.__indirect(num + __p.bb_pos), __p.bb);
			}
		}

		public int Unk3
		{
			get
			{
				int num = __p.__offset(20);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk4
		{
			get
			{
				int num = __p.__offset(22);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public long LoginTime
		{
			get
			{
				int num = __p.__offset(24);
				if (num == 0)
				{
					return 0L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public int UserudpinfoLength
		{
			get
			{
				int num = __p.__offset(26);
				if (num == 0)
				{
					return 0;
				}
				return __p.__vector_len(num);
			}
		}

		public sbyte Unk5
		{
			get
			{
				int num = __p.__offset(28);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetSbyte(num + __p.bb_pos);
			}
		}

		public int Unk6
		{
			get
			{
				int num = __p.__offset(30);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public sbyte Unk7
		{
			get
			{
				int num = __p.__offset(32);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetSbyte(num + __p.bb_pos);
			}
		}

		public int Unk8
		{
			get
			{
				int num = __p.__offset(34);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk9
		{
			get
			{
				int num = __p.__offset(36);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk10
		{
			get
			{
				int num = __p.__offset(38);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int GameOption
		{
			get
			{
				int num = __p.__offset(40);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int ShuMp
		{
			get
			{
				int num = __p.__offset(42);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int Unk11
		{
			get
			{
				int num = __p.__offset(44);
				if (num == 0)
				{
					return 0;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public long FreePassType
		{
			get
			{
				int num = __p.__offset(46);
				if (num == 0)
				{
					return -1L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public long FreePassType2
		{
			get
			{
				int num = __p.__offset(48);
				if (num == 0)
				{
					return -1L;
				}
				return __p.bb.GetLong(num + __p.bb_pos);
			}
		}

		public int Unk12
		{
			get
			{
				int num = __p.__offset(50);
				if (num == 0)
				{
					return -1;
				}
				return __p.bb.GetInt(num + __p.bb_pos);
			}
		}

		public int TopRank
		{
			get
			{
				int num = __p.__offset(52);
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

		public static LoginUserInfo GetRootAsLoginUserInfo(ByteBuffer _bb)
		{
			return GetRootAsLoginUserInfo(_bb, default(LoginUserInfo));
		}

		public static LoginUserInfo GetRootAsLoginUserInfo(ByteBuffer _bb, LoginUserInfo obj)
		{
			return obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb);
		}

		public void __init(int _i, ByteBuffer _bb)
		{
			__p = new Table(_i, _bb);
		}

		public LoginUserInfo __assign(int _i, ByteBuffer _bb)
		{
			__init(_i, _bb);
			return this;
		}

		public byte Userudpinfo(int j)
		{
			int num = __p.__offset(26);
			if (num == 0)
			{
				return 0;
			}
			return __p.bb.Get(__p.__vector(num) + j);
		}

		public ArraySegment<byte>? GetUserudpinfoBytes()
		{
			return __p.__vector_as_arraysegment(26);
		}

		public byte[] GetUserudpinfoArray()
		{
			return __p.__vector_as_array<byte>(26);
		}

		public static Offset<LoginUserInfo> CreateLoginUserInfo(FlatBufferBuilder builder, int session = 0, short unk1 = 0, int unk2 = 0, long TR = 0L, long EXP = 0L, int Attribute = 0, int playingtime = 0, Offset<LoginCoupleInfo> usercoupleinfoOffset = default(Offset<LoginCoupleInfo>), int unk3 = 0, int unk4 = 0, long loginTime = 0L, VectorOffset userudpinfoOffset = default(VectorOffset), sbyte unk5 = 0, int unk6 = 0, sbyte unk7 = 0, int unk8 = 0, int unk9 = 0, int unk10 = 0, int GameOption = 0, int shuMp = 0, int unk11 = 0, long FreePassType = -1L, long FreePassType2 = -1L, int unk12 = -1, int TopRank = 0)
		{
			builder.StartTable(25);
			AddFreePassType2(builder, FreePassType2);
			AddFreePassType(builder, FreePassType);
			AddLoginTime(builder, loginTime);
			AddEXP(builder, EXP);
			AddTR(builder, TR);
			AddTopRank(builder, TopRank);
			AddUnk12(builder, unk12);
			AddUnk11(builder, unk11);
			AddShuMp(builder, shuMp);
			AddGameOption(builder, GameOption);
			AddUnk10(builder, unk10);
			AddUnk9(builder, unk9);
			AddUnk8(builder, unk8);
			AddUnk6(builder, unk6);
			AddUserudpinfo(builder, userudpinfoOffset);
			AddUnk4(builder, unk4);
			AddUnk3(builder, unk3);
			AddUsercoupleinfo(builder, usercoupleinfoOffset);
			AddPlayingtime(builder, playingtime);
			AddAttribute(builder, Attribute);
			AddUnk2(builder, unk2);
			AddSession(builder, session);
			AddUnk1(builder, unk1);
			AddUnk7(builder, unk7);
			AddUnk5(builder, unk5);
			return EndLoginUserInfo(builder);
		}

		public static void StartLoginUserInfo(FlatBufferBuilder builder)
		{
			builder.StartTable(25);
		}

		public static void AddSession(FlatBufferBuilder builder, int session)
		{
			builder.AddInt(0, session, 0);
		}

		public static void AddUnk1(FlatBufferBuilder builder, short unk1)
		{
			builder.AddShort(1, unk1, 0);
		}

		public static void AddUnk2(FlatBufferBuilder builder, int unk2)
		{
			builder.AddInt(2, unk2, 0);
		}

		public static void AddTR(FlatBufferBuilder builder, long TR)
		{
			builder.AddLong(3, TR, 0L);
		}

		public static void AddEXP(FlatBufferBuilder builder, long EXP)
		{
			builder.AddLong(4, EXP, 0L);
		}

		public static void AddAttribute(FlatBufferBuilder builder, int Attribute)
		{
			builder.AddInt(5, Attribute, 0);
		}

		public static void AddPlayingtime(FlatBufferBuilder builder, int playingtime)
		{
			builder.AddInt(6, playingtime, 0);
		}

		public static void AddUsercoupleinfo(FlatBufferBuilder builder, Offset<LoginCoupleInfo> usercoupleinfoOffset)
		{
			builder.AddOffset(7, usercoupleinfoOffset.Value, 0);
		}

		public static void AddUnk3(FlatBufferBuilder builder, int unk3)
		{
			builder.AddInt(8, unk3, 0);
		}

		public static void AddUnk4(FlatBufferBuilder builder, int unk4)
		{
			builder.AddInt(9, unk4, 0);
		}

		public static void AddLoginTime(FlatBufferBuilder builder, long loginTime)
		{
			builder.AddLong(10, loginTime, 0L);
		}

		public static void AddUserudpinfo(FlatBufferBuilder builder, VectorOffset userudpinfoOffset)
		{
			builder.AddOffset(11, userudpinfoOffset.Value, 0);
		}

		public static VectorOffset CreateUserudpinfoVector(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			for (int num = data.Length - 1; num >= 0; num--)
			{
				builder.AddByte(data[num]);
			}
			return builder.EndVector();
		}

		public static VectorOffset CreateUserudpinfoVectorBlock(FlatBufferBuilder builder, byte[] data)
		{
			builder.StartVector(1, data.Length, 1);
			builder.Add(data);
			return builder.EndVector();
		}

		public static void StartUserudpinfoVector(FlatBufferBuilder builder, int numElems)
		{
			builder.StartVector(1, numElems, 1);
		}

		public static void AddUnk5(FlatBufferBuilder builder, sbyte unk5)
		{
			builder.AddSbyte(12, unk5, 0);
		}

		public static void AddUnk6(FlatBufferBuilder builder, int unk6)
		{
			builder.AddInt(13, unk6, 0);
		}

		public static void AddUnk7(FlatBufferBuilder builder, sbyte unk7)
		{
			builder.AddSbyte(14, unk7, 0);
		}

		public static void AddUnk8(FlatBufferBuilder builder, int unk8)
		{
			builder.AddInt(15, unk8, 0);
		}

		public static void AddUnk9(FlatBufferBuilder builder, int unk9)
		{
			builder.AddInt(16, unk9, 0);
		}

		public static void AddUnk10(FlatBufferBuilder builder, int unk10)
		{
			builder.AddInt(17, unk10, 0);
		}

		public static void AddGameOption(FlatBufferBuilder builder, int GameOption)
		{
			builder.AddInt(18, GameOption, 0);
		}

		public static void AddShuMp(FlatBufferBuilder builder, int shuMp)
		{
			builder.AddInt(19, shuMp, 0);
		}

		public static void AddUnk11(FlatBufferBuilder builder, int unk11)
		{
			builder.AddInt(20, unk11, 0);
		}

		public static void AddFreePassType(FlatBufferBuilder builder, long FreePassType)
		{
			builder.AddLong(21, FreePassType, -1L);
		}

		public static void AddFreePassType2(FlatBufferBuilder builder, long FreePassType2)
		{
			builder.AddLong(22, FreePassType2, -1L);
		}

		public static void AddUnk12(FlatBufferBuilder builder, int unk12)
		{
			builder.AddInt(23, unk12, -1);
		}

		public static void AddTopRank(FlatBufferBuilder builder, int TopRank)
		{
			builder.AddInt(24, TopRank, 0);
		}

		public static Offset<LoginUserInfo> EndLoginUserInfo(FlatBufferBuilder builder)
		{
			return new Offset<LoginUserInfo>(builder.EndTable());
		}

		public static void FinishLoginUserInfoBuffer(FlatBufferBuilder builder, Offset<LoginUserInfo> offset)
		{
			builder.Finish(offset.Value);
		}

		public static void FinishSizePrefixedLoginUserInfoBuffer(FlatBufferBuilder builder, Offset<LoginUserInfo> offset)
		{
			builder.FinishSizePrefixed(offset.Value);
		}
	}
}
