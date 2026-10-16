using System.Linq;
using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring;
using RoomServer.Structuring.ItemRacing;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ItemRacing_GetAbilityAck : NetPacket
	{
		public ItemRacing_GetAbilityAck(Account User, int area, int group, int unk, byte[] buffer, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_ACQUIRE_ACK);
			ns.Write(area);
			ns.Write((int)User.RoomPos);
			ns.Write(0);
			ns.Write(group);
			ns.Write(ItemRacingHolder.ItemRacingGroupSettings.Find((ItemRacingGroupSetting f) => f.GroupNum == group).Element);
			int value = ItemRacingHolder.ItemRacingGroupAbilities.Count((ItemRacingGroupAbility c) => c.GroupNum == group);
			ns.Write(value);
			foreach (ItemRacingGroupAbility groupability in ItemRacingHolder.ItemRacingGroupAbilities.FindAll((ItemRacingGroupAbility f) => f.GroupNum == group))
			{
				ns.Write(groupability.AbilityNum);
				ns.Write(1000);
				ns.WriteAnsiFixed_intSize(groupability.AbilityValue);
				ns.Write(groupability.AffectArea);
				ns.Write(groupability.Effect);
				ns.Write(groupability.CoolDown);
				ns.Write(groupability.DefaultTime);
				ns.Write(groupability.Preserve);
				ns.WriteAnsiFixed_intSize(string.Empty);
				ns.WriteAnsiFixed_intSize(ItemRacingHolder.ItemRacingAbilities.Find((ItemRacingAbility f) => f.AbilityNum == groupability.AbilityNum).AbilityDesc);
			}
			ns.Write(unk);
			ns.Write(buffer.Length);
			ns.Write(buffer, 0, buffer.Length);
			ns.WriteAnsiFixed_intSize(ItemRacingHolder.ItemRacingGroupSettings.Find((ItemRacingGroupSetting f) => f.GroupNum == group).GroupName);
			_ = last;
		}
	}
}
