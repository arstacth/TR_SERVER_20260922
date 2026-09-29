using AgentServer.Holders;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	internal static class StatSystemSp
	{
		public static int Remain()
		{
			int sp = ServerSettingHolder.ServerSettings != null
				? ServerSettingHolder.ServerSettings.StatSystemStatPoint
				: 999;
			if (sp < 999)
			{
				sp = 999;
			}
			return sp;
		}

		public static int Max()
		{
			int max = ServerSettingHolder.ServerSettings != null
				? ServerSettingHolder.ServerSettings.StatSystemMainStatMaxPoint
				: 999;
			int sp = Remain();
			if (max < sp)
			{
				max = sp;
			}
			return max;
		}
	}

	/// <summary>
	/// Wire 1989. 01:03 dbgtrace consumed 34 of 210 then leftover 176:
	/// lastSlot(999), page count(0), userinfo count(1). SP-as-lastSlot was wrong.
	/// Layout: result, lastSlot, pageCount, [pageCount × 16], userinfoCount, [userinfoCount × 16].
	/// </summary>
	public sealed class StatSystemMyInfoAck : NetPacket
	{
		public StatSystemMyInfoAck(byte last, int pageCount = 1, string[] titles = null)
		{
			_ = pageCount;
			_ = titles;
			// 01:03 parsed lastSlot, pageCount 0, userinfo 1, 16-byte row (34 bytes).
			// Page loop + extra ints Overpop +4 at every new end (50/52/56).
			ns.Write((ushort)Opcodes.eServer_PACKED_1989_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(1);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class StatSystemSetPageNumAck : NetPacket
	{
		public StatSystemSetPageNumAck(int page, byte last)
		{
			ns.Write((ushort)1610);
			ns.Write(0);
			ns.Write(page);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class StatSystemPayPageResultAck : NetPacket
	{
		public StatSystemPayPageResultAck(byte last)
		{
			ns.Write((ushort)639);
			ns.Write(0);
			ns.Write(0);
			_ = last;
		}
	}

	public sealed class StatSystemSavePageAck : NetPacket
	{
		public StatSystemSavePageAck(int page, int remainSp, byte last)
		{
			ns.Write((ushort)1101);
			ns.Write(0);
			ns.Write(page);
			ns.Write(remainSp);
			_ = last;
		}
	}

	public sealed class StatSystemSaveTitleAck : NetPacket
	{
		public StatSystemSaveTitleAck(int page, string title, byte last)
		{
			ns.Write((ushort)583);
			ns.Write(0);
			ns.Write(page);
			ns.WriteBIG5Fixed_shortSize(title ?? string.Empty);
			_ = last;
		}
	}
}
