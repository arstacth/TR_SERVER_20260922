using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;
using TRCommon;

namespace AgentServer.Packet.Send
{
	public sealed class GET_AVATAR_ACK : NetPacket
	{
		public GET_AVATAR_ACK(Account User, bool bRequestNickName, byte last)
		{
			_ = bRequestNickName;
			User.EnsureAvatarDyeingSlots();
			ns.WriteOP(Opcodes.eServer_GET_AVATAR_ACK);
			ns.Write(0);
			long bodyStart = ns.Position;
			WriteAvatarHalf(User, costume: false, 0);
			WriteAvatarHalf(User, costume: true, 12);
			ns.Write(User.advancedAvatarInfo.isUseCostume);
			// Packed memcpy is 353 after opcode+result: 2×(46+130)+1 bool, then TCP last.
			const int bodyNeed = 353;
			int written = (int)(ns.Position - bodyStart);
			if (written < bodyNeed)
			{
				ns.Fill(bodyNeed - written);
			}
			ns.Write(last);
		}

		private void WriteAvatarHalf(Account user, bool costume, int dyeStart)
		{
			AdvancedAvatarInfo av = user.advancedAvatarInfo;
			for (byte i = 0; i < 15; i = (byte)(i + 1))
			{
				ns.Write(costume
					? (ushort)(int)av.m_costumeAvatarInfo.m_nItemPartArry[i]
					: (ushort)(int)av.m_realAvatarInfo.m_nItemPartArry[i]);
			}
			for (byte i = 0; i < 7; i = (byte)(i + 1))
			{
				ns.Write(costume
					? (ushort)(int)av.m_costumeAvatarInfo.m_nGameAccArry[i]
					: (ushort)(int)av.m_realAvatarInfo.m_nGameAccArry[i]);
			}
			ns.Write(costume
				? (ushort)(int)av.m_costumeAvatarInfo.m_nEFItemArry[0]
				: (ushort)(int)av.m_realAvatarInfo.m_nEFItemArry[0]);
			for (int i = 0; i < 12; i++)
			{
				UserItemDyeing dye = user.AvatarItemDyeing[dyeStart + i];
				ns.Write(dye.DyeingPart);
				ns.Write(dye.Color1, 0, 3);
				ns.Write(dye.Color2, 0, 3);
				ns.Write(dye.Color3, 0, 3);
			}
			int extra = Conf.AvatarDyePadBytes - 120;
			if (extra > 0)
			{
				ns.Fill(extra);
			}
		}
	}
}
