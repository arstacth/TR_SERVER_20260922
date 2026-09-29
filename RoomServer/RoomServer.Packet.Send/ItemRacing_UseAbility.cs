using LocalCommons.Network;
using RoomServer.Holders;
using RoomServer.Structuring;
using RoomServer.Structuring.ItemRacing;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ItemRacing_UseAbility : NetPacket
	{
		public ItemRacing_UseAbility(Account User, int group, int ability, byte[] buffer, byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_INGAME_SPECIAL_ABILITY_FIRE_ACK);
			ns.Write(group);
			ns.Write(ability);
			ns.Write(1000);
			ItemRacingGroupAbility itemRacingGroupAbility = ItemRacingHolder.ItemRacingGroupAbilities.Find((ItemRacingGroupAbility f) => f.GroupNum == group && f.AbilityNum == ability);
			ns.WriteAnsiFixed_intSize(itemRacingGroupAbility.AbilityValue);
			ns.Write(itemRacingGroupAbility.AffectArea);
			ns.Write(itemRacingGroupAbility.Effect);
			ns.Write(itemRacingGroupAbility.CoolDown);
			ns.Write(itemRacingGroupAbility.DefaultTime);
			ns.Write(itemRacingGroupAbility.Preserve);
			ns.WriteAnsiFixed_intSize(string.Empty);
			ns.WriteAnsiFixed_intSize(ItemRacingHolder.ItemRacingAbilities.Find((ItemRacingAbility f) => f.AbilityNum == ability).AbilityDesc);
			ns.Write((int)User.RoomPos);
			ns.Write(0);
			ns.Write(buffer.Length);
			ns.Write(buffer, 0, buffer.Length);
			_ = last;
		}
	}
}
