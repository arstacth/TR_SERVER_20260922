using System.Collections.Generic;
using AgentServer.Structuring;
using LocalCommons.Network;

namespace AgentServer.Packet.RoomServer
{
	public class RoomServerHandle
	{
		public static void CreateRoom_AddList(PacketReader reader)
		{
			int roomServerID = reader.ReadLEInt32();
			int iD = reader.ReadLEInt32();
			// Must be i32 — summer/event kinds (381+) were truncated as byte → 125.
			int roomKindId = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			bool hasPassword = !reader.ReadBoolean();
			byte playerCount = reader.ReadByte();
			byte slotCount = reader.ReadByte();
			bool isPlaying = !reader.ReadBoolean();
			bool isStepOn = reader.ReadBoolean();
			int itemType = reader.ReadLEInt32();
			int mapNum = reader.ReadLEInt32();
			reader.Offset += 4;
			int isTeamPlay = reader.ReadLEInt32();
			bool hasAfreecaTV = reader.ReadBoolean();
			bool hasPiero = reader.ReadBoolean();
			bool gMItem = reader.ReadBoolean();
			reader.Offset += 7;
			byte buffType = reader.ReadByte();
			reader.Offset += 3;
			int itemNum = reader.ReadLEInt32();
			reader.Offset += 4;
			int farmIndex = reader.ReadLEInt32();
			string guildName = string.Empty;
			int guildMatchRoomID = 0;
			if (roomKindId == 79)
			{
				int fixedLength2 = reader.ReadLEInt16();
				guildName = reader.ReadBig5StringSafe(fixedLength2);
				reader.Offset += 4;
				guildMatchRoomID = reader.ReadLEInt32();
				reader.Offset += 4;
			}
			NormalRoom normalRoom = new NormalRoom
			{
				RoomServerID = roomServerID,
				ID = iD,
				RoomKindID = roomKindId,
				Name = name,
				HasPassword = hasPassword,
				PlayerCount = playerCount,
				SlotCount = slotCount,
				isPlaying = isPlaying,
				IsStepOn = isStepOn,
				ItemType = itemType,
				IsTeamPlay = isTeamPlay,
				MapNum = mapNum,
				hasAfreecaTV = hasAfreecaTV,
				hasPiero = hasPiero,
				GMItem = gMItem,
				BuffType = buffType,
				ItemNum = itemNum,
				guildName = guildName,
				GuildMatchRoomID = guildMatchRoomID,
				FarmIndex = farmIndex
			};
			Rooms.AddOrUpdateRoom(normalRoom.ID, normalRoom);
		}

		public static void GamerRoom_PlayingUpdate(PacketReader reader)
		{
			int roomsession = reader.ReadLEInt32();
			bool isPlaying = reader.ReadBoolean();
			int bonusStageLevel = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(roomsession);
			if (room != null)
			{
				room.isPlaying = isPlaying;
				room.BonusStageLevel = bonusStageLevel;
			}
		}

		public static void GamerRoom_ChangeMap(PacketReader reader)
		{
			int roomsession = reader.ReadLEInt32();
			int mapNum = reader.ReadLEInt32();
			Rooms.GetRoom(roomsession).MapNum = mapNum;
		}

		public static void GamerRoom_ChangeSetting(PacketReader reader)
		{
			NormalRoom room = Rooms.GetRoom(reader.ReadLEInt32());
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			string text = string.Empty;
			if (num > 0)
			{
				text = reader.ReadBig5StringSafe(num);
			}
			bool isStepOn = reader.ReadBoolean();
			int itemType = reader.ReadLEInt32();
			room.Name = name;
			room.Password = text;
			room.HasPassword = !string.IsNullOrEmpty(text);
			room.IsStepOn = isStepOn;
			room.ItemType = itemType;
		}

		public static void AddPublicFarmList(PacketReader reader)
		{
			int iD = reader.ReadLEInt32();
			int farmIndex = reader.ReadLEInt32();
			int farmTypeNum = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			bool hasPassword = reader.ReadBoolean();
			byte b = reader.ReadByte();
			byte b2 = reader.ReadByte();
			reader.Offset += 4;
			reader.Offset += 2;
			b = reader.ReadByte();
			int farmEXP = reader.ReadLEInt32();
			reader.Offset += 4;
			int fixedLength2 = reader.ReadLEInt16();
			string farmMasterName = reader.ReadBig5StringSafe(fixedLength2);
			byte chatFarmType = reader.ReadByte();
			b2 = reader.ReadByte();
			reader.Offset++;
			reader.Offset++;
			int num = reader.ReadLEInt32();
			List<string> list = new List<string>();
			for (int i = 0; i < num; i++)
			{
				int fixedLength3 = reader.ReadLEInt16();
				string item = reader.ReadBig5StringSafe(fixedLength3);
				list.Add(item);
			}
			bool hasFishingReward = reader.ReadBoolean();
			NormalRoom normalRoom = new NormalRoom
			{
				ID = iD,
				FarmTypeNum = farmTypeNum,
				Name = name,
				HasPassword = hasPassword,
				PlayerCount = b,
				MaxPlayersCount = b2,
				FarmEXP = farmEXP,
				FarmIndex = farmIndex,
				FarmMasterName = farmMasterName,
				ChatFarmType = chatFarmType,
				PlayerName = list,
				hasFishingReward = hasFishingReward
			};
			Rooms.AddOrUpdatePublicFarmRoom(normalRoom.ID, normalRoom);
		}

		public static void UpdateFarmFishingReward(PacketReader reader)
		{
			int num = reader.ReadLEInt32();
			bool hasFishingReward = reader.ReadBoolean();
			NormalRoom room = Rooms.GetRoom(num);
			if (room != null)
			{
				room.hasFishingReward = hasFishingReward;
				if (Rooms.PublicFarmRoom.TryGetValue(num, out var value))
				{
					value.hasFishingReward = hasFishingReward;
				}
			}
		}

		public static void UpdateTeamInfo(PacketReader reader)
		{
			int roomsession = reader.ReadLEInt32();
			int redTeamCount = reader.ReadLEInt32();
			int blueTeamCount = reader.ReadLEInt32();
			NormalRoom room = Rooms.GetRoom(roomsession);
			if (room != null)
			{
				room.RedTeamCount = redTeamCount;
				room.BlueTeamCount = blueTeamCount;
			}
		}
	}
}
