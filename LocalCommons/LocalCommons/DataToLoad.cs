using System;

namespace LocalCommons
{
	[Flags]
	public enum DataToLoad
	{
		Items = 1,
		Maps = 2,
		Jobs = 4,
		Servers = 8,
		Barracks = 0x10,
		Monsters = 0x20,
		Skills = 0x40,
		Exp = 0x80,
		Dialogues = 0x100,
		Shops = 0x200,
		Help = 0x400,
		CustomCommands = 0x800,
		ChatMacros = 0x1000,
		All = int.MaxValue
	}
}
