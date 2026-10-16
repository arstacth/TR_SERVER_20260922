using System.Linq;
using MySql.Data.MySqlClient;
using NestedDictionaryLib;
using RoomServer.Structuring;
using RoomServer.Structuring.TowerEvent;
using Serilog;

namespace RoomServer.Holders
{
	public static class TowerEventHolder
	{
		public static NestedDictionary<int, int, Tower_BoxData> TowerQuizList = new NestedDictionary<int, int, Tower_BoxData>();

		public static void LoadTowerQuizInfo()
		{
			TowerQuizList.Clear();
			using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
			{
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("select * from essentowerquizinfo", mySqlConnection);
				mySqlCommand.Parameters.Clear();
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					Tower_BoxData tower_BoxData = new Tower_BoxData
					{
						QuizType = mySqlDataReader.GetInt32("fdType"),
						BoxGrade = 3,
						QuestionNum = mySqlDataReader.GetInt32("fdIndex"),
						UNK2 = true,
						Question = mySqlDataReader.GetString("fdQuiz"),
						Answer = mySqlDataReader.GetString("fdAnswer")
					};
					TowerQuizList.Add(tower_BoxData.QuizType, tower_BoxData.QuestionNum, tower_BoxData);
				}
			}
			Log.Information("LoadTowerQuizInfo Done!");
		}

		public static void TowerEventStatus(int type)
		{
			switch (type)
			{
			case 2:
				ServerStatus.TowerEventInit();
				{
					foreach (NormalRoom item in Rooms.RoomList.Values.Where((NormalRoom rm) => rm.RoomKindID == 74))
					{
						item.TowerEventInit();
					}
					break;
				}
			case 3:
				ServerStatus.TowerEventEnd();
				{
					foreach (NormalRoom item2 in Rooms.RoomList.Values.Where((NormalRoom rm) => rm.RoomKindID == 74))
					{
						item2.TowerEventEnd();
					}
					break;
				}
			}
		}
	}
}
