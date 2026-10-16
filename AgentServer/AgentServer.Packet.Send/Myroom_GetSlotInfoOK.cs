using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_GetSlotInfoOK : NetPacket
	{
		public Myroom_GetSlotInfoOK(List<MyRoomSlotInfo> m_vSlotInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_GET_USERSLOT_INFO_ACK);
			ns.Write(m_vSlotInfo.Count);
			foreach (MyRoomSlotInfo item in m_vSlotInfo)
			{
				ns.Write(item.m_iSlotNum);
				ns.WriteAnsiFixed_intSize(item.m_strSlotName);
				// Thai row = Character half + Costume half + mode (matches SetSlot ACK / REQ).
				WriteAvatarHalf(item.m_AvatarInfo);
				WriteAvatarHalf(item.m_CostumeAvatarInfo);
				ns.Write((byte)(item.m_bIsUseCostume ? 1 : 0));
			}
			_ = last;
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
