using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using MySql.Data.MySqlClient;
using Serilog;

namespace AgentServer.Packet
{
	public class CubeHandle
	{
		public static void Handle_CubeCheck(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (currentAccount.CubeState != 0 && currentAccount.CubeState != eSTATE.eSTATE_INFOCHECK_DONE)
			{
				Client.SendAsync(new CubeInfo(num, 0, 0, 0, eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_INVALID_STATE, last));
				return;
			}
			if (!ItemCubeHolder.CubeInfo.ContainsKey(num))
			{
				Client.SendAsync(new CubeInfo(num, 0, 0, 0, eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_NO_CUBE, last));
				return;
			}
			currentAccount.CubeState = eSTATE.eSTATE_INFOCHECK;
			itemcube_preapre_cube(currentAccount.UserNum, num, out var processing);
			itemcube_preapre_cube_done(currentAccount, num, processing, last);
		}

		public static void Handle_CubeOpen(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (currentAccount.CubeState != eSTATE.eSTATE_INFOCHECK_DONE)
			{
				Client.SendAsync(new CubeOpen(num, new List<ITEM_OPEN_INFO>(), new List<ITEM_UNOPEN_INFO>(), eCUBE_TYPE.eCUBE_TYPE_NORMAL, eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_STATE_ERROR, 0, last));
				return;
			}
			if (currentAccount.CubeProcess == null)
			{
				Client.SendAsync(new CubeOpen(num, new List<ITEM_OPEN_INFO>(), new List<ITEM_UNOPEN_INFO>(), eCUBE_TYPE.eCUBE_TYPE_NORMAL, eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_UNKNOWN_ERROR, 0, last));
				return;
			}
			if (num != currentAccount.CubeProcess.cube)
			{
				Client.SendAsync(new CubeOpen(num, new List<ITEM_OPEN_INFO>(), new List<ITEM_UNOPEN_INFO>(), currentAccount.CubeProcess.result.cube_type, eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_NOT_CHECKED, 0, last));
				return;
			}
			if (!ItemCubeHolder.CubeInfo.ContainsKey(num))
			{
				Client.SendAsync(new CubeOpen(num, new List<ITEM_OPEN_INFO>(), new List<ITEM_UNOPEN_INFO>(), currentAccount.CubeProcess.result.cube_type, eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_REMOVE_BY_RELOAD, 0, last));
				return;
			}
			int num2 = 0;
			foreach (ITEM_OPEN_INFO item in currentAccount.CubeProcess.result.items_open)
			{
				num2 += item.guage_delta;
			}
			num2 = ((currentAccount.CubeProcess.gold_guage + num2 > currentAccount.CubeProcess.gold_guage_max) ? (currentAccount.CubeProcess.gold_guage_max - currentAccount.CubeProcess.gold_guage) : num2);
			itemcube_open_done(currentAccount, num, num2);
			currentAccount.CubeState = eSTATE.eSTATE_ACCEPT;
			Client.SendAsync(new CubeOpen(num, currentAccount.CubeProcess, last));
		}

		public static void Handle_CubeItemAccept(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			reader.ReadLEInt32();
			int itemindex = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (currentAccount.CubeState != eSTATE.eSTATE_ACCEPT)
			{
				Client.SendAsync(new CubeItemAccept(eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_STATE_ERROR, num, last));
				return;
			}
			if (currentAccount.CubeProcess == null)
			{
				Client.SendAsync(new CubeItemAccept(eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_UNKNOWN_ERROR, num, last));
				Log.Error("Fatal error!! processing is NULL. state accept");
				return;
			}
			if (currentAccount.CubeProcess.result.items_open.Count((ITEM_OPEN_INFO c) => c.index == itemindex) <= 0)
			{
				Client.SendAsync(new CubeItemAccept(eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_INVALID_INDEX, num, last));
				Log.Error("Fatal error!! Acception index is invalid({0})", itemindex);
				return;
			}
			if (!currentAccount.CubeProcess.result.items_open.FirstOrDefault((ITEM_OPEN_INFO f) => f.index == itemindex).canmove_storage && num2 == 0)
			{
				Client.SendAsync(new CubeItemAccept(eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_STATE_ERROR, num, last));
				return;
			}
			itemcube_accept(currentAccount, num, itemindex, num2);
			if (currentAccount.CubeProcess.acception_result == eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_OK)
			{
				Client.SendAsync(new CubeItemAccept(currentAccount.CubeProcess.acception_result, num, last));
				currentAccount.CubeState = eSTATE.eSTATE_IDLE;
				currentAccount.CubeProcess = null;
			}
			else
			{
				Client.SendAsync(new CubeItemAccept(currentAccount.CubeProcess.acception_result, num, last));
			}
		}

		private static void itemcube_preapre_cube(int UserNum, int CubeItem, out CUBE_INFO_PROCESS processing)
		{
			processing = new CUBE_INFO_PROCESS();
			eCUBE_TYPE t = eCUBE_TYPE.eCUBE_TYPE_NORMAL;
			int a = 0;
			processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_OK;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemcube_preapre_cube");
				mySqlCommandHelper.AddParamInt("cube", CubeItem);
				mySqlCommandHelper.AddParamInt("user_num", UserNum);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					processing.cube_count = mySqlCommandHelper.GetInt("fdCount");
					processing.gold_guage = mySqlCommandHelper.GetInt("fdGoldGuage");
					processing.gold_guage_max = mySqlCommandHelper.GetInt("fdGoldGuageMax");
					processing.open_count = mySqlCommandHelper.GetInt("fdOpenable");
					t = (eCUBE_TYPE)mySqlCommandHelper.GetInt("fdCubeType");
					a = mySqlCommandHelper.GetInt("fdMaxAcceptable");
					processing.cube = CubeItem;
				}
				mySqlCommandHelper.NextResult();
				processing.result = new CCubeOpenAck(CubeItem, t, eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_OK, a);
				while (mySqlCommandHelper.HasResult())
				{
					ITEM_OPEN_INFO item = new ITEM_OPEN_INFO
					{
						index = mySqlCommandHelper.GetInt("fdIndex"),
						item = mySqlCommandHelper.GetInt("fdItem"),
						count = mySqlCommandHelper.GetInt("fdCount"),
						guage_delta = mySqlCommandHelper.GetInt("fdGuage"),
						canmove_storage = (mySqlCommandHelper.GetInt("fdCanMoveStorage") > 0)
					};
					processing.result.items_open.Add(item);
				}
				mySqlCommandHelper.NextResult();
				if (processing.open_count < processing.result.items_open.Count)
				{
					processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_DB_ERROR;
					return;
				}
				while (mySqlCommandHelper.HasResult())
				{
					ITEM_UNOPEN_INFO item2 = new ITEM_UNOPEN_INFO
					{
						index = mySqlCommandHelper.GetInt("fdIndex"),
						item = mySqlCommandHelper.GetInt("fdItem"),
						count = mySqlCommandHelper.GetInt("fdCount")
					};
					processing.result.items_unopen.Add(item2);
				}
			}
			catch (MySqlException ex)
			{
				processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_DB_ERROR;
				if (ex.Message.Contains("No cube."))
				{
					processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_NO_CUBE;
				}
				else if (ex.Message.Contains("Not Enough Capacity"))
				{
					processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_NOMORE_STORAGE;
				}
				Log.Error("usp_itemcube_preapre_cube Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				processing.r = eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_DB_ERROR;
				Log.Error("usp_itemcube_preapre_cube Error:{0}", ex2.Message);
			}
		}

		private static void itemcube_preapre_cube_done(Account User, int CubeItem, CUBE_INFO_PROCESS process, byte last)
		{
			if (User.CubeState != eSTATE.eSTATE_INFOCHECK)
			{
				return;
			}
			if (process.r == eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_OK)
			{
				if (process.result.items_open.Count >= process.result.max_acceptable)
				{
					User.Connection.SendAsync(new CubeInfo(CubeItem, process.cube_count, process.gold_guage, process.gold_guage_max, process.r, last));
					User.CubeState = eSTATE.eSTATE_INFOCHECK_DONE;
					User.CubeProcess = process;
					return;
				}
				Log.Error("CItemCubeUserInfo : max acceptable error {0}, {1}", process.result.max_acceptable, process.result.items_open.Count);
				User.Connection.SendAsync(new CubeInfo(CubeItem, 0, 0, 0, eCUBE_INFO_RESULT.eCUBE_INFO_RESULT_DB_ERROR, last));
			}
			else
			{
				User.Connection.SendAsync(new CubeInfo(CubeItem, 0, 0, 0, process.r, last));
			}
			User.CubeState = eSTATE.eSTATE_IDLE;
		}

		private static void itemcube_open_done(Account User, int CubeItem, int delta_guage)
		{
			try
			{
				User.CubeProcess.result.set_result(eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_OK);
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemcube_open_done");
				mySqlCommandHelper.AddParamInt("cube", CubeItem);
				mySqlCommandHelper.AddParamInt("user_num", User.UserNum);
				mySqlCommandHelper.AddParamInt("cube_type", (int)User.CubeProcess.result.cube_type);
				mySqlCommandHelper.AddParamInt("delta_goldguage", delta_guage);
				mySqlCommandHelper.Execute();
			}
			catch (Exception ex)
			{
				User.CubeProcess.result.set_result(eCUBE_OPEN_RESULT.eCUBE_OPEN_RESULT_UNKNOWN_ERROR);
				Log.Error("usp_itemcube_open_done Error:{0}", ex.Message);
			}
		}

		public static void itemcube_accept(Account User, int CubeItem, int index, int accepttype)
		{
			ITEM_OPEN_INFO iTEM_OPEN_INFO = User.CubeProcess.result.items_open.FirstOrDefault((ITEM_OPEN_INFO f) => f.index == index);
			try
			{
				User.CubeProcess.acception_result = eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_OK;
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemcube_accept");
				mySqlCommandHelper.AddParamInt("cube", CubeItem);
				mySqlCommandHelper.AddParamInt("user_num", User.UserNum);
				mySqlCommandHelper.AddParamInt("cube_type", (int)User.CubeProcess.result.cube_type);
				mySqlCommandHelper.AddParamInt("item1", iTEM_OPEN_INFO.item);
				mySqlCommandHelper.AddParamInt("count1", iTEM_OPEN_INFO.count);
				mySqlCommandHelper.AddParamInt("accept1", accepttype);
				mySqlCommandHelper.ExecuteNonQuery();
			}
			catch (MySqlException ex)
			{
				User.CubeProcess.acception_result = eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_DB_ERROR;
				if (ex.Message.Contains("Not Enough Capacity"))
				{
					User.CubeProcess.acception_result = eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_NOMORE_STORAGE;
				}
				Log.Error("usp_itemcube_accept Error:{0}", ex.Message);
			}
			catch (Exception ex2)
			{
				User.CubeProcess.acception_result = eCUBE_ACCEPT_RESULT.eCUBE_ACCEPT_RESULT_DB_ERROR;
				Log.Error("usp_itemcube_accept Error:{0}", ex2.Message);
			}
		}
	}
}
