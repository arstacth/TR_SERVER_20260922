using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AgentServer.Database;
using AgentServer.Structuring.Item;
using Serilog;

namespace AgentServer.Holders
{
	public static class ItemCubeHolder
	{
		public static ConcurrentDictionary<int, CUBE_INFO> CubeInfo { get; } = new ConcurrentDictionary<int, CUBE_INFO>();


		public static void LoadItemCubeInfo()
		{
			try
			{
				new Dictionary<int, CUBE_INFO>();
				CubeInfo.Clear();
				using (MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_itemcube_cubeinfo"))
				{
					mySqlCommandHelper.Execute();
					while (mySqlCommandHelper.HasResult())
					{
						CUBE_INFO cUBE_INFO = default(CUBE_INFO);
						cUBE_INFO.cube = mySqlCommandHelper.GetInt("fdCube");
						cUBE_INFO.silver_cube_rate = mySqlCommandHelper.GetFloat("fdSilverRate");
						cUBE_INFO.canmove_storage_if_duplicated = mySqlCommandHelper.GetBoolean("fdCanStorage");
						CUBE_INFO value = cUBE_INFO;
						CubeInfo.TryAdd(value.cube, value);
					}
				}
				Log.Information("Load ItemCubeInfo : {0}", CubeInfo.Count);
			}
			catch (Exception ex)
			{
				Log.Error("Load ItemCubeInfo Error : {0}", ex.ToString());
			}
		}
	}
}
