namespace AgentServer
{
	public enum eLoginFail
	{
		eLoginFail_UNKNOWN,
		eLoginFail_AUTHENTICATE,
		eLoginFail_HASH_MISMATCH,
		eLoginFail_DUPLICATE_ID,
		eLoginFail_PROTOCOL_MISMATCH,
		eLoginFail_SERVER_NOT_READY,
		eLoginFail_SERVER_FULL,
		eLoginFail_BLACK_LIST,
		eLoginFail_INVALID_COUNTRY,
		eLoginFail_BAD_NUMBER_FOR_PREVENT_ABUSING,
		eLoginFail_REPORT_SYSTEM_PENALTY
	}
}
