using System.Collections.Generic;

namespace TRCommon
{
	public class CharAbilityAttr
	{
		private static readonly int ITEM_POSITION_FOR_SET_ITEM_ATTR = 50;

		public CItemAttr m_characterAttr;

		private CPropertyBuildManager m_propertyBuildManager;

		public CharAbilityAttr()
		{
			m_characterAttr = new CItemAttr();
			m_propertyBuildManager = new CPropertyBuildManager();
		}

		public void makeAttrFrom(AdvancedAvatarInfo avatarInfo, CActiveItems funcItems, CUserItemAttrManager userItemAttr, CAvatarLock avatarLock)
		{
			m_propertyBuildManager.clear();
			m_characterAttr.clear();
			bool flag = false;
			AvatarInfo info;
			AvatarInfo info2;
			if (avatarLock.isValid)
			{
				flag = avatarLock.isUseCostume;
				avatarLock.getAvatarInfo(AVATAR.AVATAR_REAL, out info);
				avatarLock.getAvatarInfo(AVATAR.AVATAR_COSTUME, out info2);
			}
			else
			{
				flag = avatarInfo.m_bIsUseCostume;
				info = avatarInfo.getAvatarInfo(1);
				info2 = avatarInfo.getAvatarInfo(2);
			}
			_checkAvatarInfo(info, 1, userItemAttr);
			if (flag)
			{
				_checkAvatarInfo(info2, 2, userItemAttr);
			}
			Dictionary<int, bool> mapGroupItemNum = new Dictionary<int, bool>();
			Dictionary<int, bool> mapGroupItemNum2 = new Dictionary<int, bool>();
			SetItemDescTableEx setItemDescTableEx = new SetItemDescTableEx();
			if (setItemDescTableEx.makeSetItemAbilities(info, out mapGroupItemNum))
			{
				setItemDescTableEx.parseSetItem(mapGroupItemNum2);
				if (setItemDescTableEx.getAllItemAbilities(out var abilities))
				{
					m_propertyBuildManager.addProperty(new CPropertyCheckSource(abilities, ITEM_POSITION_FOR_SET_ITEM_ATTR));
				}
			}
			if (flag && setItemDescTableEx.makeSetItemAbilities(info2, out mapGroupItemNum2))
			{
				setItemDescTableEx.parseSetItem(mapGroupItemNum, bCostume: true);
				if (setItemDescTableEx.getAllItemAbilities(out var abilities2))
				{
					m_propertyBuildManager.addProperty(new CPropertyCheckSource(abilities2, ITEM_POSITION_FOR_SET_ITEM_ATTR));
				}
			}
			List<CPropertyCheckSource> vecPropertyCheckSource = new List<CPropertyCheckSource>();
			funcItems.getPropertyCheckSource(userItemAttr, avatarInfo.getRealCharacter, out vecPropertyCheckSource);
			m_propertyBuildManager.addProperty(vecPropertyCheckSource);
			m_characterAttr = m_propertyBuildManager.getResultAttr();
			m_characterAttr.m_attr.applyLimit(ShopItemTable.getAttrLimit);
		}

		private void _checkAvatarInfo(AvatarInfo avatarInfo, short avatarType, CUserItemAttrManager userItemAttr)
		{
			if (avatarType == 1)
			{
				if ((int)avatarInfo.m_character == 101)
				{
					_addItemAttr(1, 0, 0, avatarType, userItemAttr);
				}
				else
				{
					_addItemAttr(avatarInfo.m_character, 0, 0, avatarType, userItemAttr);
				}
			}
			cpk_type cpk_type2 = (ushort)1;
			while ((int)cpk_type2 < 15)
			{
				_addItemAttr(avatarInfo.m_character, cpk_type2, avatarInfo.getItemPart(cpk_type2), avatarType, userItemAttr);
				cpk_type2 = (short)((short)cpk_type2 + 1);
			}
		}

		private void addItemAttr(int iItemDescNum, cpk_type position, short avatarType, CUserItemAttrManager userItemAttr)
		{
			CItemAttr rAttr;
			if (ShopItemTable.isEnchantItem(iItemDescNum))
			{
				if (userItemAttr.getCharAttr(iItemDescNum, out rAttr))
				{
					m_propertyBuildManager.addProperty(new CPropertyCheckSource(rAttr, position, avatarType));
					return;
				}
				userItemAttr.getItemAttr(iItemDescNum, out rAttr);
				if (ItemAttrTable.getItemAttrFromItemDescNum(iItemDescNum, out var it) && it != null)
				{
					if (rAttr == null)
					{
						rAttr = new CItemAttr();
					}
					rAttr += it;
					m_propertyBuildManager.addProperty(new CPropertyCheckSource(rAttr, position, avatarType));
				}
			}
			else if (userItemAttr.getCharAttr(iItemDescNum, out rAttr))
			{
				m_propertyBuildManager.addProperty(new CPropertyCheckSource(rAttr, position, avatarType));
			}
			else if (userItemAttr.getItemAttr(iItemDescNum, out rAttr))
			{
				m_propertyBuildManager.addProperty(new CPropertyCheckSource(rAttr, position, avatarType));
			}
			else
			{
				if (ItemAttrTable.getItemAttrFromItemDescNum(iItemDescNum, out var it2) && it2 != null)
				{
					if (rAttr == null)
					{
						rAttr = new CItemAttr();
					}
					rAttr += it2;
					m_propertyBuildManager.addProperty(new CPropertyCheckSource(rAttr, position, avatarType));
				}
			}
		}

		private void _addItemAttr(cpk_type character, cpk_type position, cpk_type kind, short avatarType, CUserItemAttrManager userItemAttr)
		{
			cpk_type cpk_type2 = (int)kind % 10000;
			if ((int)cpk_type2 > 2000 && (int)cpk_type2 < 10000)
			{
				character = 0;
			}
			if ((int)position == 10 && (int)kind >= 10000)
			{
				character = 0;
			}
			if ((int)position == 11 && (int)kind > 1000)
			{
				character = 0;
			}
			if ((int)position == 2 && (int)kind >= 1000 && (int)kind < 2000)
			{
				position = (ushort)106;
				if (ShopItemTable.getItemDescNumFromCPK(character, position, kind) == -1)
				{
					character = 0;
				}
			}
			else if ((int)position == 2 && (int)kind >= 30000 && (int)kind < 40000)
			{
				character = 0;
				position = (ushort)117;
			}
			bool flag = false;
			if ((int)character != 0 && (int)position == 0 && (int)kind == 0)
			{
				flag = true;
			}
			if ((int)kind != 0)
			{
				flag = true;
			}
			if (flag)
			{
				int itemDescNumFromCPK = ShopItemTable.getItemDescNumFromCPK(character, position, kind);
				addItemAttr(itemDescNumFromCPK, position, avatarType, userItemAttr);
			}
		}
	}
}
