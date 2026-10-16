using System.Runtime.InteropServices;

namespace TRCommon
{
	[StructLayout(LayoutKind.Explicit)]
	public struct AvatarInfo
	{
		[FieldOffset(0)]
		public cpk_type m_character;

		[FieldOffset(2)]
		public cpk_type m_head;

		[FieldOffset(4)]
		public cpk_type m_topBody;

		[FieldOffset(6)]
		public cpk_type m_downBody;

		[FieldOffset(8)]
		public cpk_type m_foot;

		[FieldOffset(10)]
		public cpk_type m_acHead;

		[FieldOffset(12)]
		public cpk_type m_acFace;

		[FieldOffset(14)]
		public cpk_type m_acHand;

		[FieldOffset(16)]
		public cpk_type m_acBack;

		[FieldOffset(18)]
		public cpk_type m_acNeck;

		[FieldOffset(20)]
		public cpk_type m_pet;

		[FieldOffset(22)]
		public cpk_type m_expansion;

		[FieldOffset(24)]
		public cpk_type m_acWrist;

		[FieldOffset(26)]
		public cpk_type m_acBooster;

		[FieldOffset(28)]
		public cpk_type m_accTail;

		[FieldOffset(0)]
		[MarshalAs(UnmanagedType.Struct)]
		public ItemPartArry m_nItemPartArry;

		[FieldOffset(30)]
		public cpk_type m_ghead;

		[FieldOffset(32)]
		public cpk_type m_gaccHand;

		[FieldOffset(34)]
		public cpk_type m_gaccFoot;

		[FieldOffset(36)]
		public cpk_type m_gTopBody;

		[FieldOffset(38)]
		public cpk_type m_gaccPose;

		[FieldOffset(40)]
		public cpk_type m_gaccBack;

		[FieldOffset(42)]
		public cpk_type m_gaccEvent;

		[FieldOffset(30)]
		[MarshalAs(UnmanagedType.Struct)]
		public GameAccArry m_nGameAccArry;

		[FieldOffset(44)]
		public cpk_type m_efHeadBack;

		[FieldOffset(44)]
		[MarshalAs(UnmanagedType.Struct)]
		public EFItemArry m_nEFItemArry;

		public bool isPieroAvatar => (int)m_character == 101;

		public AvatarInfo(int i)
		{
			m_character = 0;
			m_head = 0;
			m_topBody = 0;
			m_downBody = 0;
			m_foot = 0;
			m_acHead = 0;
			m_acFace = 0;
			m_acHand = 0;
			m_acBack = 0;
			m_acNeck = 0;
			m_pet = 0;
			m_expansion = 0;
			m_acWrist = 0;
			m_acBooster = 0;
			m_accTail = 0;
			m_ghead = 0;
			m_gaccHand = 0;
			m_gaccFoot = 0;
			m_gTopBody = 0;
			m_gaccPose = 0;
			m_gaccBack = 0;
			m_gaccEvent = 0;
			m_efHeadBack = 0;
		}

		public void setCharacter(cpk_type nChar)
		{
			m_character = nChar;
		}

		public ushort GetWear(int i)
		{
			if (i < 0 || i >= 15)
			{
				return 0;
			}
			return m_nItemPartArry[i];
		}

		public void SetWear(int i, ushort v)
		{
			if (i < 0 || i >= 15)
			{
				return;
			}
			m_nItemPartArry[i] = v;
		}

		public ushort GetAcc(int i)
		{
			if (i < 0 || i >= 7)
			{
				return 0;
			}
			return m_nGameAccArry[i];
		}

		public void SetAcc(int i, ushort v)
		{
			if (i < 0 || i >= 7)
			{
				return;
			}
			m_nGameAccArry[i] = v;
		}

		public ushort GetEF()
		{
			return m_nEFItemArry[0];
		}

		public void SetEF(ushort v)
		{
			m_nEFItemArry[0] = v;
		}

		public void setItemPart(cpk_type nPart, cpk_type nKind)
		{
			cpk_type cpk_type2 = NetCommonFunc.getAvatarPartsByItemPosition(nPart);
			if (NetCommonFunc.isWearItemPosition(cpk_type2))
			{
				m_nItemPartArry[cpk_type2] = nKind;
			}
		}

		public cpk_type getItemPart(cpk_type nPart)
		{
			if (NetCommonFunc.isWearItemPosition(nPart))
			{
				return m_nItemPartArry[nPart];
			}
			return ushort.MaxValue;
		}

		public void setGameAcc(cpk_type gAcc, cpk_type nKind)
		{
			m_nGameAccArry[gAcc] = nKind;
		}

		public cpk_type getGameAcc(cpk_type gAcc)
		{
			if ((int)gAcc >= 0 && (int)gAcc < 7)
			{
				return m_nGameAccArry[gAcc];
			}
			return ushort.MaxValue;
		}

		public void setEFItem(cpk_type effect, cpk_type nKind)
		{
			m_nEFItemArry[effect] = nKind;
		}

		public cpk_type getEFItem(cpk_type effect)
		{
			if ((int)effect >= 0 && (int)effect < 1)
			{
				return m_nEFItemArry[effect];
			}
			return ushort.MaxValue;
		}

		public bool isWearThisItem(cpk_type nPosition, cpk_type nKind)
		{
			cpk_type cpk_type2 = NetCommonFunc.getAvatarPartsByItemPosition(nPosition);
			if (!NetCommonFunc.isWearItemPosition(cpk_type2))
			{
				return false;
			}
			int idx = (int)cpk_type2;
			if (idx < 0 || idx >= 15)
			{
				return false;
			}
			return (int)nKind == m_nItemPartArry[idx];
		}

		public static bool operator ==(AvatarInfo lhs, AvatarInfo rhs)
		{
			for (byte b = 0; b < 15; b = (byte)(b + 1))
			{
				if (lhs.m_nItemPartArry[b] != rhs.m_nItemPartArry[b])
				{
					return false;
				}
			}
			for (byte b2 = 0; b2 < 7; b2 = (byte)(b2 + 1))
			{
				if (lhs.m_nGameAccArry[b2] != rhs.m_nGameAccArry[b2])
				{
					return false;
				}
			}
			for (byte b3 = 0; b3 < 1; b3 = (byte)(b3 + 1))
			{
				if (lhs.m_nEFItemArry[b3] != rhs.m_nEFItemArry[b3])
				{
					return false;
				}
			}
			return true;
		}

		public static bool operator !=(AvatarInfo lhs, AvatarInfo rhs)
		{
			for (byte b = 0; b < 15; b = (byte)(b + 1))
			{
				if (lhs.m_nItemPartArry[b] != rhs.m_nItemPartArry[b])
				{
					return true;
				}
			}
			for (byte b2 = 0; b2 < 7; b2 = (byte)(b2 + 1))
			{
				if (lhs.m_nGameAccArry[b2] != rhs.m_nGameAccArry[b2])
				{
					return true;
				}
			}
			for (byte b3 = 0; b3 < 1; b3 = (byte)(b3 + 1))
			{
				if (lhs.m_nEFItemArry[b3] != rhs.m_nEFItemArry[b3])
				{
					return true;
				}
			}
			return false;
		}
	}
}
