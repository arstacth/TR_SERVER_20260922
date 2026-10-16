using System.Collections.Generic;
using AgentServer.Packet.RoomServer;
using Akka.Actor;

namespace AgentServer.Structuring
{
	public class NormalRoom
	{
		public int RoomServerID;

		public int ID;

		public string Name;

		public string Password;

		public int IsTeamPlay;

		public int ItemType;

		public bool IsStepOn;

		public int MapNum = 1;

		public int RoomKindID;

		public byte MaxPlayersCount = 8;

		public byte SlotCount = 8;

		public bool HasPassword;

		public bool is8Player = true;

		public byte RoomMasterIndex;

		public bool isPlaying;

		public byte BuffType;

		public bool GMItem;

		public bool hasPiero;

		public bool hasAfreecaTV;

		public byte PlayerCount;

		public string guildName = string.Empty;

		public int GuildMatchRoomID;

		public int FarmIndex;

		public int FarmTypeNum;

		public string FarmName = string.Empty;

		public int FarmEXP;

		public string FarmMasterName = string.Empty;

		public byte ChatFarmType;

		public List<string> PlayerName = new List<string>();

		public bool hasFishingReward;

		public int RedTeamCount;

		public int BlueTeamCount;

		public int ItemNum = -1;

		public int BonusStageLevel;

		public void BroadcastToAll(int Session, byte[] packet)
		{
			if (RoomServer.RoomServerList.TryGetValue(RoomServerID, out var value))
			{
				value.Tell(new AG_TO_RM_TO_User(Session, ID, packet).ToArray());
			}
		}

		public void BroadcastToMe(int Session, byte[] packet)
		{
			if (RoomServer.RoomServerList.TryGetValue(RoomServerID, out var value))
			{
				value.Tell(new AG_TO_RM_TO_User_ME(Session, ID, packet).ToArray());
			}
		}
	}
}
