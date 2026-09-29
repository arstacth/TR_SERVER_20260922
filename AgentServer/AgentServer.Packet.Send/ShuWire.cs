using System.Collections.Generic;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	internal static class ShuWire
	{
		public static void WriteAvatars(PacketWriter ns, List<ShuAvatarInfo> avatars)
		{
			ns.Write((short)48);
			long[] slots = new long[6];
			for (int i = 0; i < 6; i++)
			{
				// Client empty sentinel is -1. Zero triggers _buildShuCharHasEquipItem duplicate ItemID=[0].
				slots[i] = -1L;
			}
			if (avatars != null)
			{
				foreach (ShuAvatarInfo avatar in avatars)
				{
					if (avatar.Position >= 0 && avatar.Position < 6 && avatar.itemID > 0)
					{
						slots[avatar.Position] = avatar.itemID;
					}
				}
			}
			for (int i = 0; i < 6; i++)
			{
				ns.Write(slots[i]);
			}
		}

		public static void WriteStatus(PacketWriter ns, List<ShuStatusInfo> status)
		{
			ns.Write((short)16);
			int[] vals = new int[4];
			if (status != null)
			{
				foreach (ShuStatusInfo item in status)
				{
					if (item.statustype >= 0 && item.statustype < 4)
					{
						vals[item.statustype] = item.value;
					}
				}
			}
			for (int i = 0; i < 4; i++)
			{
				ns.Write(vals[i]);
			}
		}

		public static void WriteStatusValues(PacketWriter ns, List<int> values)
		{
			ns.Write((short)16);
			for (int i = 0; i < 4; i++)
			{
				ns.Write((values != null && i < values.Count) ? values[i] : 0);
			}
		}

		public static void WriteCharacter(PacketWriter ns, long characterItemID, ShuCharInfo value, List<ShuAvatarInfo> avatars, List<ShuStatusInfo> status)
		{
			WriteCharacterDebugStyle(ns, characterItemID, value, avatars, status);
		}

		/// <summary>Byte layout from Desktop\DEBUG\AgentServer.exe Shu_GetUserCharacterItemList.</summary>
		public static void WriteCharacterDebugStyle(PacketWriter ns, long characterItemID, ShuCharInfo value, List<ShuAvatarInfo> avatars, List<ShuStatusInfo> status)
		{
			if (value == null)
			{
				value = new ShuCharInfo
				{
					avatarItemNum = 0,
					Name = string.Empty,
					state = 0,
					MotionList = 0L,
					PurchaseMotionList = 0L
				};
			}
			ns.Write(characterItemID);
			ns.Write(characterItemID);
			ns.Write(value.avatarItemNum);
			ns.Write((short)48);
			// Always 6 slots (48 bytes). Empty = -1 (not 0 — duplicate ItemID=[0]).
			long[] slots = new long[6];
			for (int i = 0; i < 6; i++)
			{
				slots[i] = -1L;
			}
			if (avatars != null)
			{
				foreach (ShuAvatarInfo avatar in avatars)
				{
					if (avatar.Position >= 0 && avatar.Position < 6 && avatar.itemID > 0)
					{
						slots[avatar.Position] = avatar.itemID;
					}
				}
			}
			// Slot 0 is the character body; must be a real item id.
			if (slots[0] <= 0)
			{
				slots[0] = characterItemID;
			}
			for (int i = 0; i < 6; i++)
			{
				ns.Write(slots[i]);
			}
			ns.Write((short)16);
			int[] vals = new int[4];
			if (status != null)
			{
				foreach (ShuStatusInfo item2 in status)
				{
					if (item2.statustype >= 0 && item2.statustype < 4)
					{
						vals[item2.statustype] = item2.value < 0 ? 0 : item2.value;
					}
				}
			}
			// Fill missing status slots with safe defaults (satiety/max bars).
			if (vals[0] <= 0)
			{
				vals[0] = 240;
			}
			if (vals[1] <= 0)
			{
				vals[1] = 100;
			}
			if (vals[2] <= 0)
			{
				vals[2] = 1000;
			}
			for (int i = 0; i < 4; i++)
			{
				ns.Write(vals[i]);
			}
			ns.Write(value.MotionList < 0 ? 0L : value.MotionList);
			ns.Write(value.PurchaseMotionList < 0 ? 0L : value.PurchaseMotionList);
			// Keep wire names ASCII. Thai/UTF-8 DB names vs Encoding.Default desync
			// the short-length string and leave the Shu list looking empty.
			string name = "Shu" + characterItemID;
			ns.WriteAnsiFixed_intSize(name);
			ns.Write(value.state);
		}
	}
}
