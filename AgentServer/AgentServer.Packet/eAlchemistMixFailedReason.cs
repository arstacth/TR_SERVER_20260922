namespace AgentServer.Packet
{
	public enum eAlchemistMixFailedReason
	{
		eAlchemistMixFailedReason_UNKNOWN,
		eAlchemistMixFailedReason_CANNOT_FIND_RESULTITEM,
		eAlchemistMixFailedReason_DUPLICATE_RESULTITEM,
		eAlchemistMixFailedReason_NO_NEED_ITEMS,
		eAlchemistMixFailedReason_CANNOT_GIVE_RESULT_ITEM,
		eAlchemistMixFailedReason_NO_MIX_CONDITION,
		eAlchemistMixFailedReason_NOT_ENOUGH_UNCONSUMED_ITEM,
		eAlchemistMixFailedReason_OVER_MAKING_LIMIT,
		eAlchemistMixFailedReason_Max
	}
}
