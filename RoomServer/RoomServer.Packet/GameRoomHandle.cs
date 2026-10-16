using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using LocalCommons.Network;
using LocalCommons.Utilities;
using NetMsg.Room;
using RoomServer.Holders;
using RoomServer.Packet.Send;
using RoomServer.Structuring;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Guild;
using RoomServer.Structuring.Item;
using RoomServer.Structuring.Room;
using RoomServer.Structuring.Shu;
using RoomServer.Structuring.User;
using Serilog;
using TRCommon;

namespace RoomServer.Packet
{
	public class GameRoomHandle
	{
		public static void Handle_CreateGameRoom(IActorRef Sender, PacketReader reader, byte last)
		{
			if (!EnterRoomGetUserInfo(Sender, reader, out var User))
			{
				return;
			}
			reader.ReadLEInt32();
			int num = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string name = reader.ReadBig5StringSafe(fixedLength);
			int num2 = reader.ReadLEInt16();
			string password = string.Empty;
			if (num2 > 0)
			{
				password = reader.ReadBig5StringSafe(num2);
			}
			int isTeamPlay = reader.ReadLEInt32();
			bool isStepOn = reader.ReadBoolean();
			int itemType = reader.ReadLEInt32();
			int num3 = reader.ReadLEInt32();
			if (num == 79 && (User.GuildNum <= 0 || User.GuildInfo == null))
			{
				User.SendAsync(new GameRoom_CreateRoomError(9, last));
				Log.Error("Invalid GuildInfo userNum : {0}", User.UserNum);
				return;
			}
			// Park (74) / farm (75) / myroom (76) are not race-map kinds — skip map↔kind table.
			if ((!MapHolder.MapRoomKinds.TryGetValue(num, out var value) || !value.Contains(num3)) && ServerSettingHolder.ServerSettings.useMapNumByRoomKindIDCheck && num3 != 0 && num3 != 74 && num3 != 75 && num3 != 76)
			{
				if (value != null && value.Count > 0)
				{
					Log.Warning("invalid MapNum By RoomKind ID - userNum : {0}, roomKind : {1}, mapNum : {2}, using {3}", User.UserNum, num, num3, value[0]);
					num3 = value[0];
				}
				else
				{
					Log.Warning("invalid MapNum By RoomKind ID - userNum : {0}, roomKind : {1}, mapNum : {2} (creating anyway)", User.UserNum, num, num3);
				}
			}
			if (MapHolder.AssaultModeLimitInfos.TryGetValue(num, out var value2) && User.AttackPoint < value2)
			{
				User.SendAsync(new GameRoom_EnterRoomError(12, (byte)num, last));
				return;
			}
			if (!RoomHolder.RoomKindInfos.TryGetValue(num, out var value3))
			{
				Log.Error("Invalid RoomKind ID:{0} userNum: {1}", num, User.UserNum);
				return;
			}
			int mapNum;
			if (num == 74)
			{
				mapNum = num3;
			}
			else if (num3 <= 0)
			{
				mapNum = (MapHolder.MapRoomKinds.TryGetValue(num, out var kindMaps) && kindMaps.Count > 0) ? kindMaps[0] : 1;
			}
			else
			{
				mapNum = num3;
			}
			RoomSettings settings = new RoomSettings
			{
				Name = name,
				Password = password,
				IsTeamPlay = isTeamPlay,
				ItemType = itemType,
				IsStepOn = isStepOn,
				MapNum = mapNum,
				RoomKindID = num,
				roomkindinfo = value3
			};
			lock (User.CreateRoomLock)
			{
				if (!User.InGame && User.CurrentRoomId == 0)
				{
					NormalRoom normalRoom = new NormalRoom(User, settings, last);
					User.SendAsync(new GameRoom_GoodsInfo(normalRoom, last));
					User.SendAsync(new GameRoom_EnterRoomOK(last));
					User.SendAsync(new GameRoom_SendRoomInfo(normalRoom, last, User.RoomPos));
					try
					{
						User.SendAsync(new GameRoom_SendPlayerInfo(User, normalRoom.RoomKindID, last));
					}
					catch (Exception ex)
					{
						Log.Error("SendPlayerInfo failed: {0}", ex);
						User.SendAsync(new GameRoom_EnterRoomError(67, (byte)num, last));
						return;
					}
					User.SendAsync(new GameRoom_GetRoomMaster(normalRoom.RoomMasterIndex, last));
					normalRoom.GetSortedHeroClassGameIndices();
					User.SendAsync(new GameRoom_UnknownResponse(last));
					User.SendAsync(new GameRoom_ChangeMap_FF0906(normalRoom.MapNum, last));
				}
			}
		}

		public static void Handle_LeaveRoom(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			try
			{
				if (value.InGame)
				{
					room?.LeaveRoom(value, flag, last);
				}
				if (flag)
				{
					AgentServer.CurrentAccounts.TryRemove(key, out var _);
				}
			}
			catch (Exception ex)
			{
				Log.Error("Player [{0}] error on leave room:\r\n{1}", value.NickName, ex.ToString());
			}
		}

		public static void Handle_GameEndInfo(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (!value.InGame || !room.isPlaying)
			{
				return;
			}
			float num = reader.ReadLESingle();
			room.MapMaxDistance = reader.ReadLESingle();
			short num2 = reader.ReadLEInt16();
			Utility.CurrentTimeMilliseconds();
			Log.Debug("Game End - Nickname: {0}, GameEndType: {1}, RaceDistance: {2}", value.NickName, num2, num);
			switch (num2)
			{
			case 1:
				if (value.GameEndType != 0)
				{
					break;
				}
				value.GameEndType = num2;
				value.RaceDistance = num;
				if (room.GameMode != 5)
				{
					break;
				}
				{
					foreach (Account item in room.Players.Values.OrderBy((Account o) => o.RoomPos))
					{
						item.GameEndType = 1;
						item.RaceDistance = num;
					}
					break;
				}
			case 2:
				value.GameEndType = num2;
				value.RaceDistance = num;
				value.LapTime = room.GetCurrentTime();
				value.ServerLapTime = value.LapTime;
				break;
			case 3:
				value.GameEndType = num2;
				value.RaceDistance = num;
				value.LapTime = 980000000;
				value.ServerLapTime = value.LapTime;
				break;
			case 4:
				value.GameEndType = num2;
				value.RaceDistance = num;
				value.LapTime = 990000000;
				value.ServerLapTime = value.LapTime;
				break;
			case 5:
				value.GameEndType = num2;
				value.RaceDistance = num;
				break;
			}
		}

		public static void Handle_PlayerList(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
				if (room != null)
				{
					value.SendAsync(new GameRoom_PlayerPosList(room.PlayerList(), last));
				}
			}
		}

		public static void Handle_EnterRoom(IActorRef Sender, PacketReader reader, byte last)
		{
			if (!EnterRoomGetUserInfo(Sender, reader, out var User))
			{
				return;
			}
			int num = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt16();
			string pw = string.Empty;
			if (num2 > 0)
			{
				pw = reader.ReadBig5StringSafe(num2);
			}
			int num3 = reader.ReadLEInt32();
			reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (Rooms.ExistRoom(num))
			{
				Rooms.GetRoom(num).EnterRoom(User, pw, last);
				if ((num3 == 75 || num3 == 76) && User.MyFarmUniqueNum == User.CurrentFarmUniqueNum)
				{
					User.SendAsync(new GetCurrentMapFarmCraftItem(new List<FarmCraftMapItem>(), last));
				}
				return;
			}
			foreach (IActorRef value in AgentServer.AgentServerList.Values)
			{
				value.Tell(new RemoveRoom
				{
					RoomID = num
				});
			}
			User.SendAsync(new GameRoom_EnterRoomError(1, num3, last));
		}

		public static void Handle_KickPlayer(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string nickname = reader.ReadBig5StringSafe(fixedLength);
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			NormalRoom room = Rooms.GetRoom(value.CurrentRoomId);
			if (room.Players.Values.Any((Account p) => p.NickName == nickname))
			{
				Account account = room.Players.Values.FirstOrDefault((Account p) => p.NickName == nickname);
				if (room.RoomMasterIndex == value.RoomPos && room.RoomKindID != 74 && account.Attribute == 0 && !room.isPlaying)
				{
					room.KickPlayer(account, last);
				}
				if (value.Attribute != 0)
				{
					room.KickPlayer(account, last);
				}
			}
		}

		public static void Handle_UpdateGuild(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			reader.ReadLEInt32();
			if (AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				int num = reader.ReadLEInt16();
				string name = string.Empty;
				if (num > 0)
				{
					name = reader.ReadBig5StringSafe(num);
				}
				int num2 = reader.ReadLEInt16();
				string text = string.Empty;
				if (num2 > 0)
				{
					text = reader.ReadBig5StringSafe(num2);
				}
				value.GuildInfo.guildName = text;
				int guildnum = (value.GuildNum = reader.ReadLEInt32());
				int grade = reader.ReadLEInt32();
				value.GuildUserInfo.grade = grade;
				int unk = reader.ReadLEInt32();
				short num4 = reader.ReadLEInt16();
				value.GuildInfo.level = num4;
				Rooms.GetRoom(value.CurrentRoomId)?.BroadcastToAll(new GameRoomUpdateGuildACK3(value.RoomPos, name, text, guildnum, grade, unk, num4, last));
			}
		}

		public static void Handle_PassVertification(PacketReader reader, byte last)
		{
			int key = reader.ReadLEInt32();
			if (!AgentServer.CurrentAccounts.TryGetValue(key, out var value))
			{
				return;
			}
			if (reader.ReadByte() != value.VertificationCode)
			{
				value.WrongTime++;
				value.VertificationCode = (byte)new Random(Guid.NewGuid().GetHashCode()).Next(0, 100);
				if (value.WrongTime >= 3)
				{
					value.SendAsync(new ClientCheckAutoBanACK(7, last));
					value.isDisconnected = true;
					GameRoomEvent.VertificationBan(value);
				}
				else
				{
					value.SendAsync(new GameRoom_PassVertification(value, 271, last));
				}
			}
			else
			{
				value.WrongTime = 0;
				value.NeedVertificated = false;
				value.SendAsync(new GameRoom_PassVertification(value, 0, last));
			}
		}

		public static bool EnterRoomGetUserInfo(IActorRef Sender, PacketReader reader, out Account User)
		{
			try
			{
				int num = reader.ReadLEInt32();
				int num2 = reader.ReadLEInt32();
				if (!AgentServer.AgentServerList.TryGetValue(num2, out var value))
				{
					User = null;
					return false;
				}
				if (!AgentServer.CurrentAccounts.TryGetValue(num, out User))
				{
					User = new Account();
					User.Session = num;
					User.ServerID = num2;
					AgentServer.CurrentAccounts.TryAdd(User.Session, User);
				}
				User.AgentServer = value;
				User.AgentConnect = Sender;
				int userNum = reader.ReadLEInt32();
				User.UserNum = userNum;
				int fixedLength = reader.ReadLEInt16();
				string nickName = reader.ReadBig5StringSafe(fixedLength);
				int fixedLength2 = reader.ReadLEInt16();
				string userID = reader.ReadBig5StringSafe(fixedLength2);
				User.NickName = nickName;
				User.UserID = userID;
				byte[] uDPInfo = reader.ReadByteArray(48);
				User.UDPInfo = uDPInfo;
				int fixedLength3 = reader.ReadLEInt16();
				User.LastIp = reader.ReadBig5StringSafe(fixedLength3);
				GuildInfo guildInfo = new GuildInfo();
				GuildUserInfo guildUserInfo = new GuildUserInfo();
				int num3 = reader.ReadLEInt16();
				int guildNum = 0;
				if (num3 > 0)
				{
					guildInfo.guildName = reader.ReadBig5StringSafe(num3);
					guildNum = reader.ReadLEInt32();
					reader.Offset += 4;
					guildInfo.level = reader.ReadLEInt16();
					guildUserInfo.grade = reader.ReadLEInt16();
				}
				else
				{
					reader.Offset += 12;
				}
				User.GuildNum = guildNum;
				User.GuildInfo = guildInfo;
				User.GuildUserInfo = guildUserInfo;
				reader.Offset++;
				List<UserItemDyeing> list = new List<UserItemDyeing>();
				list.AddRange(Enumerable.Repeat(new UserItemDyeing(), 24));
				AvatarInfo realInfo = User.advancedAvatarInfo.m_realAvatarInfo;
				for (int i = 0; i < 15; i++)
				{
					realInfo.SetWear(i, reader.ReadLEUInt16());
				}
				User.advancedAvatarInfo.m_realAvatarInfo = realInfo;
				for (int j = 0; j < 12; j++)
				{
					UserItemDyeing userItemDyeing2 = (list[j] = new UserItemDyeing
					{
						DyeingPart = reader.ReadByte(),
						Color1 = reader.ReadByteArray(3),
						Color2 = reader.ReadByteArray(3),
						Color3 = reader.ReadByteArray(3)
					});
				}
				AvatarInfo costumeInfo = User.advancedAvatarInfo.m_costumeAvatarInfo;
				for (byte b = 0; b < 15; b = (byte)(b + 1))
				{
					costumeInfo.SetWear(b, reader.ReadLEUInt16());
				}
				User.advancedAvatarInfo.m_costumeAvatarInfo = costumeInfo;
				for (int k = 12; k < 24; k++)
				{
					UserItemDyeing userItemDyeing4 = (list[k] = new UserItemDyeing
					{
						DyeingPart = reader.ReadByte(),
						Color1 = reader.ReadByteArray(3),
						Color2 = reader.ReadByteArray(3),
						Color3 = reader.ReadByteArray(3)
					});
				}
				User.advancedAvatarInfo.m_bIsUseCostume = reader.ReadBoolean();
				reader.Offset++;
				User.AvatarItemDyeing = list;
				reader.Offset += 2;
				int attribute = reader.ReadLEInt32();
				long tR = reader.ReadLEInt64();
				long exp = reader.ReadLEInt64();
				float num4 = reader.ReadLESingle();
				User.Attribute = attribute;
				User.TR = tR;
				User.Exp = exp;
				User.Luck = (decimal)num4;
				int coupleNum = reader.ReadLEInt32();
				int coupleType = reader.ReadLEInt32();
				int num5 = reader.ReadLEInt16();
				string mateName = string.Empty;
				if (num5 > 0)
				{
					mateName = reader.ReadBig5StringSafe(num5);
				}
				long createTime = reader.ReadLEInt64();
				long marriedTime = reader.ReadLEInt64();
				long ringChangedTime = reader.ReadLEInt64();
				int coupleRingNum = reader.ReadLEInt32();
				int maxRingDays = reader.ReadLEInt32();
				short coupleLevel = reader.ReadLEInt16();
				reader.Offset += 2;
				int condDays = reader.ReadLEInt32();
				int accumulateExp = reader.ReadLEInt32();
				int coupleRank = reader.ReadLEInt32();
				int couplePoint = reader.ReadLEInt32();
				reader.Offset += 4;
				UserCoupleInfo coupleInfo = new UserCoupleInfo
				{
					CoupleNum = coupleNum,
					CoupleType = coupleType,
					MateName = mateName,
					CreateTime = createTime,
					MarriedTime = marriedTime,
					RingChangedTime = ringChangedTime,
					CoupleRingNum = coupleRingNum,
					MaxRingDays = maxRingDays,
					CoupleLevel = coupleLevel,
					CondDays = condDays,
					AccumulateExp = accumulateExp,
					CoupleRank = coupleRank,
					CouplePoint = couplePoint
				};
				reader.Offset++;
				User.CoupleInfo = coupleInfo;
				reader.Offset++;
				User.activeItem.decodeActiveItems(reader);
				User.userItemAttr.decodeUserItemAttr(reader);
				User.userItemAttr.decodeUserCharAttr(reader);
				int myFarmUniqueNum = reader.ReadLEInt32();
				User.MyFarmUniqueNum = myFarmUniqueNum;
				int farmWeatherTypeNum = reader.ReadLEInt32();
				int farmSkyTypeNum = reader.ReadLEInt32();
				int farmTypeNum = reader.ReadLEInt32();
				int num6 = reader.ReadLEInt16();
				string farmName = string.Empty;
				if (num6 > 0)
				{
					farmName = reader.ReadBig5StringSafe(num6);
				}
				int num7 = reader.ReadLEInt16();
				string masterName = string.Empty;
				if (num7 > 0)
				{
					masterName = reader.ReadBig5StringSafe(num7);
				}
				long expireTime = reader.ReadLEInt64();
				long createTime2 = reader.ReadLEInt64();
				reader.Offset++;
				bool isPublic = reader.ReadBoolean();
				int totalCount = reader.ReadLEInt32();
				int todaysVisitorCount = reader.ReadLEInt32();
				bool premiumFarmUsing = reader.ReadBoolean();
				long premiumFarmExpireDateTime = reader.ReadLEInt64();
				int farmExp = reader.ReadLEInt32();
				reader.Offset += 4;
				reader.Offset++;
				MyFarmInfo myFarmInfo = new MyFarmInfo
				{
					FarmWeatherTypeNum = farmWeatherTypeNum,
					FarmSkyTypeNum = farmSkyTypeNum,
					FarmTypeNum = farmTypeNum,
					FarmName = farmName,
					MasterName = masterName,
					ExpireTime = expireTime,
					CreateTime = createTime2,
					isPublic = isPublic,
					TotalCount = totalCount,
					TodaysVisitorCount = todaysVisitorCount,
					PremiumFarmUsing = premiumFarmUsing,
					PremiumFarmExpireDateTime = premiumFarmExpireDateTime,
					farmExp = farmExp
				};
				User.MyFarmInfo = myFarmInfo;
				int num8 = reader.ReadLEInt32();
				User.PartyType = num8;
				reader.ReadLEInt32();
				User.SubPartyType = num8;
				bool flag = reader.ReadBoolean();
				User.UseShu = flag;
				if (flag)
				{
					int fixedLength4 = reader.ReadLEInt16();
					string shuName = reader.ReadBig5StringSafe(fixedLength4);
					int shuItemNum = reader.ReadLEInt32();
					int value2 = reader.ReadLEInt32();
					List<int> list2 = new List<int>(4) { 0, 0, 0, 0 };
					list2[3] = value2;
					reader.Offset += 4;
					List<short> list3 = new List<short>(6) { 0, 0, 0, 0, 0, 0 };
					short num9 = (short)(reader.ReadLEInt16() / 2);
					for (int l = 0; l < num9; l++)
					{
						list3[l] = reader.ReadLEInt16();
					}
					short num10 = (short)(reader.ReadLEInt16() / 4);
					for (int m = 0; m < num10; m++)
					{
						list2[m] = reader.ReadLEInt32();
					}
					long motionList = reader.ReadLEInt64();
					UserShuInfo userShuInfo = new UserShuInfo
					{
						ShuName = shuName,
						ShuItemNum = shuItemNum,
						ShuAvatarKind = list3,
						Statusinfo = list2,
						MotionList = motionList
					};
					User.UserShuInfo = userShuInfo;
				}
				int topRank = reader.ReadLEInt32();
				User.TopRank = topRank;
				User.avatarLock.decode(reader);
				User.GetMyLevel();
				User.charAbilityAttrMakeAttr();
				return true;
			}
			catch (Exception ex)
			{
				User = null;
				Log.Error("EnterRoom Error:{0}", ex.ToString());
				return false;
			}
		}
	}
}
