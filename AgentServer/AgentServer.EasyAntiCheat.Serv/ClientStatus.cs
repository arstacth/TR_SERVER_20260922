namespace AgentServer.EasyAntiCheat.Server
{
	public enum ClientStatus
	{
		Reserved,
		ClientAuthenticationFailed,
		ClientAuthenticatedLocal,
		ClientBanned,
		ClientViolation,
		ClientAuthenticatedRemote
	}
}
