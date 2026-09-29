using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class MyRoom_GetCharacterSetting_ACK : NetPacket
	{
		public MyRoom_GetCharacterSetting_ACK(AvatarInfo AvatarInfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eServer_MYROOM_GET_MY_CHARACTER_SETTING_ACK);
			WriteAvatarHalf(AvatarInfo);
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
