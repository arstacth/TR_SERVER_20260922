using Serilog;

namespace RoomServer.Room
{
	public static class RoomResultTable
	{
		private const int MAX_ROOM_PLAYER_NUM = 100;

		private const int MAX_RELAY_TEAM_NUM = 6;

		private static int[,] m_iNormalExpTable = new int[101, 101];

		private static int[,] m_iNormalGameMoneyTable = new int[101, 101];

		private static int[,] m_iRelayExpTable = new int[7, 7];

		private static int[,] m_iRelayGameMoneyTable = new int[7, 7];

		public static void InitResultTable()
		{
			initNormalTable();
			initRelayTable();
		}

		public static int getTimeOutExp(int iNumRunner)
		{
			return m_iNormalExpTable[iNumRunner, 0];
		}

		public static int getTimeOutGamePoint(int iNumRunner)
		{
			return m_iNormalGameMoneyTable[iNumRunner, 0];
		}

		public static int getExp(int iRank, int iNumRunner)
		{
			return m_iNormalExpTable[iNumRunner, iRank];
		}

		public static int getGamePoint(int iRank, int iNumRunner)
		{
			return m_iNormalGameMoneyTable[iNumRunner, iRank];
		}

		public static int getWinTeamGuildMatchExp(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 65;
			case 3:
			case 4:
				return 70;
			case 5:
			case 6:
				return 75;
			case 7:
			case 8:
				return 80;
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
				return 85;
			default:
				Log.Error("Error : In result proc exp, win team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getLoseTeamGuildMatchExp(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 30;
			case 3:
			case 4:
				return 33;
			case 5:
			case 6:
				return 36;
			case 7:
			case 8:
				return 40;
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
				return 43;
			default:
				Log.Error("Error : In result proc exp, win team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getWinTeamExp(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 20;
			case 3:
			case 4:
				return 30;
			case 5:
			case 6:
				return 40;
			case 7:
			case 8:
				return 50;
			case 9:
			case 10:
				return 55;
			case 11:
			case 12:
				return 60;
			case 13:
			case 14:
				return 65;
			case 15:
			case 16:
				return 70;
			case 17:
			case 18:
				return 75;
			case 19:
			case 20:
				return 80;
			default:
				Log.Error("Error : In result proc exp, win team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getLoseTeamExp(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 15;
			case 3:
			case 4:
				return 17;
			case 5:
			case 6:
				return 20;
			case 7:
			case 8:
				return 22;
			case 9:
			case 10:
				return 25;
			case 11:
			case 12:
				return 27;
			case 13:
			case 14:
				return 30;
			case 15:
			case 16:
				return 32;
			case 17:
			case 18:
				return 35;
			case 19:
			case 20:
				return 37;
			default:
				Log.Error("Error : In result proc exp, lose team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getLoseTeamGuildMatchGameMoney(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 35;
			case 3:
			case 4:
				return 40;
			case 5:
			case 6:
				return 45;
			case 7:
			case 8:
				return 50;
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
				return 55;
			default:
				Log.Debug("Error : In result proc gamemoney, team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getWinTeamGuildMatchGameMoney(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 160;
			case 3:
			case 4:
				return 190;
			case 5:
			case 6:
				return 220;
			case 7:
			case 8:
				return 250;
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
				return 280;
			default:
				Log.Error("Error : In result proc gamemoney, win team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getWinTeamGameMoney(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 15;
			case 3:
			case 4:
				return 20;
			case 5:
			case 6:
				return 40;
			case 7:
			case 8:
				return 70;
			case 9:
			case 10:
				return 100;
			case 11:
			case 12:
				return 130;
			case 13:
			case 14:
				return 160;
			case 15:
			case 16:
				return 190;
			case 17:
			case 18:
				return 220;
			case 19:
			case 20:
				return 250;
			default:
				Log.Error("Error : In result proc gamemoney, win team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getLoseTeamGameMoney(int roomPlayerNum)
		{
			switch (roomPlayerNum)
			{
			case 1:
			case 2:
				return 10;
			case 3:
			case 4:
				return 10;
			case 5:
			case 6:
				return 15;
			case 7:
			case 8:
				return 20;
			case 9:
			case 10:
				return 25;
			case 11:
			case 12:
				return 30;
			case 13:
			case 14:
				return 35;
			case 15:
			case 16:
				return 40;
			case 17:
			case 18:
				return 45;
			case 19:
			case 20:
				return 50;
			default:
				Log.Error("Error : In result proc gamemoney, team player num incorrect. playerNum({0})", roomPlayerNum);
				return 0;
			}
		}

		public static int getRelayTeamResultExp(int iRank, int numTeam)
		{
			return m_iRelayExpTable[numTeam, iRank];
		}

		public static int getRelayTeamResultGameMoney(int iRank, int numTeam)
		{
			return m_iRelayGameMoneyTable[numTeam, iRank];
		}

		private static void initNormalTable()
		{
			int num = 0;
			int num2 = 0;
			for (num = 0; num < 101; num++)
			{
				for (num2 = 0; num2 < 101; num2++)
				{
					m_iNormalExpTable[num, num2] = 10;
					m_iNormalGameMoneyTable[num, num2] = 10;
				}
			}
			m_iNormalExpTable[2, 1] = 20;
			m_iNormalExpTable[2, 2] = 15;
			m_iNormalExpTable[3, 1] = 30;
			m_iNormalExpTable[3, 2] = 20;
			m_iNormalExpTable[3, 3] = 16;
			m_iNormalExpTable[4, 1] = 35;
			m_iNormalExpTable[4, 2] = 25;
			m_iNormalExpTable[4, 3] = 21;
			m_iNormalExpTable[4, 4] = 17;
			m_iNormalExpTable[5, 1] = 40;
			m_iNormalExpTable[5, 2] = 30;
			m_iNormalExpTable[5, 3] = 26;
			m_iNormalExpTable[5, 4] = 22;
			m_iNormalExpTable[5, 5] = 18;
			m_iNormalExpTable[6, 1] = 45;
			m_iNormalExpTable[6, 2] = 35;
			m_iNormalExpTable[6, 3] = 31;
			m_iNormalExpTable[6, 4] = 27;
			m_iNormalExpTable[6, 5] = 23;
			m_iNormalExpTable[6, 6] = 19;
			m_iNormalExpTable[7, 1] = 50;
			m_iNormalExpTable[7, 2] = 40;
			m_iNormalExpTable[7, 3] = 36;
			m_iNormalExpTable[7, 4] = 32;
			m_iNormalExpTable[7, 5] = 28;
			m_iNormalExpTable[7, 6] = 24;
			m_iNormalExpTable[7, 7] = 20;
			m_iNormalExpTable[8, 1] = 55;
			m_iNormalExpTable[8, 2] = 45;
			m_iNormalExpTable[8, 3] = 41;
			m_iNormalExpTable[8, 4] = 37;
			m_iNormalExpTable[8, 5] = 32;
			m_iNormalExpTable[8, 6] = 28;
			m_iNormalExpTable[8, 7] = 24;
			m_iNormalExpTable[8, 8] = 20;
			for (num = 9; num <= 15; num++)
			{
				m_iNormalExpTable[num, 1] = 60;
				m_iNormalExpTable[num, 2] = 55;
				m_iNormalExpTable[num, 3] = 50;
				m_iNormalExpTable[num, 4] = 47;
				m_iNormalExpTable[num, 5] = 44;
				m_iNormalExpTable[num, 6] = 41;
				m_iNormalExpTable[num, 7] = 38;
				m_iNormalExpTable[num, 8] = 35;
				m_iNormalExpTable[num, 9] = 33;
				m_iNormalExpTable[num, 10] = 32;
				m_iNormalExpTable[num, 11] = 30;
				m_iNormalExpTable[num, 12] = 29;
				m_iNormalExpTable[num, 13] = 28;
				m_iNormalExpTable[num, 14] = 27;
				m_iNormalExpTable[num, 15] = 26;
			}
			for (num = 16; num <= 20; num++)
			{
				m_iNormalExpTable[num, 1] = 70;
				m_iNormalExpTable[num, 2] = 65;
				m_iNormalExpTable[num, 3] = 60;
				m_iNormalExpTable[num, 4] = 53;
				m_iNormalExpTable[num, 5] = 51;
				m_iNormalExpTable[num, 6] = 49;
				m_iNormalExpTable[num, 7] = 46;
				m_iNormalExpTable[num, 8] = 44;
				m_iNormalExpTable[num, 9] = 42;
				m_iNormalExpTable[num, 10] = 39;
				m_iNormalExpTable[num, 11] = 38;
				m_iNormalExpTable[num, 12] = 37;
				m_iNormalExpTable[num, 13] = 36;
				m_iNormalExpTable[num, 14] = 35;
				m_iNormalExpTable[num, 15] = 33;
				m_iNormalExpTable[num, 16] = 32;
				m_iNormalExpTable[num, 17] = 31;
				m_iNormalExpTable[num, 18] = 30;
				m_iNormalExpTable[num, 19] = 29;
				m_iNormalExpTable[num, 20] = 27;
			}
			for (num = 21; num <= 25; num++)
			{
				m_iNormalExpTable[num, 1] = 80;
				m_iNormalExpTable[num, 2] = 75;
				m_iNormalExpTable[num, 3] = 70;
				m_iNormalExpTable[num, 4] = 59;
				m_iNormalExpTable[num, 5] = 57;
				m_iNormalExpTable[num, 6] = 55;
				m_iNormalExpTable[num, 7] = 52;
				m_iNormalExpTable[num, 8] = 50;
				m_iNormalExpTable[num, 9] = 48;
				m_iNormalExpTable[num, 10] = 46;
				m_iNormalExpTable[num, 11] = 45;
				m_iNormalExpTable[num, 12] = 44;
				m_iNormalExpTable[num, 13] = 43;
				m_iNormalExpTable[num, 14] = 41;
				m_iNormalExpTable[num, 15] = 40;
				m_iNormalExpTable[num, 16] = 39;
				m_iNormalExpTable[num, 17] = 38;
				m_iNormalExpTable[num, 18] = 37;
				m_iNormalExpTable[num, 19] = 35;
				m_iNormalExpTable[num, 20] = 34;
				m_iNormalExpTable[num, 21] = 33;
				m_iNormalExpTable[num, 22] = 32;
				m_iNormalExpTable[num, 23] = 31;
				m_iNormalExpTable[num, 24] = 29;
				m_iNormalExpTable[num, 25] = 28;
			}
			for (num = 26; num <= 29; num++)
			{
				m_iNormalExpTable[num, 1] = 90;
				m_iNormalExpTable[num, 2] = 85;
				m_iNormalExpTable[num, 3] = 75;
				m_iNormalExpTable[num, 4] = 64;
				m_iNormalExpTable[num, 5] = 62;
				m_iNormalExpTable[num, 6] = 60;
				m_iNormalExpTable[num, 7] = 58;
				m_iNormalExpTable[num, 8] = 56;
				m_iNormalExpTable[num, 9] = 54;
				m_iNormalExpTable[num, 10] = 52;
				m_iNormalExpTable[num, 11] = 51;
				m_iNormalExpTable[num, 12] = 50;
				m_iNormalExpTable[num, 13] = 48;
				m_iNormalExpTable[num, 14] = 47;
				m_iNormalExpTable[num, 15] = 46;
				m_iNormalExpTable[num, 16] = 45;
				m_iNormalExpTable[num, 17] = 44;
				m_iNormalExpTable[num, 18] = 42;
				m_iNormalExpTable[num, 19] = 41;
				m_iNormalExpTable[num, 20] = 40;
				m_iNormalExpTable[num, 21] = 39;
				m_iNormalExpTable[num, 22] = 38;
				m_iNormalExpTable[num, 23] = 36;
				m_iNormalExpTable[num, 24] = 35;
				m_iNormalExpTable[num, 25] = 34;
				m_iNormalExpTable[num, 26] = 33;
				m_iNormalExpTable[num, 27] = 32;
				m_iNormalExpTable[num, 28] = 30;
				m_iNormalExpTable[num, 29] = 29;
			}
			num = 30;
			m_iNormalExpTable[num, 1] = 100;
			m_iNormalExpTable[num, 2] = 95;
			m_iNormalExpTable[num, 3] = 90;
			m_iNormalExpTable[num, 4] = 73;
			m_iNormalExpTable[num, 5] = 71;
			m_iNormalExpTable[num, 6] = 69;
			m_iNormalExpTable[num, 7] = 67;
			m_iNormalExpTable[num, 8] = 65;
			m_iNormalExpTable[num, 9] = 62;
			m_iNormalExpTable[num, 10] = 60;
			m_iNormalExpTable[num, 11] = 59;
			m_iNormalExpTable[num, 12] = 57;
			m_iNormalExpTable[num, 13] = 56;
			m_iNormalExpTable[num, 14] = 54;
			m_iNormalExpTable[num, 15] = 53;
			m_iNormalExpTable[num, 16] = 51;
			m_iNormalExpTable[num, 17] = 50;
			m_iNormalExpTable[num, 18] = 48;
			m_iNormalExpTable[num, 19] = 47;
			m_iNormalExpTable[num, 20] = 45;
			m_iNormalExpTable[num, 21] = 44;
			m_iNormalExpTable[num, 22] = 42;
			m_iNormalExpTable[num, 23] = 41;
			m_iNormalExpTable[num, 24] = 39;
			m_iNormalExpTable[num, 25] = 38;
			m_iNormalExpTable[num, 26] = 36;
			m_iNormalExpTable[num, 27] = 35;
			m_iNormalExpTable[num, 28] = 33;
			m_iNormalExpTable[num, 29] = 32;
			m_iNormalExpTable[num, 30] = 30;
			m_iNormalGameMoneyTable[2, 1] = 15;
			m_iNormalGameMoneyTable[2, 2] = 10;
			m_iNormalGameMoneyTable[3, 1] = 20;
			m_iNormalGameMoneyTable[3, 2] = 15;
			m_iNormalGameMoneyTable[3, 3] = 11;
			m_iNormalGameMoneyTable[4, 1] = 30;
			m_iNormalGameMoneyTable[4, 2] = 23;
			m_iNormalGameMoneyTable[4, 3] = 17;
			m_iNormalGameMoneyTable[4, 4] = 13;
			m_iNormalGameMoneyTable[5, 1] = 40;
			m_iNormalGameMoneyTable[5, 2] = 31;
			m_iNormalGameMoneyTable[5, 3] = 25;
			m_iNormalGameMoneyTable[5, 4] = 19;
			m_iNormalGameMoneyTable[5, 5] = 15;
			m_iNormalGameMoneyTable[6, 1] = 60;
			m_iNormalGameMoneyTable[6, 2] = 48;
			m_iNormalGameMoneyTable[6, 3] = 38;
			m_iNormalGameMoneyTable[6, 4] = 31;
			m_iNormalGameMoneyTable[6, 5] = 25;
			m_iNormalGameMoneyTable[6, 6] = 20;
			m_iNormalGameMoneyTable[7, 1] = 80;
			m_iNormalGameMoneyTable[7, 2] = 66;
			m_iNormalGameMoneyTable[7, 3] = 55;
			m_iNormalGameMoneyTable[7, 4] = 45;
			m_iNormalGameMoneyTable[7, 5] = 37;
			m_iNormalGameMoneyTable[7, 6] = 31;
			m_iNormalGameMoneyTable[7, 7] = 26;
			m_iNormalGameMoneyTable[8, 1] = 100;
			m_iNormalGameMoneyTable[8, 2] = 84;
			m_iNormalGameMoneyTable[8, 3] = 71;
			m_iNormalGameMoneyTable[8, 4] = 60;
			m_iNormalGameMoneyTable[8, 5] = 51;
			m_iNormalGameMoneyTable[8, 6] = 43;
			m_iNormalGameMoneyTable[8, 7] = 36;
			m_iNormalGameMoneyTable[8, 8] = 30;
			for (num = 9; num <= 15; num++)
			{
				m_iNormalGameMoneyTable[num, 1] = 100;
				m_iNormalGameMoneyTable[num, 2] = 92;
				m_iNormalGameMoneyTable[num, 3] = 84;
				m_iNormalGameMoneyTable[num, 4] = 77;
				m_iNormalGameMoneyTable[num, 5] = 71;
				m_iNormalGameMoneyTable[num, 6] = 65;
				m_iNormalGameMoneyTable[num, 7] = 59;
				m_iNormalGameMoneyTable[num, 8] = 54;
				m_iNormalGameMoneyTable[num, 9] = 50;
				m_iNormalGameMoneyTable[num, 10] = 46;
				m_iNormalGameMoneyTable[num, 11] = 42;
				m_iNormalGameMoneyTable[num, 12] = 38;
				m_iNormalGameMoneyTable[num, 13] = 35;
				m_iNormalGameMoneyTable[num, 14] = 32;
				m_iNormalGameMoneyTable[num, 15] = 30;
			}
			for (num = 16; num <= 20; num++)
			{
				m_iNormalGameMoneyTable[num, 1] = 110;
				m_iNormalGameMoneyTable[num, 2] = 103;
				m_iNormalGameMoneyTable[num, 3] = 96;
				m_iNormalGameMoneyTable[num, 4] = 89;
				m_iNormalGameMoneyTable[num, 5] = 83;
				m_iNormalGameMoneyTable[num, 6] = 78;
				m_iNormalGameMoneyTable[num, 7] = 73;
				m_iNormalGameMoneyTable[num, 8] = 68;
				m_iNormalGameMoneyTable[num, 9] = 63;
				m_iNormalGameMoneyTable[num, 10] = 59;
				m_iNormalGameMoneyTable[num, 11] = 55;
				m_iNormalGameMoneyTable[num, 12] = 51;
				m_iNormalGameMoneyTable[num, 13] = 48;
				m_iNormalGameMoneyTable[num, 14] = 45;
				m_iNormalGameMoneyTable[num, 15] = 42;
				m_iNormalGameMoneyTable[num, 16] = 39;
				m_iNormalGameMoneyTable[num, 17] = 36;
				m_iNormalGameMoneyTable[num, 18] = 34;
				m_iNormalGameMoneyTable[num, 19] = 32;
				m_iNormalGameMoneyTable[num, 20] = 30;
			}
			for (num = 21; num <= 25; num++)
			{
				m_iNormalGameMoneyTable[num, 1] = 120;
				m_iNormalGameMoneyTable[num, 2] = 113;
				m_iNormalGameMoneyTable[num, 3] = 107;
				m_iNormalGameMoneyTable[num, 4] = 101;
				m_iNormalGameMoneyTable[num, 5] = 95;
				m_iNormalGameMoneyTable[num, 6] = 90;
				m_iNormalGameMoneyTable[num, 7] = 85;
				m_iNormalGameMoneyTable[num, 8] = 80;
				m_iNormalGameMoneyTable[num, 9] = 75;
				m_iNormalGameMoneyTable[num, 10] = 71;
				m_iNormalGameMoneyTable[num, 11] = 67;
				m_iNormalGameMoneyTable[num, 12] = 63;
				m_iNormalGameMoneyTable[num, 13] = 60;
				m_iNormalGameMoneyTable[num, 14] = 56;
				m_iNormalGameMoneyTable[num, 15] = 53;
				m_iNormalGameMoneyTable[num, 16] = 50;
				m_iNormalGameMoneyTable[num, 17] = 47;
				m_iNormalGameMoneyTable[num, 18] = 45;
				m_iNormalGameMoneyTable[num, 19] = 42;
				m_iNormalGameMoneyTable[num, 20] = 40;
				m_iNormalGameMoneyTable[num, 21] = 37;
				m_iNormalGameMoneyTable[num, 22] = 35;
				m_iNormalGameMoneyTable[num, 23] = 33;
				m_iNormalGameMoneyTable[num, 24] = 31;
				m_iNormalGameMoneyTable[num, 25] = 30;
			}
			for (num = 26; num <= 29; num++)
			{
				m_iNormalGameMoneyTable[num, 1] = 130;
				m_iNormalGameMoneyTable[num, 2] = 124;
				m_iNormalGameMoneyTable[num, 3] = 117;
				m_iNormalGameMoneyTable[num, 4] = 111;
				m_iNormalGameMoneyTable[num, 5] = 106;
				m_iNormalGameMoneyTable[num, 6] = 101;
				m_iNormalGameMoneyTable[num, 7] = 96;
				m_iNormalGameMoneyTable[num, 8] = 91;
				m_iNormalGameMoneyTable[num, 9] = 86;
				m_iNormalGameMoneyTable[num, 10] = 82;
				m_iNormalGameMoneyTable[num, 11] = 78;
				m_iNormalGameMoneyTable[num, 12] = 74;
				m_iNormalGameMoneyTable[num, 13] = 70;
				m_iNormalGameMoneyTable[num, 14] = 67;
				m_iNormalGameMoneyTable[num, 15] = 63;
				m_iNormalGameMoneyTable[num, 16] = 60;
				m_iNormalGameMoneyTable[num, 17] = 57;
				m_iNormalGameMoneyTable[num, 18] = 54;
				m_iNormalGameMoneyTable[num, 19] = 52;
				m_iNormalGameMoneyTable[num, 20] = 49;
				m_iNormalGameMoneyTable[num, 21] = 47;
				m_iNormalGameMoneyTable[num, 22] = 44;
				m_iNormalGameMoneyTable[num, 23] = 42;
				m_iNormalGameMoneyTable[num, 24] = 40;
				m_iNormalGameMoneyTable[num, 25] = 38;
				m_iNormalGameMoneyTable[num, 26] = 36;
				m_iNormalGameMoneyTable[num, 27] = 34;
				m_iNormalGameMoneyTable[num, 28] = 33;
				m_iNormalGameMoneyTable[num, 29] = 31;
			}
			num = 30;
			m_iNormalGameMoneyTable[num, 1] = 150;
			m_iNormalGameMoneyTable[num, 2] = 143;
			m_iNormalGameMoneyTable[num, 3] = 135;
			m_iNormalGameMoneyTable[num, 4] = 129;
			m_iNormalGameMoneyTable[num, 5] = 122;
			m_iNormalGameMoneyTable[num, 6] = 116;
			m_iNormalGameMoneyTable[num, 7] = 110;
			m_iNormalGameMoneyTable[num, 8] = 105;
			m_iNormalGameMoneyTable[num, 9] = 100;
			m_iNormalGameMoneyTable[num, 10] = 95;
			m_iNormalGameMoneyTable[num, 11] = 90;
			m_iNormalGameMoneyTable[num, 12] = 85;
			m_iNormalGameMoneyTable[num, 13] = 80;
			m_iNormalGameMoneyTable[num, 14] = 77;
			m_iNormalGameMoneyTable[num, 15] = 73;
			m_iNormalGameMoneyTable[num, 16] = 69;
			m_iNormalGameMoneyTable[num, 17] = 66;
			m_iNormalGameMoneyTable[num, 18] = 63;
			m_iNormalGameMoneyTable[num, 19] = 60;
			m_iNormalGameMoneyTable[num, 20] = 57;
			m_iNormalGameMoneyTable[num, 21] = 54;
			m_iNormalGameMoneyTable[num, 22] = 51;
			m_iNormalGameMoneyTable[num, 23] = 49;
			m_iNormalGameMoneyTable[num, 24] = 46;
			m_iNormalGameMoneyTable[num, 25] = 44;
			m_iNormalGameMoneyTable[num, 26] = 42;
			m_iNormalGameMoneyTable[num, 27] = 40;
			m_iNormalGameMoneyTable[num, 28] = 38;
			m_iNormalGameMoneyTable[num, 29] = 36;
			m_iNormalGameMoneyTable[num, 30] = 34;
		}

		private static void initRelayTable()
		{
			int num = 0;
			int num2 = 0;
			for (num = 0; num < 7; num++)
			{
				for (num2 = 0; num2 < 7; num2++)
				{
					m_iRelayExpTable[num, num2] = 0;
					m_iRelayGameMoneyTable[num, num2] = 0;
				}
			}
			m_iRelayExpTable[2, 1] = 40;
			m_iRelayExpTable[2, 2] = 25;
			m_iRelayExpTable[2, 0] = 2;
			m_iRelayGameMoneyTable[2, 1] = 40;
			m_iRelayGameMoneyTable[2, 2] = 27;
			m_iRelayGameMoneyTable[2, 0] = 5;
			m_iRelayExpTable[3, 1] = 50;
			m_iRelayExpTable[3, 2] = 40;
			m_iRelayExpTable[3, 3] = 25;
			m_iRelayExpTable[3, 0] = 2;
			m_iRelayGameMoneyTable[3, 1] = 65;
			m_iRelayGameMoneyTable[3, 2] = 43;
			m_iRelayGameMoneyTable[3, 3] = 29;
			m_iRelayGameMoneyTable[3, 0] = 5;
			m_iRelayExpTable[4, 1] = 60;
			m_iRelayExpTable[4, 2] = 50;
			m_iRelayExpTable[4, 3] = 40;
			m_iRelayExpTable[4, 4] = 30;
			m_iRelayExpTable[4, 0] = 2;
			m_iRelayGameMoneyTable[4, 1] = 80;
			m_iRelayGameMoneyTable[4, 2] = 59;
			m_iRelayGameMoneyTable[4, 3] = 43;
			m_iRelayGameMoneyTable[4, 4] = 32;
			m_iRelayGameMoneyTable[4, 0] = 5;
			m_iRelayExpTable[5, 1] = 70;
			m_iRelayExpTable[5, 2] = 60;
			m_iRelayExpTable[5, 3] = 50;
			m_iRelayExpTable[5, 4] = 40;
			m_iRelayExpTable[5, 5] = 30;
			m_iRelayExpTable[5, 0] = 2;
			m_iRelayGameMoneyTable[5, 1] = 100;
			m_iRelayGameMoneyTable[5, 2] = 77;
			m_iRelayGameMoneyTable[5, 3] = 59;
			m_iRelayGameMoneyTable[5, 4] = 45;
			m_iRelayGameMoneyTable[5, 5] = 35;
			m_iRelayGameMoneyTable[5, 0] = 5;
			m_iRelayExpTable[6, 1] = 80;
			m_iRelayExpTable[6, 2] = 70;
			m_iRelayExpTable[6, 3] = 60;
			m_iRelayExpTable[6, 4] = 50;
			m_iRelayExpTable[6, 5] = 40;
			m_iRelayExpTable[6, 6] = 30;
			m_iRelayExpTable[6, 0] = 2;
			m_iRelayGameMoneyTable[6, 1] = 110;
			m_iRelayGameMoneyTable[6, 2] = 88;
			m_iRelayGameMoneyTable[6, 3] = 70;
			m_iRelayGameMoneyTable[6, 4] = 56;
			m_iRelayGameMoneyTable[6, 5] = 45;
			m_iRelayGameMoneyTable[6, 6] = 36;
			m_iRelayGameMoneyTable[6, 0] = 5;
		}
	}
}
