namespace CommunityAgentServer.Structuring.Opcode
{
	public enum eCommunityAgentOpcode : short
	{
		IGNORE_0 = 0,
		LOGIN_REQ = 2,
		IGNORE_4 = 4,
		FORWARD_MULTI_REQ = 6,
		FORWARD_ONE_REQ = 7,
		FORWARD_ONE_ACK = 7,
		FORWARD_OFFLINE_ACK = 8,
		PING_REQ = 9,
		IGNORE_12 = 12,
		PROFILE_REQ = 14,
		PROFILE_ACK = 15,
		CHECK_GIFT_REQ = 18,
	}

	public enum eCommunityAgentProfile : short
	{
		UNKNOWN_0_REQ = 0,
		SET_PROFILE_REQ = 2,
		SET_PROFILE_ACK = 2,
		GET_MY_PROFILE_REQ = 3,
		GET_BY_NICK_REQ = 11,
	}
}
