namespace TRCommon
{
	public static class NetCommonFunc
	{
		public static bool isPetPosition(cpk_type p)
		{
			if (ITEM_POSITION.ITEM_POSITION_PET != (ITEM_POSITION)p)
			{
				return eFuncItemPosition.eFuncItemPosition_WEARABLE_PET == (eFuncItemPosition)p;
			}
			return true;
		}

		public static bool isWearItemPosition(cpk_type p)
		{
			return (ITEM_POSITION)p < ITEM_POSITION.ITEM_POSITION_COUNT;
		}

		public static bool isTopBodyWearItemPosition(cpk_type p)
		{
			if (ITEM_POSITION.ITEM_POSITION_TOPBODY != (ITEM_POSITION)p && eFuncItemPosition.eFuncItemPosition_TRANSFORM_APPEARANCE != (eFuncItemPosition)p)
			{
				return eFuncItemPosition.eFuncItemPosition_WEARABLE_PET == (eFuncItemPosition)p;
			}
			return true;
		}

		public static ushort getAvatarPartsByItemPosition(cpk_type p)
		{
			if (isTopBodyWearItemPosition(p))
			{
				return 2;
			}
			return p;
		}

		public static bool isUsableItemInRoom(cpk_type p)
		{
			if (eFuncItemPosition.eFuncItemPosition_REBIRTH_ITEM == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_FARM_NAME_TAG_ITEM == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_FRIEND_CALL_STONE_ITEM == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_CHATTING_MOTION == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_POSE == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_POSE_WITH_MODEL == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_MOTION == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_EXPANSION_VOICE == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_DASH_EFFECT == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_PARTY_BUFF == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_FREE_PASS_BUFF == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_EXTRA_ABILITY == (eFuncItemPosition)p || eFuncItemPosition.eFuncItemPosition_SKIN == (eFuncItemPosition)p)
			{
				return true;
			}
			return false;
		}

		public static cpk_type getItemPositionByItemKind(cpk_type p, cpk_type k)
		{
			if (ITEM_POSITION.ITEM_POSITION_TOPBODY == (ITEM_POSITION)p)
			{
				if (1000 <= (int)k && 2000 > (int)k)
				{
					return 106;
				}
				if (30000 <= (int)k && 40000 > (int)k)
				{
					return 117;
				}
			}
			return p;
		}
	}
}
