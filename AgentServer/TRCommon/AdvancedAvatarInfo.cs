using System.Collections.Generic;

namespace TRCommon
{
	public class AdvancedAvatarInfo
	{
		public AvatarInfo m_realAvatarInfo;

		public AvatarInfo m_costumeAvatarInfo;

		public bool m_bIsUseCostume;

		public bool isValidCharacter
		{
			get
			{
				int real = (int)m_realAvatarInfo.m_character;
				int costume = (int)m_costumeAvatarInfo.m_character;
				return costume == 0 || real == costume;
			}
		}

		public cpk_type getRealCharacter => m_realAvatarInfo.m_character;

		public AvatarInfo getRealAvatarInfo => m_realAvatarInfo;

		public bool isUseCostume => m_bIsUseCostume;

		public cpk_type getRealGHead => m_realAvatarInfo.m_ghead;

		public cpk_type getCostumeGHead
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_ghead;
				}
				return getRealGHead;
			}
		}

		public cpk_type getRealGaccHand => m_realAvatarInfo.m_gaccHand;

		public cpk_type getCostumeGaccHand
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gaccHand;
				}
				return getRealGaccHand;
			}
		}

		public cpk_type getRealGaccFoot => m_realAvatarInfo.m_gaccFoot;

		public cpk_type getCostumeGaccFoot
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gaccFoot;
				}
				return getRealGaccFoot;
			}
		}

		public cpk_type getRealGTopBody => m_realAvatarInfo.m_gTopBody;

		public cpk_type getCostumeGTopBody
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gTopBody;
				}
				return getRealGTopBody;
			}
		}

		public cpk_type getRealGaccPose => m_realAvatarInfo.m_gaccPose;

		public cpk_type getCostumeGaccPose
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gaccPose;
				}
				return getRealGaccPose;
			}
		}

		public cpk_type getRealGaccBack => m_realAvatarInfo.m_gaccBack;

		public cpk_type getCostumeGaccBack
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gaccBack;
				}
				return getRealGaccBack;
			}
		}

		public cpk_type getRealGaccEvent => m_realAvatarInfo.m_gaccEvent;

		public cpk_type getCostumeGaccEvent
		{
			get
			{
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo.m_gaccEvent;
				}
				return getRealGaccEvent;
			}
		}

		public bool isPieroAvatar => (int)getRealCharacter == 101;

		public AdvancedAvatarInfo()
		{
			m_realAvatarInfo = default(AvatarInfo);
			m_costumeAvatarInfo = default(AvatarInfo);
			m_bIsUseCostume = false;
		}

		public AdvancedAvatarInfo(AdvancedAvatarInfo clone)
		{
			m_realAvatarInfo = clone.m_realAvatarInfo;
			m_costumeAvatarInfo = clone.m_costumeAvatarInfo;
			m_bIsUseCostume = clone.m_bIsUseCostume;
		}

		public AdvancedAvatarInfo ShallowCopy()
		{
			return (AdvancedAvatarInfo)MemberwiseClone();
		}

		public void clear()
		{
			m_realAvatarInfo = default(AvatarInfo);
			m_costumeAvatarInfo = default(AvatarInfo);
			m_bIsUseCostume = false;
		}

		public void SetItemParts(bool costume, ushort[] parts)
		{
			if (parts == null || parts.Length < 15)
			{
				return;
			}
			if (costume)
			{
				for (int i = 0; i < 15; i++)
				{
					m_costumeAvatarInfo.m_nItemPartArry[i] = parts[i];
				}
			}
			else
			{
				for (int i = 0; i < 15; i++)
				{
					m_realAvatarInfo.m_nItemPartArry[i] = parts[i];
				}
			}
		}

		public void SetGameAccs(bool costume, ushort[] accs)
		{
			if (accs == null || accs.Length < 7)
			{
				return;
			}
			if (costume)
			{
				for (int i = 0; i < 7; i++)
				{
					m_costumeAvatarInfo.m_nGameAccArry[i] = accs[i];
				}
			}
			else
			{
				for (int i = 0; i < 7; i++)
				{
					m_realAvatarInfo.m_nGameAccArry[i] = accs[i];
				}
			}
		}

		public void SetEFItems(bool costume, ushort[] items)
		{
			if (items == null || items.Length < 1)
			{
				return;
			}
			if (costume)
			{
				m_costumeAvatarInfo.m_nEFItemArry[0] = items[0];
			}
			else
			{
				m_realAvatarInfo.m_nEFItemArry[0] = items[0];
			}
		}

		public void EnsureVisibleStarterClothes()
		{
			// Do not invent shop starter CPKs the user may not own.
			SanitizeCostumeOrShowReal();
		}

		public void SanitizeCostumeOrShowReal()
		{
			int realChar = (int)m_realAvatarInfo.m_character;
			if (realChar <= 0 || realChar == 65535)
			{
				m_bIsUseCostume = false;
			}
		}

		public cpk_type getCharacter(int avatarType)
		{
			return getAvatarInfo(avatarType).getItemPart(0);
		}

		public void setCharacter(cpk_type nChracter)
		{
			m_realAvatarInfo.setCharacter(nChracter);
			m_costumeAvatarInfo.setCharacter(nChracter);
		}

		public AvatarInfo getAvatarInfo(int avatarType)
		{
			switch (avatarType)
			{
			case 1:
				return m_realAvatarInfo;
			case 2:
				if (m_bIsUseCostume)
				{
					return m_costumeAvatarInfo;
				}
				return m_realAvatarInfo;
			default:
				return m_realAvatarInfo;
			}
		}

		public cpk_type getCostumeCharacter()
		{
			if (m_bIsUseCostume)
			{
				return m_costumeAvatarInfo.m_character;
			}
			return getRealCharacter;
		}

		public void setAvatarInfo(int avatarType, AvatarInfo info)
		{
			switch (avatarType)
			{
			case 1:
				m_realAvatarInfo = info;
				break;
			case 2:
				if (!m_bIsUseCostume)
				{
					m_realAvatarInfo = info;
				}
				else
				{
					m_costumeAvatarInfo = info;
				}
				break;
			}
		}

		public AvatarInfo getCostumeAvatarInfo(bool checkUseCostume = true)
		{
			if (!checkUseCostume)
			{
				return m_costumeAvatarInfo;
			}
			if (m_bIsUseCostume)
			{
				return m_costumeAvatarInfo;
			}
			return getRealAvatarInfo;
		}

		public void setRealAvatarInfo(AvatarInfo info)
		{
			m_realAvatarInfo = info;
		}

		public void setCostumeAvatarInfo(AvatarInfo info)
		{
			if (!m_bIsUseCostume)
			{
				setRealAvatarInfo(info);
			}
			else
			{
				m_costumeAvatarInfo = info;
			}
		}

		public cpk_type getItemPart(int avatarType, cpk_type nPart)
		{
			return getAvatarInfo(avatarType).getItemPart(nPart);
		}

		public void setItemPart(int avatarType, cpk_type nPart, cpk_type nKind)
		{
			switch (avatarType)
			{
			case 1:
				m_realAvatarInfo.setItemPart(nPart, nKind);
				break;
			case 2:
				if (!m_bIsUseCostume)
				{
					m_realAvatarInfo.setItemPart(nPart, nKind);
				}
				else
				{
					m_costumeAvatarInfo.setItemPart(nPart, nKind);
				}
				break;
			}
		}

		public cpk_type getRealItemPart(cpk_type nPart)
		{
			return m_realAvatarInfo.getItemPart(nPart);
		}

		public cpk_type getCostumeItemPart(cpk_type nPart)
		{
			if (m_bIsUseCostume)
			{
				return m_costumeAvatarInfo.getItemPart(nPart);
			}
			return getRealItemPart(nPart);
		}

		public void setRealItemPart(cpk_type nPart, cpk_type nKind)
		{
			m_realAvatarInfo.setItemPart(nPart, nKind);
		}

		public void setCostumeItemPart(cpk_type nPart, cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealItemPart(nPart, nKind);
			}
			else
			{
				m_costumeAvatarInfo.setItemPart(nPart, nKind);
			}
		}

		public void setUseCostume(bool bUse)
		{
			m_bIsUseCostume = bUse;
		}

		public cpk_type getGameAcc(int avatarType, cpk_type gAcc)
		{
			return getAvatarInfo(avatarType).getGameAcc(gAcc);
		}

		public void setGameAcc(int avatarType, cpk_type gAcc, cpk_type nKind)
		{
			switch (avatarType)
			{
			case 1:
				m_realAvatarInfo.setGameAcc(gAcc, nKind);
				break;
			case 2:
				if (!m_bIsUseCostume)
				{
					m_realAvatarInfo.setGameAcc(gAcc, nKind);
				}
				else
				{
					m_costumeAvatarInfo.setGameAcc(gAcc, nKind);
				}
				break;
			}
		}

		public cpk_type getEFItem(int avatarType, cpk_type gAcc)
		{
			return getAvatarInfo(avatarType).getEFItem(gAcc);
		}

		public void setEFItem(int avatarType, cpk_type gAcc, cpk_type nKind)
		{
			switch (avatarType)
			{
			case 1:
				m_realAvatarInfo.setEFItem(gAcc, nKind);
				break;
			case 2:
				if (!m_bIsUseCostume)
				{
					m_realAvatarInfo.setEFItem(gAcc, nKind);
				}
				else
				{
					m_costumeAvatarInfo.setEFItem(gAcc, nKind);
				}
				break;
			}
		}

		public void setRealGHead(cpk_type nKind)
		{
			m_realAvatarInfo.m_ghead = nKind;
		}

		public void setCostumeGHead(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGHead(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_ghead = nKind;
			}
		}

		public void setRealGaccHand(cpk_type nKind)
		{
			m_realAvatarInfo.m_gaccHand = nKind;
		}

		public void setCostumeGaccHand(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGaccHand(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gaccHand = nKind;
			}
		}

		public void setRealGaccFoot(cpk_type nKind)
		{
			m_realAvatarInfo.m_gaccFoot = nKind;
		}

		public void setCostumeGaccFoot(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGaccFoot(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gaccFoot = nKind;
			}
		}

		public void setRealGTopBody(cpk_type nKind)
		{
			m_realAvatarInfo.m_gTopBody = nKind;
		}

		public void setCostumeGTopBody(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGTopBody(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gTopBody = nKind;
			}
		}

		public void setRealGaccPose(cpk_type nKind)
		{
			m_realAvatarInfo.m_gaccPose = nKind;
		}

		public void setCostumeGaccPose(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGaccPose(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gaccPose = nKind;
			}
		}

		public void setRealGaccBack(cpk_type nKind)
		{
			m_realAvatarInfo.m_gaccBack = nKind;
		}

		public void setCostumeGaccBack(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGaccBack(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gaccBack = nKind;
			}
		}

		public void setRealGaccEvent(cpk_type nKind)
		{
			m_realAvatarInfo.m_gaccEvent = nKind;
		}

		public void setCostumeGaccEvent(cpk_type nKind)
		{
			if (!m_bIsUseCostume)
			{
				setRealGaccEvent(nKind);
			}
			else
			{
				m_costumeAvatarInfo.m_gaccEvent = nKind;
			}
		}

		public bool isCostumeGaccUseCheck(ITEM_POSITION_GACC eGacc, cpk_type nKind)
		{
			return (int)nKind == (int)getGameAcc(2, (int)eGacc);
		}

		public bool isWearThisItem(cpk_type nPosition, cpk_type nKind)
		{
			if (m_realAvatarInfo.isWearThisItem(nPosition, nKind) || m_costumeAvatarInfo.isWearThisItem(nPosition, nKind))
			{
				return true;
			}
			return false;
		}

		public bool isWearThisItemAtRealPart(cpk_type nPosition, cpk_type nKind)
		{
			return (checkWearThisItem(nPosition, nKind) & eAvatarWearStateBit.eAvatarWearStateBit_REAL_ONLY) > eAvatarWearStateBit.eAvatarWearStateBit_NONE;
		}

		public eAvatarWearStateBit checkWearThisItem(cpk_type nPosition, cpk_type nKind)
		{
			return (m_realAvatarInfo.isWearThisItem(nPosition, nKind) ? eAvatarWearStateBit.eAvatarWearStateBit_REAL_ONLY : eAvatarWearStateBit.eAvatarWearStateBit_NONE) | (m_costumeAvatarInfo.isWearThisItem(nPosition, nKind) ? eAvatarWearStateBit.eAvatarWearStateBit_REAL_ONLY : eAvatarWearStateBit.eAvatarWearStateBit_NONE);
		}

		public static bool operator ==(AdvancedAvatarInfo lhs, AdvancedAvatarInfo rhs)
		{
			if (object.ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			if (object.ReferenceEquals(lhs, null) || object.ReferenceEquals(rhs, null))
			{
				return false;
			}
			if (lhs.m_realAvatarInfo == rhs.m_realAvatarInfo && lhs.m_costumeAvatarInfo == rhs.m_costumeAvatarInfo && lhs.m_bIsUseCostume == rhs.m_bIsUseCostume)
			{
				return true;
			}
			return false;
		}

		public static bool operator !=(AdvancedAvatarInfo lhs, AdvancedAvatarInfo rhs)
		{
			return !(lhs == rhs);
		}

		public List<AvatarEquipmentStatus> getEquipmentStatus(AdvancedAvatarInfo rhs)
		{
			List<AvatarEquipmentStatus> list = new List<AvatarEquipmentStatus>();
			AvatarEquipmentStatus item = default(AvatarEquipmentStatus);
			if ((int)m_realAvatarInfo.m_character != (int)rhs.m_realAvatarInfo.m_character)
			{
				return list;
			}
			cpk_type cpk_type2 = 1;
			while ((int)cpk_type2 < 15)
			{
				if (m_realAvatarInfo.GetWear((int)cpk_type2) != rhs.m_realAvatarInfo.GetWear((int)cpk_type2))
				{
					if (0 >= m_realAvatarInfo.GetWear((int)cpk_type2))
					{
						item.m_bEquipment = true;
						item.m_bCostume = false;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = rhs.m_realAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
					else if (0 >= rhs.m_realAvatarInfo.GetWear((int)cpk_type2))
					{
						item.m_bEquipment = false;
						item.m_bCostume = false;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = m_realAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
					else
					{
						item.m_bEquipment = true;
						item.m_bCostume = false;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = rhs.m_realAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
						item.m_bEquipment = false;
						item.m_bCostume = false;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = m_realAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
				}
				if (m_costumeAvatarInfo.GetWear((int)cpk_type2) != rhs.m_costumeAvatarInfo.GetWear((int)cpk_type2))
				{
					if (0 >= m_costumeAvatarInfo.GetWear((int)cpk_type2))
					{
						item.m_bEquipment = true;
						item.m_bCostume = true;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = rhs.m_costumeAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
					else if (0 >= rhs.m_costumeAvatarInfo.GetWear((int)cpk_type2))
					{
						item.m_bEquipment = false;
						item.m_bCostume = true;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = m_costumeAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
					else
					{
						item.m_bEquipment = true;
						item.m_bCostume = true;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = rhs.m_costumeAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
						item.m_bEquipment = false;
						item.m_bCostume = true;
						item.m_character = m_realAvatarInfo.m_character;
						item.m_position = cpk_type2;
						item.m_kind = m_costumeAvatarInfo.GetWear((int)cpk_type2);
						list.Add(item);
					}
				}
				cpk_type2 = (short)((short)cpk_type2 + 1);
			}
			return list;
		}
	}
}
