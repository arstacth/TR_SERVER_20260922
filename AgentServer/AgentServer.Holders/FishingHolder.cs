using System;
using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Fishing;
using MySql.Data.MySqlClient;
using Serilog;
using Weighted_Randomizer;

namespace AgentServer.Holders
{
	public static class FishingHolder
	{
		public static Dictionary<int, FishingEquip> FishingEquips = new Dictionary<int, FishingEquip>();

		public static Dictionary<int, Fish> Fishes = new Dictionary<int, Fish>();

		public static Dictionary<int, DecoyEx> Decoy_Ex = new Dictionary<int, DecoyEx>();

		public static Dictionary<int, IWeightedRandomizer<Decoy>> FishingBaitsDrawItem = new Dictionary<int, IWeightedRandomizer<Decoy>>();

		public static Dictionary<int, IWeightedRandomizer<Decoy>> FishingFailureDrawItem = new Dictionary<int, IWeightedRandomizer<Decoy>>();

		public static void LoadFishingInfo()
		{
			FishingEquips.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essenfishingequip;", mySqlConnection);
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					FishingEquip value = new FishingEquip
					{
						MinSec = Convert.ToInt32(mySqlDataReader["fdMinSec"]),
						MaxSec = Convert.ToInt32(mySqlDataReader["fdMaxSec"])
					};
					FishingEquips.Add(Convert.ToInt32(mySqlDataReader["fdItemNum"]), value);
				}
			}
			List<int> list = new List<int>();
			List<Decoy> list2 = new List<Decoy>();
			using (MySqlConnection mySqlConnection2 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection2.Open();
				using MySqlCommand mySqlCommand2 = new MySqlCommand("select * from essenfishing_decoy;", mySqlConnection2);
				using MySqlDataReader mySqlDataReader2 = mySqlCommand2.ExecuteReader();
				while (mySqlDataReader2.Read())
				{
					Decoy decoy2 = new Decoy
					{
						DecoyNum = Convert.ToInt32(mySqlDataReader2["fdDecoyNum"]),
						FishNum = Convert.ToInt32(mySqlDataReader2["fdFishNum"]),
						Count = Convert.ToInt32(mySqlDataReader2["fdCount"]),
						Failure = Convert.ToBoolean(mySqlDataReader2["fdFailure"])
					};
					list2.Add(decoy2);
					if (!list.Contains(decoy2.DecoyNum))
					{
						list.Add(decoy2.DecoyNum);
					}
				}
			}
			Decoy_Ex.Clear();
			using (MySqlConnection mySqlConnection3 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection3.Open();
				using MySqlCommand mySqlCommand3 = new MySqlCommand("select * from essenfishing_decoyex;", mySqlConnection3);
				using MySqlDataReader mySqlDataReader3 = mySqlCommand3.ExecuteReader();
				while (mySqlDataReader3.Read())
				{
					int key = Convert.ToInt32(mySqlDataReader3["fdDecoyNum"]);
					DecoyEx value2 = new DecoyEx
					{
						MiniGameType = Convert.ToInt32(mySqlDataReader3["fdMiniGameType"]),
						FailureReward = Convert.ToBoolean(mySqlDataReader3["fdFailureReward"]),
						CatchingMinTime = Convert.ToInt32(mySqlDataReader3["fdCatchingMinTime"]),
						CatchingMaxTime = Convert.ToInt32(mySqlDataReader3["fdCatchingMaxTime"])
					};
					Decoy_Ex.Add(key, value2);
				}
			}
			Fishes.Clear();
			using (MySqlConnection mySqlConnection4 = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection4.Open();
				using MySqlCommand mySqlCommand4 = new MySqlCommand("select * from essenfishing_fish;", mySqlConnection4);
				using MySqlDataReader mySqlDataReader4 = mySqlCommand4.ExecuteReader();
				while (mySqlDataReader4.Read())
				{
					int key2 = Convert.ToInt32(mySqlDataReader4["fdFishNum"]);
					Fish value3 = new Fish
					{
						MinSize = Convert.ToInt32(mySqlDataReader4["fdMinSize"]),
						MaxSize = Convert.ToInt32(mySqlDataReader4["fdMaxSize"])
					};
					Fishes.Add(key2, value3);
				}
			}
			FishingBaitsDrawItem.Clear();
			FishingFailureDrawItem.Clear();
			foreach (int decoy in list)
			{
				IWeightedRandomizer<Decoy> weightedRandomizer = new StaticWeightedRandomizer<Decoy>();
				IWeightedRandomizer<Decoy> weightedRandomizer2 = new StaticWeightedRandomizer<Decoy>();
				foreach (Decoy item in list2.Where((Decoy w) => w.DecoyNum == decoy))
				{
					if (!item.Failure)
					{
						weightedRandomizer.Add(item, item.Count);
					}
					else
					{
						weightedRandomizer2.Add(item, item.Count);
					}
				}
				FishingBaitsDrawItem.Add(decoy, weightedRandomizer);
				if (weightedRandomizer2.Count > 0)
				{
					FishingFailureDrawItem.Add(decoy, weightedRandomizer2);
				}
			}
			Log.Information("Load Fishing Info Done!");
		}
	}
}
