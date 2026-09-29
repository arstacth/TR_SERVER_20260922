using AgentServer.Network.Connections;
using AgentServer.Packet.Send;

namespace AgentServer.Packet
{
	public class UnknownHandle
	{
		public static void Handle_FF9701(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket1(last));
		}

		public static void Handle_FFCF01(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket3(last));
		}

		public static void Handle_FFD501(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket4(last));
		}

		public static void Handle_FF3202(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket6(last));
		}

		public static void Handle_FF4F02(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new DailyMissionInfo(last));
		}

		public static void Handle_FF5202(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket7(last));
		}

		public static void Handle_FF5602(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket8(last));
		}

		public static void Handle_FF5902(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket9(last));
		}

		public static void Handle_FF6602(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket12(last));
		}

		public static void Handle_FF6802(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket13(last));
		}

		public static void Handle_FFA905(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket14(last));
		}

		public static void Handle_FFF201(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket15("0A00000003000000901700000030B32C67010000030000009E1700000030B32C6701000003000000AC1700000030B32C6701000003000000BA1700000030B32C6701000003000000C81700000030B32C6701000003000000D61700000030B32C6701000003000000E41700000030B32C6701000003000000F21700000030B32C6701000003000000001800000030B32C67010000030000000E1800000030B32C67010000", last));
			Client.SendAsync(new UnknownPacket15("0A00000004000000A81800000030B32C6701000004000000B61800000030B32C6701000004000000C41800000030B32C6701000004000000D21800000030B32C6701000004000000E01800000030B32C6701000004000000EE1800000030B32C6701000004000000FC1800000030B32C67010000040000000A1900000030B32C6701000004000000181900000030B32C6701000004000000261900000030B32C67010000", last));
		}

		public static void Handle_FFFE01(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket16(last));
		}

		public static void Handle_FF8406(ClientConnection Client, byte last)
		{
			_ = Client.CurrentAccount;
			Client.SendAsync(new UnknownPacket20(last));
		}
	}
}
