using System.Collections.Generic;
using AgentServer;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using LocalCommons.Network;
using Serilog;

namespace AgentServer.Packet
{
	public static class CapybaraShopHandle
	{
		public static void Handle_TodaySchedule(ClientConnection client, PacketReader reader, byte last)
		{
			int shopNum = 0;
			if (reader.Remaining >= 4)
			{
				shopNum = reader.ReadLEInt32();
			}
			List<CapybaraScheduleRow> rows = CapybaraShopHolder.LoadTodaySchedule(shopNum);
			if (Conf.ProtocolDebug)
			{
				Log.Information("CAPYBARA TODAY_SCHEDULE shop={0} windows={1} user={2}", shopNum, rows.Count, client.CurrentAccount != null ? client.CurrentAccount.UserID : "?");
			}
			client.SendAsync(new CapybaraShopTodayScheduleAck(rows, last));
		}

		public static void Handle_UserInfo(ClientConnection client, PacketReader reader, byte last)
		{
			int shopNum = 0;
			int scheduleNum = 0;
			if (reader.Remaining >= 4)
			{
				shopNum = reader.ReadLEInt32();
			}
			if (reader.Remaining >= 4)
			{
				scheduleNum = reader.ReadLEInt32();
			}
			List<CapybaraUserTradeRow> rows = CapybaraShopHolder.GetUserInfo(client.CurrentAccount.UserNum, shopNum, scheduleNum);
			client.SendAsync(new CapybaraShopUserInfoAck(shopNum, scheduleNum, rows, last));
		}

		public static void Handle_Trade(ClientConnection client, PacketReader reader, byte last)
		{
			int shopNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int displayStand = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int category = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int scheduleNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			int itemNum = reader.Remaining >= 4 ? reader.ReadLEInt32() : 0;
			bool ok = CapybaraShopHolder.TradeItem(client.CurrentAccount.UserNum, shopNum, displayStand, category, scheduleNum, itemNum, out int tradeCount);
			client.SendAsync(new CapybaraShopTradeAck(ok ? 1 : 0, tradeCount, last));
		}
	}
}
