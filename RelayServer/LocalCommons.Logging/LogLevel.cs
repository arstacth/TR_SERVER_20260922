using System;

namespace LocalCommons.Logging
{
	[Flags]
	public enum LogLevel
	{
		Info = 1,
		Warning = 2,
		Error = 4,
		Debug = 8,
		Status = 0x10,
		Exception = 0x20,
		None = 0x7FFF
	}
}
