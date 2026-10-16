using System.Collections.Generic;
using LocalCommons.Network;

namespace TRCommon
{
	public class CAvatarLock
	{
		public List<int> m_avatarItems = new List<int>();

		public List<int> m_costumeItmes = new List<int>();

		public bool m_bUseCostume;

		public bool m_bUseAvatarLock;

		public bool isValid => m_bUseAvatarLock;

		public bool isUseCostume => m_bUseCostume;

		public CAvatarLock()
		{
			clear();
		}

		public CAvatarLock(List<int> avatarItems, List<int> costumItmes, bool bUseCostume)
		{
			m_avatarItems = avatarItems;
			m_costumeItmes = costumItmes;
			m_bUseCostume = bUseCostume;
			m_bUseAvatarLock = true;
			if (m_avatarItems.Count == 0)
			{
				m_bUseAvatarLock = false;
			}
		}

		public void clear()
		{
			m_avatarItems.Clear();
			m_costumeItmes.Clear();
			m_bUseCostume = false;
			m_bUseAvatarLock = false;
		}

		public void encode(PacketWriter encoder)
		{
			encoder.Write(m_avatarItems.Count);
			foreach (int avatarItem in m_avatarItems)
			{
				encoder.Write(avatarItem);
			}
			encoder.Write(m_costumeItmes.Count);
			foreach (int costumeItme in m_costumeItmes)
			{
				encoder.Write(costumeItme);
			}
			encoder.Write(m_bUseCostume);
			encoder.Write(m_bUseAvatarLock);
		}

		public void encode2(PacketWriter encoder)
		{
			encoder.Write(m_avatarItems.Count);
			foreach (int avatarItem in m_avatarItems)
			{
				encoder.Write(avatarItem);
			}
			encoder.Write(m_costumeItmes.Count);
			foreach (int costumeItme in m_costumeItmes)
			{
				encoder.Write(costumeItme);
			}
		}

		public void decode(PacketReader decoder)
		{
			m_avatarItems.Clear();
			int num = decoder.ReadLEInt32();
			for (int i = 0; i < num; i++)
			{
				m_avatarItems.Add(decoder.ReadLEInt32());
			}
			m_costumeItmes.Clear();
			num = decoder.ReadLEInt32();
			for (int j = 0; j < num; j++)
			{
				m_costumeItmes.Add(decoder.ReadLEInt32());
			}
			m_bUseCostume = decoder.ReadBoolean();
			m_bUseAvatarLock = decoder.ReadBoolean();
		}

		public bool getAvatarInfo(AVATAR iAvatarType, out AvatarInfo info)
		{
			info = default(AvatarInfo);
			if (!isValid)
			{
				return false;
			}
			List<int> list;
			if (AVATAR.AVATAR_REAL == iAvatarType)
			{
				list = m_avatarItems;
			}
			else
			{
				if (AVATAR.AVATAR_COSTUME != iAvatarType)
				{
					return false;
				}
				list = m_costumeItmes;
			}
			foreach (int item in list)
			{
				if (ShopItemTable.getCPKfromItemDescNum(item, out var Character, out var Position, out var Kind))
				{
					if ((int)Character != 0 && (int)Position == 0 && (int)Kind == 0)
					{
						info.setCharacter(Character);
					}
					else
					{
						info.setItemPart(Position, Kind);
					}
				}
			}
			return true;
		}

		public void deleteItem(int iItemNum)
		{
			if (isValid)
			{
				m_avatarItems.Remove(iItemNum);
				m_costumeItmes.Remove(iItemNum);
			}
		}

		public void deleteItem(List<int> items)
		{
			if (isValid)
			{
				m_avatarItems.RemoveAll((int r) => items.Contains(r));
				m_costumeItmes.RemoveAll((int r) => items.Contains(r));
			}
		}

		public bool isExist(int iItemNum)
		{
			if (!m_avatarItems.Contains(iItemNum))
			{
				return m_costumeItmes.Contains(iItemNum);
			}
			return true;
		}

		public bool setFromAdvancedAvatarInfo(AdvancedAvatarInfo info)
		{
			cpk_type character = info.m_realAvatarInfo.GetWear(0);
			int num = -1;
			num = ShopItemTable.getItemDescNumFromCPK(character, 0, 0);
			if (0 >= num)
			{
				return false;
			}
			m_avatarItems.Add(num);
			m_costumeItmes.Add(num);
			for (int i = 1; i < 15; i++)
			{
				if (0 < info.m_realAvatarInfo.GetWear(i))
				{
					num = ShopItemTable.getItemDescNumFromCPK(character, i, info.m_realAvatarInfo.GetWear(i));
					if (0 < num)
					{
						m_avatarItems.Add(num);
					}
				}
				if (0 < info.m_costumeAvatarInfo.GetWear(i))
				{
					num = ShopItemTable.getItemDescNumFromCPK(character, i, info.m_costumeAvatarInfo.GetWear(i));
					if (0 < num)
					{
						m_costumeItmes.Add(num);
					}
				}
			}
			m_bUseCostume = info.m_bIsUseCostume;
			m_bUseAvatarLock = true;
			return true;
		}

		public bool getAvatarItemList(AVATAR iAvatarType, out List<int> itemList)
		{
			itemList = new List<int>();
			bool result = false;
			switch (iAvatarType)
			{
			case AVATAR.AVATAR_REAL:
				foreach (int avatarItem in m_avatarItems)
				{
					itemList.Add(avatarItem);
				}
				result = true;
				break;
			case AVATAR.AVATAR_COSTUME:
				foreach (int costumeItme in m_costumeItmes)
				{
					itemList.Add(costumeItme);
				}
				result = true;
				break;
			}
			return result;
		}
	}
}
