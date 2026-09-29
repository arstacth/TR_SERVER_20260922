namespace AgentServer.EasyAntiCheat.Server
{
	public enum PlayerRegisterFlags
	{
		PlayerRegisterFlagNone = 0,
		PlayerRegisterFlagAdmin = 1,
		PlayerRegisterFlagMouseKeyboardControl = 2,
		PlayerRegisterFlagGamepadControl = 4,
		PlayerRegisterFlagTouchControl = 8,
		PlayerRegisterFlagXBox = 0x10,
		PlayerRegisterFlagPlaystation = 0x20,
		PlayerRegisterFlagiOS = 0x40,
		PlayerRegisterFlagAndroid = 0x80,
		PlayerRegisterFlagNintendoSwitch = 0x100,
		PlayerRegisterFlagUnprotectedClient = 0x200,
		PlayerRegisterFlagPlayerAIBot = 0x400
	}
}
