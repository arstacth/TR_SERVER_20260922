namespace RoomServer.Function
{
	public static class FlagsHelper
	{
		public static bool IsSet(byte flags, byte flag)
		{
			return (flags & flag) != 0;
		}

		public static void Set(ref byte flags, byte flag)
		{
			int num = flags;
			flags = (byte)(num | flag);
		}

		public static void Unset(ref byte flags, byte flag)
		{
			int num = flags;
			flags = (byte)(num & ~flag);
		}
	}
}
