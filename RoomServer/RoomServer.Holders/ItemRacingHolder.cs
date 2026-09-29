using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using RoomServer.Structuring.ItemRacing;
using Serilog;

namespace RoomServer.Holders
{
	public static class ItemRacingHolder
	{
		public static List<ItemRacingAbility> ItemRacingAbilities = new List<ItemRacingAbility>();

		public static List<ItemRacingGroupAbility> ItemRacingGroupAbilities = new List<ItemRacingGroupAbility>();

		public static List<ItemRacingGroupSetting> ItemRacingGroupSettings = new List<ItemRacingGroupSetting>();

		public static ConcurrentDictionary<int, int[]> ItemRacingMapSettings = new ConcurrentDictionary<int, int[]>();

		public static void LoadItemRacingData()
		{
			LoadItemRacingAbility();
			LoadItemRacingGroupAbility();
			LoadItemRacingGroupSetting();
			LoadItemRacingMapSetting();
			Log.Information("Load Item Racing Data Done!");
		}

		private static void LoadItemRacingAbility()
		{
			ItemRacingAbilities.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedataitemracingability;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				ItemRacingAbilities.Add(new ItemRacingAbility
				{
					AbilityNum = Convert.ToInt32(mySqlDataReader["fdAbilityNum"]),
					AbilityDesc = mySqlDataReader["fdAbilityDesc"].ToString()
				});
			}
		}

		private static void LoadItemRacingGroupAbility()
		{
			ItemRacingGroupAbilities.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedataitemracinggroupability;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				ItemRacingGroupAbilities.Add(new ItemRacingGroupAbility
				{
					GroupNum = Convert.ToInt32(mySqlDataReader["fdGroupNum"]),
					AbilityNum = Convert.ToInt32(mySqlDataReader["fdAbilityNum"]),
					AbilityValue = (Convert.IsDBNull(mySqlDataReader["fdAbilityValue"]) ? string.Empty : mySqlDataReader["fdAbilityValue"].ToString()),
					CoolDown = Convert.ToInt32(mySqlDataReader["fdCoolDown"]),
					Preserve = Convert.ToInt32(mySqlDataReader["fdPreserve"]),
					AffectArea = Convert.ToInt32(mySqlDataReader["fdAffectArea"]),
					Effect = Convert.ToInt32(mySqlDataReader["fdEffect"]),
					DefaultTime = Convert.ToInt32(mySqlDataReader["fdDefaultTime"])
				});
			}
		}

		private static void LoadItemRacingGroupSetting()
		{
			ItemRacingGroupSettings.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedataitemracinggroupsetting;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				ItemRacingGroupSettings.Add(new ItemRacingGroupSetting
				{
					GroupNum = Convert.ToInt32(mySqlDataReader["fdGroupNum"]),
					GroupName = mySqlDataReader["fdGroupName"].ToString(),
					Element = Convert.ToInt32(mySqlDataReader["fdElement"])
				});
			}
		}

		private static void LoadItemRacingMapSetting()
		{
			ItemRacingMapSettings.Clear();
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand("select * from gamedataitemracingmapsetting;", mySqlConnection);
			mySqlCommand.Parameters.Clear();
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
			while (mySqlDataReader.Read())
			{
				int key = Convert.ToInt32(mySqlDataReader["fdMapNum"]);
				int[] value = Array.ConvertAll(mySqlDataReader["fdGroups"].ToString().Split(','), int.Parse);
				ItemRacingMapSettings.TryAdd(key, value);
			}
		}
	}
}
