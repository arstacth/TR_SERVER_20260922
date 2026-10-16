using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_SetSlotItemSettingOK : NetPacket
	{
		public Myroom_SetSlotItemSettingOK(int slotNum, string SlotName, AvatarInfo avatarInfo, byte last)
			: this(slotNum, SlotName, avatarInfo, default(AvatarInfo), useCostume: false, last)
		{
		}

		/// <summary>
		/// Thai ACK matches GetSlotInfoOK row: Character half + Costume half + mode.
		/// </summary>
		public Myroom_SetSlotItemSettingOK(int slotNum, string SlotName, AvatarInfo realAvatar, AvatarInfo costumeAvatar, bool useCostume, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_SET_SLOTITEM_SETTING_ACK);
			ns.Write(slotNum);
			ns.WriteAnsiFixed_intSize(SlotName ?? string.Empty);
			WriteAvatarHalf(realAvatar);
			WriteAvatarHalf(costumeAvatar);
			ns.Write((byte)(useCostume ? 1 : 0));
		}

		private void WriteAvatarHalf(AvatarInfo info)
		{
			for (int i = 0; i < 15; i++)
			{
				ushort v = info.GetWear(i);
				ns.Write(v == ushort.MaxValue ? (ushort)0 : v);
			}
			for (int i = 0; i < 7; i++)
			{
				ushort v = info.GetAcc(i);
				ns.Write(v == ushort.MaxValue ? (ushort)0 : v);
			}
			ushort ef = info.GetEF();
			ns.Write(ef == ushort.MaxValue ? (ushort)0 : ef);
			ns.Fill(Conf.AvatarDyePadBytes);
		}
	}
}
