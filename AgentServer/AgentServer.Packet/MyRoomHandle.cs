using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using AgentServer;
using AgentServer.Database;
using AgentServer.Holders;
using AgentServer.Network.Connections;
using AgentServer.Packet.RoomServer;
using AgentServer.Packet.Send;
using AgentServer.Structuring;
using AgentServer.Structuring.Item;
using LocalCommons.Network;
using LocalCommons.Utilities;
using MySql.Data.MySqlClient;
using Serilog;
using TRCommon;

namespace AgentServer.Packet
{
	public class MyRoomHandle
	{
		public static void Handle_MyRoomGetCharacterList(ClientConnection Client, byte last)
		{
			ItemHandle.getActiveFuncItem(Client.CurrentAccount, -1);
			Client.SendAsync(new MyRoom_GetCharacterList(Client.CurrentAccount, last));
		}

		public static void Handle_MyRoomGetMyCards(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short num = reader.ReadLEInt16();
			string text = string.Empty;
			if (num > 0)
			{
				text = reader.ReadBig5StringSafe(num);
			}
			bool flag = !string.IsNullOrEmpty(text);
			AlchemistHandle.GetMyAlchemistCards(currentAccount.UserNum, text, out var cards);
			Client.SendAsync(new MyRoom_GetMyAlchemistCards(flag, cards, last));
		}

		public static void Handle_MyRoomGetCharacterSetting(ClientConnection Client, PacketReader reader, byte last)
		{
			int characterKind = reader.ReadLEUInt16();
			if (myRoomGetCharacterSetting(Client.CurrentAccount, characterKind, out var m_realAvatarInfo))
			{
				Client.SendAsync(new MyRoom_GetCharacterSetting_ACK(m_realAvatarInfo, last));
			}
			else
			{
				Client.SendAsync(new MyRoom_GetCharacterSetting_Fail_ACK(last));
			}
		}

		public static void Handle_SaveCharSetting(ClientConnection Client, PacketReader reader, byte last)
		{
			try
			{
				Account currentAccount = Client.CurrentAccount;
				bool bWrongData = false;
				reader.ReadLEInt64();
				int dyePad = Conf.AvatarDyePadBytes;
				AdvancedAvatarInfo advancedAvatarInfo = new AdvancedAvatarInfo();
				AdvancedAvatarInfo advancedAvatarInfo2 = new AdvancedAvatarInfo();
				for (int i = 0; i < 15; i++)
				{
					advancedAvatarInfo2.m_realAvatarInfo.m_nItemPartArry[i] = reader.ReadLEUInt16();
				}
				for (int j = 0; j < 7; j++)
				{
					advancedAvatarInfo2.m_realAvatarInfo.m_nGameAccArry[j] = reader.ReadLEUInt16();
				}
				for (int k = 0; k < 1; k++)
				{
					advancedAvatarInfo2.m_realAvatarInfo.m_nEFItemArry[k] = reader.ReadLEUInt16();
				}
				reader.Offset += dyePad;
				for (int l = 0; l < 15; l++)
				{
					advancedAvatarInfo2.m_costumeAvatarInfo.m_nItemPartArry[l] = reader.ReadLEUInt16();
				}
				for (int m = 0; m < 7; m++)
				{
					advancedAvatarInfo2.m_costumeAvatarInfo.m_nGameAccArry[m] = reader.ReadLEUInt16();
				}
				for (int n = 0; n < 1; n++)
				{
					advancedAvatarInfo2.m_costumeAvatarInfo.m_nEFItemArry[n] = reader.ReadLEUInt16();
				}
				reader.Offset += dyePad;
				if (reader.Remaining > 0)
				{
					advancedAvatarInfo2.m_bIsUseCostume = reader.ReadBoolean();
				}
				if (reader.Remaining > 0)
				{
					reader.Offset++;
				}
				// TRTHOLD: read wire chg flags but ignore — always persist both halves.
				// Costume mode SP is separate; wire mode still lands in memory via ShallowCopy.
				if (reader.Remaining > 0)
				{
					reader.ReadBoolean();
				}
				if (reader.Remaining > 0)
				{
					reader.ReadBoolean();
				}
				if (reader.Remaining > 0)
				{
					reader.ReadBoolean();
				}
				bool bChangeRealAvatar = true;
				bool bChangeCostumeAvatar = true;
				bool bChangeAvatarMode = false;
				Dictionary<cpk_type, cpk_type> dictionary = new Dictionary<cpk_type, cpk_type>();
				ReadCharSettingPartPairs(reader, advancedAvatarInfo2.m_realAvatarInfo, dictionary);
				Dictionary<cpk_type, cpk_type> dictionary2 = new Dictionary<cpk_type, cpk_type>();
				ReadCharSettingPartPairs(reader, advancedAvatarInfo2.m_costumeAvatarInfo, dictionary2);
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting RAW user={0} real={1} cos={2} mode={3} pairs={4}/{5}",
					currentAccount.UserID,
					DumpWear(advancedAvatarInfo2.m_realAvatarInfo),
					DumpWear(advancedAvatarInfo2.m_costumeAvatarInfo),
					advancedAvatarInfo2.m_bIsUseCostume,
					dictionary.Count,
					dictionary2.Count);
				}
				// TRTHOLD: no SanitizeSuitApply / KeepWorn. usp_myRoomSetCharacterSetting
				// already treats 65535 as KEEP and 0 as clear — C# must not invert that.
				if (!advancedAvatarInfo2.isValidCharacter)
				{
					Log.Warning("{0} - Not Invalid AdvancedAvatar Character Setting!!.", currentAccount.UserID);
					Client.SendAsync(new MyroomSetCharSettingFail(last));
					return;
				}
				advancedAvatarInfo = advancedAvatarInfo2.ShallowCopy();
				if (currentAccount.advancedAvatarInfo is null)
				{
					currentAccount.advancedAvatarInfo = new AdvancedAvatarInfo();
				}
				if (ItemHandle.checkInvalidItemDescNum(currentAccount, currentAccount.advancedAvatarInfo.m_realAvatarInfo, ref advancedAvatarInfo.m_realAvatarInfo, ref advancedAvatarInfo2.m_realAvatarInfo, dictionary, ref bWrongData) && ItemHandle.checkInvalidItemDescNum(currentAccount, currentAccount.advancedAvatarInfo.m_costumeAvatarInfo, ref advancedAvatarInfo.m_costumeAvatarInfo, ref advancedAvatarInfo2.m_costumeAvatarInfo, dictionary2, ref bWrongData))
				{
					if (MyRoomSetCharacterSetting(currentAccount.UserNum, advancedAvatarInfo, advancedAvatarInfo2, bChangeRealAvatar, bChangeCostumeAvatar, bChangeAvatarMode))
					{
						// Persist costume mode when wire toggled (Thai needs DB; TRTHOLD skipped SP).
						if (currentAccount.advancedAvatarInfo.m_bIsUseCostume != advancedAvatarInfo2.m_bIsUseCostume)
						{
							MyRoomSetCharacterSetting(currentAccount.UserNum, advancedAvatarInfo, advancedAvatarInfo2,
								bChangeRealAvatar: false, bChangeCostumeAvatar: false, bChangeAvatarMode: true);
						}
						// Memory: overlay wire; 65535 keeps previous slot (SP + TRTHOLD setAvatarInfoAndSendTCP).
						MergeWireKeepMaxIntoAccount(currentAccount, advancedAvatarInfo2);
						if (Conf.ProtocolDebug)
						{
							Log.Warning("SaveCharSetting OK user={0} realChar={1} realTop={2} cosChar={3} cosTop={4} mode={5} remain={6}",
							currentAccount.UserID,
							(int)currentAccount.advancedAvatarInfo.m_realAvatarInfo.m_character,
							(int)currentAccount.advancedAvatarInfo.m_realAvatarInfo.m_topBody,
							(int)currentAccount.advancedAvatarInfo.m_costumeAvatarInfo.m_character,
							(int)currentAccount.advancedAvatarInfo.m_costumeAvatarInfo.m_topBody,
							currentAccount.advancedAvatarInfo.m_bIsUseCostume,
							reader.Remaining);
						}
						Client.SendAsync(new MyroomSetCharSettingOK(last));
					}
					else
					{
						Client.SendAsync(new MyroomSetCharSettingFail(last));
					}
				}
				else
				{
					Log.Warning("{0} - save character setting {1} character failed!!.", currentAccount.UserID, (ushort)advancedAvatarInfo2.m_realAvatarInfo.m_character);
					Client.SendAsync(new MyroomSetCharSettingFail(last));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, "{0} - save character setting parse failed.", Client.CurrentAccount.UserID);
				Client.SendAsync(new MyroomSetCharSettingFail(last));
			}
		}

		/// <summary>
		/// Apply wire into account memory: 65535 = keep previous (matches SP + TRTHOLD).
		/// </summary>
		private static void MergeWireKeepMaxIntoAccount(Account user, AdvancedAvatarInfo wire)
		{
			if (user?.advancedAvatarInfo is null || wire is null)
			{
				return;
			}
			AdvancedAvatarInfo mem = user.advancedAvatarInfo;
			AvatarInfo real = mem.m_realAvatarInfo;
			AvatarInfo cos = mem.m_costumeAvatarInfo;
			OverlayUnlessMaxValue(ref real, wire.m_realAvatarInfo);
			OverlayUnlessMaxValue(ref cos, wire.m_costumeAvatarInfo);
			mem.m_realAvatarInfo = real;
			mem.m_costumeAvatarInfo = cos;
			mem.m_bIsUseCostume = wire.m_bIsUseCostume;
			user.advancedAvatarInfo = mem;
		}

		private static void OverlayUnlessMaxValue(ref AvatarInfo dest, AvatarInfo src)
		{
			for (int i = 0; i < 15; i++)
			{
				ushort v = src.GetWear(i);
				if (v != ushort.MaxValue)
				{
					dest.SetWear(i, v);
				}
			}
			for (int i = 0; i < 7; i++)
			{
				ushort v = src.GetAcc(i);
				if (v != ushort.MaxValue)
				{
					dest.SetAcc(i, v);
				}
			}
			if (src.GetEF() != ushort.MaxValue)
			{
				dest.SetEF(src.GetEF());
			}
		}

		private static void ReadCharSettingPartPairs(PacketReader reader, AvatarInfo parts, Dictionary<cpk_type, cpk_type> map)
		{
			if (reader.Remaining < 4)
			{
				return;
			}
			int count = reader.ReadLEInt32();
			int maxPairs = reader.Remaining / 4;
			if (count < 0)
			{
				count = 0;
			}
			if (count > maxPairs)
			{
				count = maxPairs;
			}
			for (int i = 0; i < count; i++)
			{
				if (reader.Remaining < 4)
				{
					break;
				}
				int slot = reader.ReadUInt16();
				cpk_type value = reader.ReadUInt16();
				if (slot < 0 || slot >= 15)
				{
					continue;
				}
				// TRTHOLD: any non-zero kind (incl. transform); not SlotOn (excludes nothing useful here).
				if (parts.GetWear(slot) != 0)
				{
					map[slot] = value;
				}
			}
		}

		/// <summary>
		/// KR KeepWorn path: keep item IDs the client sent. Thai suit apply sends
		/// chg=false, so official TH skips the SP — force persist when parts exist.
		/// Character-only preview clicks must not become worn/unlocked characters.
		/// </summary>
		private static void SanitizeSuitApply(Account user, AdvancedAvatarInfo incoming, ref bool bChangeRealAvatar, ref bool bChangeCostumeAvatar, ref bool bChangeAvatarMode)
		{
			if (user is null || user.advancedAvatarInfo is null || incoming is null)
			{
				return;
			}
			AdvancedAvatarInfo stored = user.advancedAvatarInfo;
			int wornChar = (int)stored.m_realAvatarInfo.m_character;
			if (wornChar <= 0)
			{
				wornChar = 1;
			}
			int rawChar = (int)incoming.m_realAvatarInfo.m_character;
			// Body keep/empty (65535/0) = character switch or preview. Pet/acc alone is not an outfit.
			bool rawBodyItems = HasVisibleBody(incoming.m_realAvatarInfo);
			bool rawClearReal = HasExplicitClearMarkers(incoming.m_realAvatarInfo);
			bool rawClearCos = HasExplicitClearMarkers(incoming.m_costumeAvatarInfo);
			if (rawChar > 0 && rawChar != wornChar && !rawBodyItems)
			{
				if (UserOwnsCharacter(user, rawChar) && myRoomGetCharacterSetting(user, rawChar, out AvatarInfo loaded)
					&& (int)loaded.m_character != 0)
				{
					AvatarInfo wireReal = incoming.m_realAvatarInfo;
					// Thai char-switch often sends all-65535 (= "I have no local outfit;
					// load DB"). That is NOT an intentional strip — wiping made suits vanish.
					// Only honor clear when wire also has chgReal and mixed body keep is absent
					// AND client sent worn accessories that prove an edit (rare). Default: load DB.
					bool intentionalClear = rawClearReal && bChangeRealAvatar
						&& HasWornParts(wireReal); // e.g. pet kept while body cleared
					if (intentionalClear)
					{
						for (int i = 1; i < 15; i++)
						{
							loaded.SetWear(i, 0);
						}
						ApplyClearMarkersToLoaded(wireReal, ref loaded);
					}
					else
					{
						// Keep accessories the client already applied (pet etc.).
						for (int i = 5; i < 15; i++)
						{
							ushort w = wireReal.GetWear(i);
							if (SlotOn(w))
							{
								loaded.SetWear(i, w);
							}
						}
					}
					incoming.m_realAvatarInfo = loaded;
					AvatarInfo cos = incoming.m_costumeAvatarInfo;
					cos.m_character = loaded.m_character;
					if (intentionalClear && rawClearCos)
					{
						for (int i = 1; i < 15; i++)
						{
							cos.SetWear(i, 0);
						}
					}
					else if (TryLoadCostumeSetting(user.UserNum, rawChar, out AvatarInfo loadedCos))
					{
						cos = loadedCos;
					}
					incoming.m_costumeAvatarInfo = cos;
					bChangeRealAvatar = true;
					bChangeCostumeAvatar = true;
					if (Conf.ProtocolDebug)
					{
						Log.Warning("SaveCharSetting char-switch {0}->{1} loaded top={2} clear={3} user={4}",
						wornChar, rawChar, (int)loaded.m_topBody, intentionalClear, user.UserID);
					}
					return;
				}
				incoming.m_realAvatarInfo = stored.m_realAvatarInfo;
				incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
				incoming.m_bIsUseCostume = stored.m_bIsUseCostume;
				bChangeRealAvatar = false;
				bChangeCostumeAvatar = false;
				bChangeAvatarMode = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preview-only char={0} kept worn={1} user={2}", rawChar, wornChar, user.UserID);
				}
				return;
			}
			// Capture wire mode / chg intent BEFORE KeepWorn fills zeros from stored outfit.
			bool wireMode = incoming.m_bIsUseCostume;
			bool wireChgMode = bChangeAvatarMode;
			bool wireChgReal = bChangeRealAvatar;
			bool wireChgCos = bChangeCostumeAvatar;
			bool skipCosKeepWorn = false;
			// HARD: Costume mode never edits Character wear from wire.
			// Thai may send Character all-65535, or put the costume outfit on the
			// Character half with chgReal=true — accepting that swapped layers.
			if (wireMode)
			{
				AvatarInfo wireReal = incoming.m_realAvatarInfo;
				bool wireRealBody = HasVisibleBody(wireReal);
				bool wireCosBody = HasVisibleBody(incoming.m_costumeAvatarInfo);
				AvatarInfo keepReal = stored.m_realAvatarInfo;
				ushort wireChar = wireReal.m_character;
				if (wireChar != 0 && wireChar != ushort.MaxValue)
				{
					keepReal.m_character = wireChar;
				}
				// REAL PROBLEM (Thai costume-ON):
				// Client often puts the *new suit* on the Character half and a thinner /
				// partial Cos half (or Cos with 65535 on body slots). Two cases:
				//   1) Suit apply: Character half has MORE worn parts than Cos → move
				//      Character wear → Costume (keep stored Character).
				//   2) Mode toggle: Cos is denser/equal but body slots are 65535 → those
				//      65535s are "unspecified", NOT unequip. Treating them as unequip
				//      wiped cosTop (23:04:59 3703→0) and every later toggle preserved 0.
				int realWorn = CountWornParts(wireReal);
				int cosWorn = CountWornParts(incoming.m_costumeAvatarInfo);
				bool cosKeepEmpty = !wireCosBody
					&& !HasWornParts(incoming.m_costumeAvatarInfo)
					&& !rawClearCos;
				bool moveRealToCos = wireRealBody && (cosKeepEmpty || realWorn > cosWorn);
				if (moveRealToCos)
				{
					// Character half is the new suit. Slots that are 0/65535 on that half are
					// "not in this suit" — merge stored Cos for those (do NOT blank them).
					// Prior skipCos path converted 65535→0 and skipped Cos KeepWorn → sparse
					// Cos (23:30:30 cosTop=3694 but down/acc wiped) = visible mismatch.
					AvatarInfo moved = wireReal;
					moved.m_character = keepReal.m_character;
					RewriteUnspecifiedCosMarkersToKeep(ref moved);
					incoming.m_costumeAvatarInfo = moved;
					rawClearCos = false;
					MergeKeepWorn(user, stored.m_costumeAvatarInfo, incoming, costume: true);
					skipCosKeepWorn = true;
					if (Conf.ProtocolDebug)
					{
						Log.Warning("SaveCharSetting costume-ON moved Character wear→Costume user={0} cosTop={1} realWorn={2} cosWorn={3}",
						user.UserID, (int)incoming.m_costumeAvatarInfo.m_topBody, realWorn, cosWorn);
					}
				}
				else if (rawClearCos && !wireCosBody && !HasWornParts(incoming.m_costumeAvatarInfo))
				{
					// Costume-ON + Cos all-65535 = keep-toggle / not editing Cos — preserve.
					incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
					rawClearCos = false;
					if (Conf.ProtocolDebug)
					{
						Log.Warning("SaveCharSetting preserve Costume under costume-ON clear user={0} cosTop={1}",
						user.UserID, (int)stored.m_costumeAvatarInfo.m_topBody);
					}
				}
				else if (wireRealBody)
				{
					// Mode toggle / wrong-half Character body: Cos 65535 = keep, not strip.
					RewriteUnspecifiedCosMarkersToKeep(ref incoming.m_costumeAvatarInfo);
					if (Conf.ProtocolDebug)
					{
						Log.Warning("SaveCharSetting costume-ON Cos 65535→keep (Character half has body) user={0} cosTopWire={1}",
						user.UserID, (int)incoming.m_costumeAvatarInfo.m_topBody);
					}
				}
				incoming.m_realAvatarInfo = keepReal;
				rawClearReal = false;
				bChangeRealAvatar = false;
				wireChgReal = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve Character under costume mode user={0} top={1} realBody={2} cosBody={3} move={4}",
					user.UserID, (int)keepReal.m_topBody, wireRealBody, wireCosBody, moveRealToCos);
				}
			}
			// Costume DEACTIVATE: Thai often sends mode=false with Character outfit on the
			// Character half and Cos keep/65535 only. Cos wire with no worn parts is NOT a
			// Cos strip — preserve stored Cos (23:30:30.948 left Cos half empty while
			// Character half had 3622…).
			else if (!wireMode && stored.m_bIsUseCostume
				&& !HasVisibleBody(incoming.m_costumeAvatarInfo)
				&& !HasWornParts(incoming.m_costumeAvatarInfo))
			{
				incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
				rawClearCos = false;
				if (!HasVisibleBody(incoming.m_realAvatarInfo)
					&& !HasWornParts(incoming.m_realAvatarInfo))
				{
					AvatarInfo keepReal = stored.m_realAvatarInfo;
					ushort wireChar = incoming.m_realAvatarInfo.m_character;
					if (wireChar != 0 && wireChar != ushort.MaxValue)
					{
						keepReal.m_character = wireChar;
					}
					incoming.m_realAvatarInfo = keepReal;
					rawClearReal = false;
				}
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve Cos on costume-OFF user={0} realTop={1} cosTop={2} realBody={3}",
					user.UserID,
					(int)incoming.m_realAvatarInfo.m_topBody,
					(int)stored.m_costumeAvatarInfo.m_topBody,
					HasVisibleBody(incoming.m_realAvatarInfo));
				}
			}
			// Mode OFF / toggle: Character clear with no body wear = not a strip
			// (02:34:07 wiped realTop while toggling costume off).
			else if (rawClearReal && !HasVisibleBody(incoming.m_realAvatarInfo)
				&& !HasWornParts(incoming.m_realAvatarInfo))
			{
				AvatarInfo keepReal = stored.m_realAvatarInfo;
				ushort wireChar = incoming.m_realAvatarInfo.m_character;
				if (wireChar != 0 && wireChar != ushort.MaxValue)
				{
					keepReal.m_character = wireChar;
				}
				incoming.m_realAvatarInfo = keepReal;
				rawClearReal = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve Character on mode-off clear user={0} top={1}",
					user.UserID, (int)keepReal.m_topBody);
				}
			}
			// Mode toggle: both halves all-65535 = not a strip.
			else if (rawClearReal && rawClearCos
				&& !HasWornParts(incoming.m_realAvatarInfo)
				&& !HasWornParts(incoming.m_costumeAvatarInfo))
			{
				incoming.m_realAvatarInfo = stored.m_realAvatarInfo;
				incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
				rawClearReal = false;
				rawClearCos = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve both on clear-toggle user={0} realTop={1} cosTop={2} mode={3}",
					user.UserID,
					(int)stored.m_realAvatarInfo.m_topBody,
					(int)stored.m_costumeAvatarInfo.m_topBody,
					wireMode);
				}
			}
			else if (!wireChgReal && rawClearReal)
			{
				AvatarInfo keepReal = stored.m_realAvatarInfo;
				ushort wireChar = incoming.m_realAvatarInfo.m_character;
				if (wireChar != 0 && wireChar != ushort.MaxValue)
				{
					keepReal.m_character = wireChar;
				}
				incoming.m_realAvatarInfo = keepReal;
				rawClearReal = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve Character (chgReal=false clear) user={0} top={1} mode={2}",
					user.UserID, (int)keepReal.m_topBody, wireMode);
				}
			}
			// Not editing Costume: all-65535 on cos with chgCos=false must not wipe stored costume.
			if (!wireChgCos && rawClearCos)
			{
				incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
				rawClearCos = false;
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting preserve Costume (chgCos=false clear) user={0} cosTop={1}",
					user.UserID, (int)stored.m_costumeAvatarInfo.m_topBody);
				}
			}
			// Character-mode (!mode, !chgCos): always ignore wire Costume half.
			// Thai often echoes Cos preview parts while only editing Character — applying
			// them swapped layers in memory (02:48:28 cosTop 3706→3700 with chgCos=false).
			// While Cos preview is on the wire, Character 65535 means "slot shown on Cos
			// preview", NOT unequip — honoring 65535 stripped Character head/down
			// (23:35:54 DB head=0 down=0 while toggling suits 3622↔3679).
			if (!wireMode && !wireChgCos)
			{
				if (HasWornParts(incoming.m_costumeAvatarInfo) || HasVisibleBody(incoming.m_costumeAvatarInfo))
				{
					AvatarInfo real = incoming.m_realAvatarInfo;
					RewriteUnspecifiedCosMarkersToKeep(ref real);
					incoming.m_realAvatarInfo = real;
					if (Conf.ProtocolDebug)
					{
						Log.Warning("SaveCharSetting Character 65535→keep (Cos preview on wire) user={0} realTop={1}",
						user.UserID, (int)incoming.m_realAvatarInfo.m_topBody);
					}
				}
				incoming.m_costumeAvatarInfo = stored.m_costumeAvatarInfo;
				rawClearCos = false;
				bChangeCostumeAvatar = false;
			}
			bool rawKeepBody = IsKeepOrEmptyBody(incoming.m_realAvatarInfo)
				&& IsKeepOrEmptyBody(incoming.m_costumeAvatarInfo)
				&& !rawClearReal && !rawClearCos;
			// Keep-only for *mode* = no layer chg flags either (post suit-ON keep echo).
			bool keepOnlyForMode = rawKeepBody && !wireChgReal && !wireChgCos;
			if (skipCosKeepWorn)
			{
				// Cos already MergeKeepWorn from stored in move path. Only fill Character.
				MergeKeepWorn(user, user.advancedAvatarInfo.m_realAvatarInfo, incoming, costume: false);
			}
			else
			{
				KeepWornPartsIfClientSentZero(user, incoming);
			}
			if (rawClearReal || rawClearCos)
			{
				if (rawClearReal)
				{
					bChangeRealAvatar = true;
				}
				if (rawClearCos)
				{
					bChangeCostumeAvatar = true;
				}
			}
			int nextChar = (int)incoming.m_realAvatarInfo.m_character;
			if (nextChar > 0 && nextChar != wornChar && !UserOwnsCharacter(user, nextChar))
			{
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting unowned char={0} remapped to {1} user={2}", nextChar, wornChar, user.UserID);
				}
				AvatarInfo realFix = incoming.m_realAvatarInfo;
				realFix.m_character = (ushort)wornChar;
				incoming.m_realAvatarInfo = realFix;
				AvatarInfo cosFix = incoming.m_costumeAvatarInfo;
				if ((int)cosFix.m_character == nextChar)
				{
					cosFix.m_character = (ushort)wornChar;
					incoming.m_costumeAvatarInfo = cosFix;
				}
			}
			if (HasWornParts(incoming.m_realAvatarInfo))
			{
				bChangeRealAvatar = true;
			}
			// Only force cos persist when client asked (chgCos) or costume-mode apply with parts.
			if (wireChgCos || (wireMode && HasWornParts(incoming.m_costumeAvatarInfo)))
			{
				bChangeCostumeAvatar = true;
			}
			else if (!wireMode && !wireChgCos)
			{
				bChangeCostumeAvatar = false;
			}
			// HARD RULE — costume mode:
			// Thai often sends mode=false + chgMode=false on keep/zero packets after suit-ON.
			// Never force mode off from that. Persist mode when:
			//   (1) client sets chgMode=true, or
			//   (2) mode=true (suit apply / activate; chgMode often false), or
			//   (3) mode=false + chgReal/chgCos and not a keep-only packet (suit OFF).
			// Also: Cos has worn body + Character keep/clear while mode=true already handled
			// above; Cos worn with wire mode=false after a wipe must not force OFF when
			// chg flags are keep-echo only (chgCos=false) — keepOnlyForMode covers that.
			if (wireChgMode)
			{
				incoming.m_bIsUseCostume = wireMode;
				bChangeAvatarMode = true;
			}
			else if (wireMode && wireMode != stored.m_bIsUseCostume && !keepOnlyForMode)
			{
				incoming.m_bIsUseCostume = true;
				bChangeAvatarMode = true;
			}
			else if (!wireMode && stored.m_bIsUseCostume && !keepOnlyForMode && (wireChgReal || wireChgCos))
			{
				incoming.m_bIsUseCostume = false;
				bChangeAvatarMode = true;
			}
			else
			{
				incoming.m_bIsUseCostume = stored.m_bIsUseCostume;
				// leave bChangeAvatarMode as wire (false) — do not rewrite costume mode SP
			}
			// Persist Character always after KeepWorn. Costume only when client asked or
			// costume-mode apply — do not force Cos write on Character-mode Cos-ignore.
			bChangeRealAvatar = true;
			if (!( !wireMode && !wireChgCos ))
			{
				bChangeCostumeAvatar = true;
			}
		}

		private static bool UserOwnsCharacter(Account user, int character)
		{
			if (user is null || character <= 0)
			{
				return true;
			}
			return ItemHandle.GetOwnedCharacterIds(user.UserNum).Contains(character);
		}

		private static bool HasWornParts(AvatarInfo info)
		{
			return CountWornParts(info) > 0;
		}

		private static int CountWornParts(AvatarInfo info)
		{
			int n = 0;
			for (int i = 1; i < 15; i++)
			{
				if (SlotOn(info.GetWear(i)))
				{
					n++;
				}
			}
			return n;
		}

		private static string DumpWear(AvatarInfo info)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 15; i++)
			{
				if (i > 0)
				{
					sb.Append(',');
				}
				sb.Append(info.GetWear(i));
			}
			return sb.ToString();
		}

		private static bool SlotOn(ushort v)
		{
			return v != 0 && v != ushort.MaxValue;
		}

		/// <summary>
		/// Drop body/acc kinds the account does not own (client often pushes local default suits).
		/// </summary>
		private static void StripUnownedWear(Account user, ref AvatarInfo info)
		{
			if (user is null)
			{
				return;
			}
			int ch = (int)info.m_character;
			if (ch <= 0 || ch == 65535)
			{
				return;
			}
			bool stripped = false;
			for (int i = 1; i < 15; i++)
			{
				ushort kind = info.GetWear(i);
				if (!SlotOn(kind))
				{
					continue;
				}
				int desc = ShopItemTable.getItemDescNumFromCPK((ushort)ch, (ushort)i, kind);
				if (desc <= 0)
				{
					desc = ShopItemTable.getItemDescNumFromCPK(0, (ushort)i, kind);
				}
				bool owned = false;
				if (desc > 0 && user.userItemAttr != null)
				{
					owned = user.userItemAttr.getItemAttr(desc, out CItemAttr _);
				}
				if (!owned && desc > 0)
				{
					owned = UserOwnsItemDesc(user.UserNum, desc);
				}
				if (!owned)
				{
					info.SetWear(i, 0);
					stripped = true;
				}
			}
			if (stripped)
			{
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveCharSetting stripped unowned wear user={0} char={1} top={2}",
					user.UserID, ch, (int)info.m_topBody);
				}
			}
		}

		private static bool UserOwnsItemDesc(int userNum, int itemDescNum)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT 1 FROM tblAvatarUser WHERE fdUserNum=@u AND fdItemDescNum=@i LIMIT 1", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				cmd.Parameters.AddWithValue("@i", itemDescNum);
				return cmd.ExecuteScalar() != null;
			}
			catch
			{
				return false;
			}
		}

		private static bool HasVisibleBody(AvatarInfo info)
		{
			return SlotOn(info.m_head) || SlotOn(info.m_topBody) || SlotOn(info.m_downBody) || SlotOn(info.m_foot);
		}

		private static bool IsKeepOrEmptyBody(AvatarInfo info)
		{
			return !SlotOn(info.m_head) && !SlotOn(info.m_topBody) && !SlotOn(info.m_downBody) && !SlotOn(info.m_foot);
		}

		/// <summary>
		/// Under costume-ON when Character half carries body, Thai Cos 65535 means
		/// "slot not authored" (keep previous), not unequip. Convert before KeepWorn.
		/// </summary>
		private static void RewriteUnspecifiedCosMarkersToKeep(ref AvatarInfo cos)
		{
			for (int i = 1; i < 15; i++)
			{
				if (cos.GetWear(i) == ushort.MaxValue)
				{
					cos.SetWear(i, 0);
				}
			}
			for (int i = 0; i < 7; i++)
			{
				if (cos.GetAcc(i) == ushort.MaxValue)
				{
					cos.SetAcc(i, 0);
				}
			}
			if (cos.GetEF() == ushort.MaxValue)
			{
				cos.SetEF(0);
			}
		}

		private static void KeepWornPartsIfClientSentZero(Account user, AdvancedAvatarInfo incoming)
		{
			if (user == null || user.advancedAvatarInfo is null || incoming is null)
			{
				return;
			}
			try
			{
				MergeKeepWorn(user, user.advancedAvatarInfo.m_realAvatarInfo, incoming, costume: false);
				MergeKeepWorn(user, user.advancedAvatarInfo.m_costumeAvatarInfo, incoming, costume: true);
			}
			catch (Exception ex)
			{
				Log.Warning("KeepWornPartsIfClientSentZero: {0}", ex.Message);
			}
		}

		private static void MergeKeepWorn(Account user, AvatarInfo current, AdvancedAvatarInfo incoming, bool costume)
		{
			_ = user;
			ref AvatarInfo next = ref costume ? ref incoming.m_costumeAvatarInfo : ref incoming.m_realAvatarInfo;
			if (next.m_character == 0)
			{
				next.m_character = current.m_character;
			}
			// Clear-all only when body is pure unequip markers (no 0=keep slots).
			// Mixed 65535+0 must KeepWorn the zeros — that is normal Character edit.
			if (HasExplicitClearMarkers(next))
			{
				// Belt: Character under costume mode = preserve, never strip.
				if (!costume && incoming.m_bIsUseCostume)
				{
					next = current;
					return;
				}
				for (int i = 1; i < 15; i++)
				{
					if (next.GetWear(i) == ushort.MaxValue)
					{
						next.SetWear(i, 0);
					}
				}
				for (int i = 0; i < 7; i++)
				{
					if (next.GetAcc(i) == ushort.MaxValue)
					{
						next.SetAcc(i, 0);
					}
				}
				if (next.GetEF() == ushort.MaxValue)
				{
					next.SetEF(0);
				}
				return;
			}
			// Different character without a loaded outfit: do not invent parts from another char.
			if (next.m_character != current.m_character)
			{
				for (int i = 1; i < 15; i++)
				{
					if (next.GetWear(i) == ushort.MaxValue)
					{
						next.SetWear(i, 0);
					}
				}
				return;
			}
			AvatarInfo fill = current;
			for (int i = 1; i < 15; i++)
			{
				int n = next.GetWear(i);
				int o = fill.GetWear(i);
				// Thai wire: 0 = keep previous, 65535 = explicit unequip.
				if (n == ushort.MaxValue)
				{
					next.SetWear(i, 0);
					continue;
				}
				if (n == 0 && o != 0 && o != ushort.MaxValue)
				{
					next.SetWear(i, (ushort)o);
				}
			}
			for (int i = 0; i < 7; i++)
			{
				int n = next.GetAcc(i);
				int o = fill.GetAcc(i);
				if (n == ushort.MaxValue)
				{
					next.SetAcc(i, 0);
					continue;
				}
				if (n == 0 && o != 0 && o != ushort.MaxValue)
				{
					next.SetAcc(i, (ushort)o);
				}
			}
			int nef = next.GetEF();
			int oef = fill.GetEF();
			if (nef == ushort.MaxValue)
			{
				next.SetEF(0);
			}
			else if (nef == 0 && oef != 0 && oef != ushort.MaxValue)
			{
				next.SetEF((ushort)oef);
			}
		}

		/// <summary>
		/// Clear-all on character body (head/top/down/foot): pure 65535 unequip markers.
		/// Mixed 65535+0 (keep) must still KeepWorn zeros — normal Character suit edit.
		/// </summary>
		private static bool HasExplicitClearMarkers(AvatarInfo info)
		{
			int bodyClear = 0;
			int bodyWorn = 0;
			int bodyKeep = 0;
			for (int i = 1; i <= 4; i++)
			{
				ushort v = info.GetWear(i);
				if (v == ushort.MaxValue)
				{
					bodyClear++;
				}
				else if (SlotOn(v))
				{
					bodyWorn++;
				}
				else if (v == 0)
				{
					bodyKeep++;
				}
			}
			if (bodyWorn > 0 || bodyKeep > 0)
			{
				return false;
			}
			// Pure unequip markers on the four body slots (clear-all / strip suit).
			return bodyClear >= 3;
		}

		private static void ApplyClearMarkersToLoaded(AvatarInfo wire, ref AvatarInfo loaded)
		{
			for (int i = 1; i < 15; i++)
			{
				ushort w = wire.GetWear(i);
				if (w == ushort.MaxValue)
				{
					loaded.SetWear(i, 0);
				}
				else if (SlotOn(w))
				{
					loaded.SetWear(i, w);
				}
			}
		}

		public static void Handle_SaveDefaultCharacter(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			ushort charid = reader.ReadLEUInt16();
			int curChar = currentAccount.advancedAvatarInfo != null
				? (int)currentAccount.advancedAvatarInfo.m_realAvatarInfo.m_character
				: 0;
			if (charid == 0)
			{
				Client.SendAsync(new Myroom_SaveDefaultChar(last));
				return;
			}
			if (!UserOwnsCharacter(currentAccount, charid))
			{
				Log.Warning("{0} - save default character {1} not owned, kept {2}", currentAccount.UserID, charid, curChar);
				Client.SendAsync(new Myroom_SaveDefaultChar(last));
				return;
			}
			// Always update UserInfoGame pointer — even when already wearing this char.
			// Char-switch via SaveCharSetting left fdAvatarCharacterSettingNum on the old
			// char (27); re-login then restored 27 instead of 43.
			bool sendAvatar = charid != curChar;
			if (myRoomSetDefaultCharacter(currentAccount, charid, sendAvatar))
			{
				if (Conf.ProtocolDebug)
				{
					Log.Warning("SaveDefaultCharacter OK user={0} char={1} was={2} sendAvatar={3}",
					currentAccount.UserID, charid, curChar, sendAvatar);
				}
				Client.SendAsync(new Myroom_SaveDefaultChar(last));
				if (sendAvatar)
				{
					currentAccount.getAttrs();
				}
			}
			else
			{
				Client.SendAsync(new Myroom_SaveDefaultCharFail(last));
			}
		}

		public static void Handle_ItemMsgPop(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (num != 0)
			{
				return;
			}
			if (HasPendingItemMsg(currentAccount.UserNum, num))
			{
				Client.SendAsync(new eServer_GET_ITEMMSG_ACK(currentAccount, num, last));
			}
		}

		private static bool HasPendingItemMsg(int userNum, int itemType)
		{
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					"SELECT 1 FROM tblUserItemMsg WHERE fdUserNum=@u AND fdType=@t LIMIT 1", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				cmd.Parameters.AddWithValue("@t", itemType);
				object o = cmd.ExecuteScalar();
				return o != null && o != DBNull.Value;
			}
			catch (Exception ex)
			{
				Log.Warning("HasPendingItemMsg: {0}", ex.Message);
				return false;
			}
		}

		public static void Handle_RepairItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemnum = reader.ReadLEInt32();
			int repairitemnum = reader.ReadLEInt32();
			if (RepairItem(currentAccount, itemnum, repairitemnum) == 0)
			{
				Client.SendAsync(new Myroom_RepairItemOK(itemnum, repairitemnum, last));
			}
			else
			{
				Client.SendAsync(new Myroom_RepairItemFail(itemnum, repairitemnum, last));
			}
		}

		public static void Handle_PetRebirth(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int petitemnum = reader.ReadLEInt32();
			int rebirthitemnum = reader.ReadLEInt32();
			myRoomPetRebirth(currentAccount.UserNum, petitemnum, rebirthitemnum, out var PetItemlist);
			Client.SendAsync(new Myroom_PetRebirth_New(PetItemlist, rebirthitemnum, last));
		}

		public static void Handle_FeedPet(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			int petitemnum = reader.ReadLEInt32();
			int num2 = reader.ReadLEInt32();
			if (num2 < 1)
			{
				return;
			}
			if (!ShopItemTable.getItemQueryFromItemDescNum(num, out var query) || query.Item1 != 0 || query.Item2 != 107)
			{
				Log.Error("Not pet feed item");
				return;
			}
			int num3 = 0;
			int num4 = 0;
			num3 = (int)ItemAttrTable.getItemAttrFromItemDescNum(num, eItemAttr.eItemAttr_usePetExp);
			num4 = (int)ItemAttrTable.getItemAttrFromItemDescNum(num, eItemAttr.eItemAttr_usePetDays);
			if ((int)ItemAttrTable.getItemAttrFromItemDescNum(num, eItemAttr.eItemAttr_PetFeedType) == 1)
			{
				if (FeedPet(currentAccount, petitemnum, num, num4, num3, num2) == 0)
				{
					Client.SendAsync(new Myroom_FeedPetOK(petitemnum, num, last));
				}
			}
			else if (FeedAllPet(currentAccount, num, num4, num3, num2) == 0)
			{
				Client.SendAsync(new Myroom_FeedPetOK(petitemnum, num, last));
			}
		}

		public static void Handle_PetUpgrade(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (!ShopItemTable.getItemQueryFromItemDescNum(num, out var query))
			{
				Log.Error("Requested bad Pet upgrade. cannot find item. itemdescnum : {0}", num);
				return;
			}
			if (query.Item2 != 10 && query.Item2 != 117)
			{
				Log.Error("Requested bad Pet upgrade. position is not 10. itemdescnum : {0}", num);
				return;
			}
			int num2 = query.Item3 % 10;
			if (num2 <= 0 || num2 >= 3)
			{
				return;
			}
			if (!ItemHolder.PetMaxEXP.TryGetValue(query.Item3, out var value))
			{
				switch (num2)
				{
				case 1:
					value = 50000;
					break;
				case 2:
					value = 100000;
					break;
				case 3:
					value = 1000000;
					break;
				}
			}
			if (PetUpgrade(currentAccount, num, value) == 0)
			{
				Client.SendAsync(new Myroom_PetUpgradeOK(currentAccount, last));
			}
		}

		public static void Handle_FFCF0100(ClientConnection Client, PacketReader reader, byte last)
		{
			bool flag = false;
			flag = reader.ReadBoolean();
			Client.SendAsync(new Myroom_FFCF0100(flag, last));
		}

		public static void Handle_GetGiftList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short startindex = reader.ReadLEInt16();
			short lastindex = reader.ReadLEInt16();
			if (startindex < 1)
			{
				startindex = 1;
			}
			if (lastindex < startindex)
			{
				lastindex = startindex;
			}
			ShopHandle.shopGetGiftAcceptWaitList(currentAccount.UserNum, startindex, lastindex, out var GiftList, out var Itemcount);
			Client.SendAsync(new Myroom_GetGiftList_New(startindex, lastindex, GiftList, Itemcount, last));
		}

		public static void Handle_AcceptGift(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			// Connection doubles the REQ last flag (0x40 → 0x80) after sequence
			// check. Packed 879 lock matches the original flag on the ACK.
			byte reqLast = reader.Buffer.LastOrDefault();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = $"{reader.ReadBig5StringSafe(fixedLength)},";
			if (!new Regex("^[\\d,]+$").IsMatch(empty))
			{
				return;
			}
			int level = currentAccount.Level;
			AcceptGiftEx(currentAccount, empty, out var itemList);
			if (itemList.Count > 0)
			{
				// Packed 762 treats ACK ids as itemDescNums (client item-table lookup).
				// Echoing gift uniNums crashed accept-all: 241 has no tblavataritemdesc
				// row (240 then 242). Early GM accept used desc 127159, not uniNum 133.
				List<int> ackIds = new List<int>(itemList.Count);
				foreach (int giftItem in itemList)
				{
					ackIds.Add(ItemHandle.ResolveGiftItemDescNum(giftItem));
				}
				Client.SendAsync(new Myroom_AcceptGiftOK(ackIds, reqLast));
				foreach (int giftItem in itemList)
				{
					ItemHandle.SendAvatarItemOne(currentAccount, giftItem, last);
				}
				if (LobbyHandle.LevelUPCheck(currentAccount, level))
				{
					Client.SendAsync(new UserLevelUPEXPInfo(1, currentAccount.Level, currentAccount.Exp, last));
				}
				ItemHandle.getActiveFuncItem(currentAccount, -1);
				ShopHandle.shopGetGiftAcceptWaitList(currentAccount.UserNum, 1, 8, out var GiftList, out var Itemcount);
				Client.SendAsync(new Myroom_GetGiftList_New(1, 8, GiftList, Itemcount, reqLast));
			}
			else
			{
				Client.SendAsync(new Myroom_AcceptGiftFail(reqLast));
			}
		}

		public static void Handle_MyroomGetUserItemAttr(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			bool bStrengthenGetMyItemLoad = reader.ReadBoolean();
			LoginTrafficManager.getUserItemAttr(currentAccount, bStrengthenGetMyItemLoad);
		}

		public static void Handle_GetStorageItemList(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			switch (reader.ReadLEInt32())
			{
			case 0:
			{
				ShopHandle.storage_getKeepingItemList(currentAccount.UserNum, out var KeepList);
				Client.SendAsync(new Myroom_GetStorageKeepingList(KeepList, last));
				break;
			}
			case 1:
			{
				ShopHandle.storage_getGiftList(currentAccount.UserNum, out var GiftList);
				Client.SendAsync(new Myroom_GetStorageGiftList(GiftList, last));
				break;
			}
			}
		}

		public static void Handle_StorageItemGift(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int fixedLength = reader.ReadLEInt16();
			string text = reader.ReadBig5StringSafe(fixedLength);
			int num = reader.ReadLEInt16();
			string msg = string.Empty;
			if (num > 0)
			{
				msg = reader.ReadBig5StringSafe(num);
			}
			long uniqueNum = reader.ReadLEInt64();
			if (currentAccount.NickName == text)
			{
				Client.SendAsync(new Myroom_StorageGiftACK(currentAccount, 4, uniqueNum, text, last));
				return;
			}
			byte error = StorageGift(currentAccount, text, msg, uniqueNum);
			Client.SendAsync(new Myroom_StorageGiftACK(currentAccount, error, uniqueNum, text, last));
		}

		public static void Handle_StorageItemReceive(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int type = reader.ReadLEInt32();
			int fixedLength = reader.ReadLEInt16();
			string empty = string.Empty;
			empty = $"{reader.ReadBig5StringSafe(fixedLength)},";
			if (new Regex("^[\\d,]+$").IsMatch(empty))
			{
				StorageReceiveEx(currentAccount, type, empty, out var itemList);
				if (itemList.Count > 0)
				{
					Client.SendAsync(new Myroom_StorageReceiveOK(type, itemList, last));
				}
			}
		}

		public static void Handle_ItemOnOff(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			bool flag = reader.ReadBoolean();
			if (flag && num == 58018 && !currentAccount.ChangedTalesBook)
			{
				currentAccount.ChangedTalesBook = true;
			}
			else
			{
				ItemOnOff(currentAccount, num, flag, last);
			}
		}

		public static void Handle_UseLuckyBag(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemnum = reader.ReadLEInt32();
			byte b = reader.ReadByte();
			int level = currentAccount.Level;
			if (b > 10)
			{
				b = 10;
			}
			Dictionary<int, int> itemlist;
			if (!ServerStatus.enableUseLuckyBag)
			{
				Client.SendAsync(new Myroom_UseLuckyBagFail(itemnum, last));
			}
			else if (UseLuckyBag(currentAccount, itemnum, b, out itemlist) && itemlist.Count > 0)
			{
				Client.SendAsync(new Myroom_UseLuckyBagOK(itemnum, b, itemlist, last));
				ItemHolder.ShuRewardCheck(currentAccount, itemlist.Keys.ToList());
				if (LobbyHandle.LevelUPCheck(currentAccount, level))
				{
					Client.SendAsync(new UserLevelUPEXPInfo(1, currentAccount.Level, currentAccount.Exp, last));
				}
			}
			else
			{
				Client.SendAsync(new Myroom_UseLuckyBagFail(itemnum, last));
			}
		}

		public static void Handle_SetSlotItemSetting(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int bodyLen = reader.Remaining;
			// Thai wire: Character half + Costume half (+ optional mode) + slot + name.
			// TRTHOLD KR was single half — reading one half made slotNum=character (43) → fail 3.
			int halfSize = 15 * 2 + 7 * 2 + 2 + Conf.AvatarDyePadBytes;
			bool dualHalf = bodyLen >= halfSize * 2 + 6;
			AvatarInfo wireReal = ReadAvatarHalf(reader);
			AvatarInfo wireCostume = default(AvatarInfo);
			bool useCostume = currentAccount.advancedAvatarInfo != null
				&& currentAccount.advancedAvatarInfo.m_bIsUseCostume;
			if (dualHalf)
			{
				wireCostume = ReadAvatarHalf(reader);
			}
			if (reader.Remaining > 5)
			{
				byte mode = reader.ReadByte();
				if (mode <= 1)
				{
					useCostume = mode != 0;
				}
				else
				{
					reader.Offset--;
				}
			}
			int slotNum = reader.ReadLEInt32();
			string text = ReadSlotName(reader);
			if (slotNum < 0 || slotNum > 20)
			{
				Log.Warning("SetSlotItemSetting bad slot user={0} slot={1} bodyLen={2} dual={3} remain={4}",
					currentAccount.UserID, slotNum, bodyLen, dualHalf, reader.Remaining);
				Client.SendAsync(new Myroom_SetSlotItemSettingFail(3, last));
				return;
			}
			AvatarInfo storedReal = currentAccount.advancedAvatarInfo != null
				? currentAccount.advancedAvatarInfo.m_realAvatarInfo
				: default(AvatarInfo);
			AvatarInfo storedCos = currentAccount.advancedAvatarInfo != null
				? currentAccount.advancedAvatarInfo.m_costumeAvatarInfo
				: default(AvatarInfo);
			bool accountCostume = currentAccount.advancedAvatarInfo != null
				&& currentAccount.advancedAvatarInfo.m_bIsUseCostume;
			bool costumeSignal = accountCostume || useCostume;
			// Persist both wire halves + mode. Flattening to a visible composite (prior fix)
			// made apply restore only cos parts onto real (head=0, top=3700) while mode stayed
			// true — dbg 00:27:31. Echo the same dual row on ACK / GetSlotInfo.
			AvatarInfo slotReal = wireReal;
			AvatarInfo slotCos = dualHalf ? wireCostume : default(AvatarInfo);
			if ((int)slotReal.m_character == 0 || slotReal.m_character == ushort.MaxValue)
			{
				slotReal.m_character = storedReal.m_character;
			}
			if ((int)slotCos.m_character == 0 || slotCos.m_character == ushort.MaxValue)
			{
				if ((int)storedCos.m_character != 0)
				{
					slotCos.m_character = storedCos.m_character;
				}
				else if (costumeSignal)
				{
					slotCos.m_character = slotReal.m_character;
				}
			}
			if (TryGetSlotFull(currentAccount.UserNum, slotNum, out AvatarInfo prevReal, out AvatarInfo prevCos, out _)
				&& (int)prevReal.m_character != 0)
			{
				MergeKeepMaxValue(ref slotReal, prevReal);
				MergeKeepMaxValue(ref slotCos, prevCos);
			}
			else
			{
				if ((int)storedReal.m_character != 0)
				{
					MergeKeepMaxValue(ref slotReal, storedReal);
				}
				if ((int)storedCos.m_character != 0)
				{
					MergeKeepMaxValue(ref slotCos, storedCos);
				}
			}
			if (Conf.ProtocolDebug)
			{
				Log.Warning("SetSlotItemSetting user={0} slot={1} char={2} top={3}/{4} wireTop={5}/{6} nameLen={7} dual={8} cosMode={9} remain={10}",
				currentAccount.UserID, slotNum, (int)slotReal.m_character,
				(int)slotReal.m_topBody, (int)slotCos.m_topBody,
				(int)wireReal.m_topBody, (int)wireCostume.m_topBody,
				text?.Length ?? 0, dualHalf, costumeSignal, reader.Remaining);
			}
			Regex regex = new Regex("[~`!@#$%^&*()+=|\\\\{}':;.,<>/?[\\]\"]");
			int byteCount = Encoding.Default.GetByteCount(text ?? string.Empty);
			bool flag = false;
			int err = 0;
			if (byteCount > 48 || (!string.IsNullOrEmpty(text) && regex.IsMatch(text)))
			{
				err = 3;
			}
			else
			{
				flag = MyRoomSetSlotItemSetting(currentAccount.UserNum, slotNum, text, slotReal, slotCos, costumeSignal, out err);
			}
			if (flag)
			{
				Client.SendAsync(new Myroom_SetSlotItemSettingOK(slotNum, text, slotReal, slotCos, costumeSignal, last));
			}
			else
			{
				Log.Warning("SetSlotItemSetting fail user={0} slot={1} err={2}", currentAccount.UserID, slotNum, err);
				Client.SendAsync(new Myroom_SetSlotItemSettingFail(err, last));
			}
		}

		private static AvatarInfo ReadAvatarHalf(PacketReader reader)
		{
			AvatarInfo avatarInfo = default(AvatarInfo);
			for (int i = 0; i < 15; i++)
			{
				avatarInfo.SetWear(i, reader.ReadLEUInt16());
			}
			for (int i = 0; i < 7; i++)
			{
				avatarInfo.SetAcc(i, reader.ReadLEUInt16());
			}
			avatarInfo.SetEF(reader.ReadLEUInt16());
			int dyeSkip = Conf.AvatarDyePadBytes;
			if (dyeSkip > reader.Remaining)
			{
				dyeSkip = reader.Remaining;
			}
			reader.Offset += dyeSkip;
			return avatarInfo;
		}

		private static string ReadSlotName(PacketReader reader)
		{
			if (reader.Remaining < 2)
			{
				return string.Empty;
			}
			int nameLen;
			if (reader.Remaining >= 4)
			{
				int asInt = BitConverter.ToInt32(reader.Buffer, reader.Offset);
				if (asInt >= 0 && asInt <= 64 && reader.Remaining >= 4 + asInt)
				{
					reader.Offset += 4;
					nameLen = asInt;
				}
				else
				{
					nameLen = reader.ReadLEInt16();
				}
			}
			else
			{
				nameLen = reader.ReadLEInt16();
			}
			if (nameLen <= 0)
			{
				return string.Empty;
			}
			return reader.ReadBig5StringSafe(nameLen);
		}

		private static bool TryGetSlotAvatar(int userNum, int slotNum, out AvatarInfo info)
		{
			return TryGetSlotFull(userNum, slotNum, out info, out _, out _);
		}

		private static bool TryGetSlotFull(int userNum, int slotNum, out AvatarInfo real, out AvatarInfo cos, out bool useCostume)
		{
			real = default(AvatarInfo);
			cos = default(AvatarInfo);
			useCostume = false;
			MyRoomGetUserSlotInfo(userNum, out List<MyRoomSlotInfo> slots);
			if (slots == null)
			{
				return false;
			}
			foreach (MyRoomSlotInfo slot in slots)
			{
				if (slot.m_iSlotNum == slotNum)
				{
					real = slot.m_AvatarInfo;
					cos = slot.m_CostumeAvatarInfo;
					useCostume = slot.m_bIsUseCostume;
					return (int)real.m_character != 0;
				}
			}
			return false;
		}

		public static void Handle_GetUserSlotInfo(ClientConnection Client, byte last)
		{
			MyRoomGetUserSlotInfo(Client.CurrentAccount.UserNum, out var m_vSlotInfo);
			Client.SendAsync(new Myroom_GetSlotInfoOK(m_vSlotInfo, last));
		}

		public static void Handle_GetFavoriteList(ClientConnection Client, byte last)
		{
			if (GetFavorite(Client.CurrentAccount.UserNum, out var itemlist))
			{
				Client.SendAsync(new Myroom_FavoriteList(itemlist, last));
			}
		}

		public static void Handle_AddFavorite(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (currentAccount.activeItem.HasItem(num) && ModifyFavorite(currentAccount.UserNum, num, 1))
			{
				Client.SendAsync(new Myroom_AddFavorite(num, last));
			}
		}

		public static void Handle_RemoveFavorite(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int num = reader.ReadLEInt32();
			if (currentAccount.activeItem.HasItem(num) && ModifyFavorite(currentAccount.UserNum, num, 2))
			{
				Client.SendAsync(new Myroom_RemoveFavorite(num, last));
			}
		}

		public static void Handle_CharacterStatReset(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short character = reader.ReadLEInt16();
			int num = reader.ReadLEInt32();
			bool flag = true;
			if (num == 1)
			{
				List<ShopBuyItemInfo> list = new List<ShopBuyItemInfo>();
				for (int i = 0; i < 1; i++)
				{
					// charStatReset must be an itemdesc with fdType in (1,3,4).
					// Live DB had 100000 (type 2 / looked like a TR price) → SP
					// "incorrect item type!" → shop fail reason 3.
					int charStatReset = ServerSettingHolder.ServerSettings.charStatReset;
					if (charStatReset <= 0 || charStatReset == 100000)
					{
						charStatReset = 29205;
					}
					int unk = 0;
					int unk2 = 133234944;
					int sellItemNum = 0;
					ShopBuyItemInfo item = new ShopBuyItemInfo
					{
						ItemNum = charStatReset,
						unk3 = unk,
						unk4 = unk2,
						SellItemNum = sellItemNum
					};
					list.Add(item);
				}
				eShopFailed_REASON m_failReason = eShopFailed_REASON.eShopFailed_REASON_UNKNOWN;
				foreach (ShopBuyItemInfo item2 in list.Where((ShopBuyItemInfo w) => !w.NotForSale))
				{
					if (ShopHandle.BuyItemCheck(currentAccount, item2.ItemNum, item2.SellItemNum, 4000, out var _, out var _, out m_failReason))
					{
						item2.BuySuccess = true;
					}
				}
				flag = list.All((ShopBuyItemInfo a) => a.BuySuccess);
				if (flag)
				{
					Client.SendAsync(new ShopBuyItem_ACK(list, isFarmOrShuItem: false, last));
				}
				else
				{
					Client.SendAsync(new ShopBuyItemFail_New(m_failReason, list, last));
				}
				ItemHandle.getActiveFuncItem(currentAccount, -1);
			}
			if (!flag || !UserCharacterStatReset(currentAccount, character, num, last))
			{
				Client.SendAsync(new Myroom_CharacterStatResetFail(last));
			}
		}

		public static void Handle_CharacterStatConfirm(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			short character = reader.ReadLEInt16();
			// Thai client: character(short) + flag/index(int) + selectedStatNum(int).
			// Older layout was character + statNum only — keep compat when no 2nd int.
			int first = reader.ReadLEInt32();
			int statNum = first;
			if (reader.Remaining >= 4)
			{
				statNum = reader.ReadLEInt32();
			}
			UserCharacterStatConfirm(currentAccount, character, statNum, last);
		}

		public static void Handle_UseExtraAbilityItem(ClientConnection Client, PacketReader reader, byte last)
		{
			Account currentAccount = Client.CurrentAccount;
			int itemNum = reader.ReadLEInt32();
			UseExtraAbilityItem(currentAccount, itemNum, last);
		}

		public static bool MyRoomSetCharacterSetting(int UserNum, AdvancedAvatarInfo m_originAdvancedAvatarInfo, AdvancedAvatarInfo m_transAdvancedAvatarInfo, bool bChangeRealAvatar, bool bChangeCostumeAvatar, bool bChangeAvatarMode)
		{
			bool flag = true;
			if (bChangeRealAvatar && flag)
			{
				try
				{
					using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_myRoomSetCharacterSetting");
					mySqlCommandHelper.AddParamInt("usernum", UserNum);
					mySqlCommandHelper.AddParamInt("pcharacter", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_character);
					mySqlCommandHelper.AddParamInt("head", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_head);
					mySqlCommandHelper.AddParamInt("topbody", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_topBody);
					mySqlCommandHelper.AddParamInt("downbody", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_downBody);
					mySqlCommandHelper.AddParamInt("foot", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_foot);
					mySqlCommandHelper.AddParamInt("acHead", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acHead);
					mySqlCommandHelper.AddParamInt("acFace", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acFace);
					mySqlCommandHelper.AddParamInt("acHand", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acHand);
					mySqlCommandHelper.AddParamInt("acBack", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acBack);
					mySqlCommandHelper.AddParamInt("acNeck", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acNeck);
					mySqlCommandHelper.AddParamInt("pet", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_pet);
					mySqlCommandHelper.AddParamInt("expansion", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_expansion);
					mySqlCommandHelper.AddParamInt("acWrist", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acWrist);
					mySqlCommandHelper.AddParamInt("acBooster", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_acBooster);
					mySqlCommandHelper.AddParamInt("acTail", m_originAdvancedAvatarInfo.m_realAvatarInfo.m_accTail);
					mySqlCommandHelper.AddParamInt("transCharacter", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_character);
					mySqlCommandHelper.AddParamInt("transHead", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_head);
					mySqlCommandHelper.AddParamInt("transTopBody", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_topBody);
					mySqlCommandHelper.AddParamInt("transDownBody", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_downBody);
					mySqlCommandHelper.AddParamInt("transFoot", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_foot);
					mySqlCommandHelper.AddParamInt("transAcHead", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acHead);
					mySqlCommandHelper.AddParamInt("transAcFace", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acFace);
					mySqlCommandHelper.AddParamInt("transAcHand", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acHand);
					mySqlCommandHelper.AddParamInt("transAcBack", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acBack);
					mySqlCommandHelper.AddParamInt("transAcNeck", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acNeck);
					mySqlCommandHelper.AddParamInt("transPet", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_pet);
					mySqlCommandHelper.AddParamInt("transExpansion", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_expansion);
					mySqlCommandHelper.AddParamInt("transAcWrist", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acWrist);
					mySqlCommandHelper.AddParamInt("transAcBooster", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_acBooster);
					mySqlCommandHelper.AddParamInt("transAcTail", m_transAdvancedAvatarInfo.m_realAvatarInfo.m_accTail);
					mySqlCommandHelper.ExecuteNonQuery();
					flag = true;
				}
				catch (Exception ex)
				{
					// MySQL ROW_COUNT()=0 when UPDATE changes no columns (all 65535 keep).
					// That is success if the character setting row exists.
					if (ex.Message != null && ex.Message.IndexOf("no avatarCharacterSetting", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						flag = CharacterSettingExists(UserNum, m_originAdvancedAvatarInfo.m_realAvatarInfo.m_character);
						if (!flag)
						{
							Log.Error("usp_myRoomSetCharacterSetting Error: {0}", ex.Message);
						}
					}
					else
					{
						flag = false;
						Log.Error("usp_myRoomSetCharacterSetting Error: {0}", ex.Message);
					}
				}
			}
			if (bChangeCostumeAvatar && flag)
			{
				try
				{
					using MySqlCommandHelper mySqlCommandHelper2 = new MySqlCommandHelper("usp_myRoomSetCostumeCharacterSetting");
					mySqlCommandHelper2.AddParamInt("usernum", UserNum);
					mySqlCommandHelper2.AddParamInt("pcharacter", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_character);
					mySqlCommandHelper2.AddParamInt("head", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_head);
					mySqlCommandHelper2.AddParamInt("topbody", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_topBody);
					mySqlCommandHelper2.AddParamInt("downbody", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_downBody);
					mySqlCommandHelper2.AddParamInt("foot", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_foot);
					mySqlCommandHelper2.AddParamInt("acHead", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acHead);
					mySqlCommandHelper2.AddParamInt("acFace", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acFace);
					mySqlCommandHelper2.AddParamInt("acHand", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acHand);
					mySqlCommandHelper2.AddParamInt("acBack", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acBack);
					mySqlCommandHelper2.AddParamInt("acNeck", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acNeck);
					mySqlCommandHelper2.AddParamInt("pet", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_pet);
					mySqlCommandHelper2.AddParamInt("expansion", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_expansion);
					mySqlCommandHelper2.AddParamInt("acWrist", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acWrist);
					mySqlCommandHelper2.AddParamInt("acBooster", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_acBooster);
					mySqlCommandHelper2.AddParamInt("acTail", m_originAdvancedAvatarInfo.m_costumeAvatarInfo.m_accTail);
					mySqlCommandHelper2.AddParamInt("transCharacter", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_character);
					mySqlCommandHelper2.AddParamInt("transHead", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_head);
					mySqlCommandHelper2.AddParamInt("transTopBody", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_topBody);
					mySqlCommandHelper2.AddParamInt("transDownBody", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_downBody);
					mySqlCommandHelper2.AddParamInt("transFoot", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_foot);
					mySqlCommandHelper2.AddParamInt("transAcHead", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acHead);
					mySqlCommandHelper2.AddParamInt("transAcFace", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acFace);
					mySqlCommandHelper2.AddParamInt("transAcHand", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acHand);
					mySqlCommandHelper2.AddParamInt("transAcBack", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acBack);
					mySqlCommandHelper2.AddParamInt("transAcNeck", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acNeck);
					mySqlCommandHelper2.AddParamInt("transPet", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_pet);
					mySqlCommandHelper2.AddParamInt("transExpansion", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_expansion);
					mySqlCommandHelper2.AddParamInt("transAcWrist", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acWrist);
					mySqlCommandHelper2.AddParamInt("transAcBooster", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_acBooster);
					mySqlCommandHelper2.AddParamInt("transAcTail", m_transAdvancedAvatarInfo.m_costumeAvatarInfo.m_accTail);
					mySqlCommandHelper2.ExecuteNonQuery();
					flag = true;
				}
				catch (Exception ex2)
				{
					// PK is fdUserNum only; no-op UPDATE then INSERT hits Duplicate entry.
					if (ex2.Message != null && ex2.Message.IndexOf("Duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						flag = true;
					}
					else
					{
						flag = false;
						Log.Error("usp_myRoomSetCostumeCharacterSetting Error: {0}", ex2.Message);
					}
				}
			}
			if (bChangeAvatarMode && flag)
			{
				try
				{
					using MySqlCommandHelper mySqlCommandHelper3 = new MySqlCommandHelper("usp_myRoomSetCostumeModeSetting");
					mySqlCommandHelper3.AddParamInt("usernum", UserNum);
					mySqlCommandHelper3.AddParamBoolean("costumeMode", m_transAdvancedAvatarInfo.m_bIsUseCostume);
					mySqlCommandHelper3.ExecuteNonQuery();
					return true;
				}
				catch (Exception ex3)
				{
					flag = false;
					Log.Error("usp_myRoomSetCostumeModeSetting Error: {0}", ex3.Message);
					return flag;
				}
			}
			return flag;
		}

		private static bool CharacterSettingExists(int userNum, int character)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(
					"SELECT 1 FROM tblavatarcharactersetting WHERE fdUserNum = @u AND fdCharacter = @c LIMIT 1",
					mySqlConnection);
				mySqlCommand.Parameters.AddWithValue("@u", userNum);
				mySqlCommand.Parameters.AddWithValue("@c", character);
				object obj = mySqlCommand.ExecuteScalar();
				return obj != null && obj != DBNull.Value;
			}
			catch (Exception ex)
			{
				Log.Warning("CharacterSettingExists u={0} c={1}: {2}", userNum, character, ex.Message);
				return false;
			}
		}

		private static bool MyRoomSetSlotItemSetting(int UserNum, int slotNum, string SlotName, AvatarInfo m_AvatarInfo, AvatarInfo cosAvatar, bool useCostume, out int err)
		{
			err = 0;
			try
			{
				// Empty incoming name would wipe a previously saved title.
				if (string.IsNullOrEmpty(SlotName))
				{
					using (MySqlConnection peek = new MySqlConnection(Conf.Connstr))
					{
						peek.Open();
						using MySqlCommand peekCmd = new MySqlCommand(
							"SELECT fdSlotName FROM UserMyRoomSlotSettingInfo WHERE fdUserNum=@u AND fdSlotNum=@s LIMIT 1",
							peek);
						peekCmd.Parameters.AddWithValue("@u", UserNum);
						peekCmd.Parameters.AddWithValue("@s", slotNum);
						object prev = peekCmd.ExecuteScalar();
						if (prev != null && prev != DBNull.Value)
						{
							string keep = Convert.ToString(prev);
							if (!string.IsNullOrEmpty(keep))
							{
								SlotName = keep;
							}
						}
					}
				}
				// Ensure slot row exists — SP only auto-inserts slot 0 (err=1 otherwise).
				using (MySqlConnection ensure = new MySqlConnection(Conf.Connstr))
				{
					ensure.Open();
					using MySqlCommand ins = new MySqlCommand(
						"INSERT IGNORE INTO UserMyRoomSlotSettingInfo (fdUserNum, fdSlotNum, fdSlotName, fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot, fdACHead, fdACFace, fdACHand, fdACBack, fdACNeck, fdPet, fdExpansion, fdACWrist, fdACBooster, fdTail) VALUES (@u,@s,@n,@c,0,0,0,0,0,0,0,0,0,0,0,0,0,0)",
						ensure);
					ins.Parameters.AddWithValue("@u", UserNum);
					ins.Parameters.AddWithValue("@s", slotNum);
					ins.Parameters.AddWithValue("@n", SlotName ?? string.Empty);
					ins.Parameters.AddWithValue("@c", (int)m_AvatarInfo.m_character);
					try
					{
						ins.ExecuteNonQuery();
					}
					catch
					{
						// Column set may differ; SP update still attempted below.
					}
				}
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_myRoomSetSlotItemSetting");
				mySqlCommandHelper.AddParamInt("usernum", UserNum);
				mySqlCommandHelper.AddParamInt("slotNum", slotNum);
				mySqlCommandHelper.AddParamVarString("pSlotName", SlotName ?? string.Empty);
				mySqlCommandHelper.AddParamInt("charact", m_AvatarInfo.m_character);
				mySqlCommandHelper.AddParamInt("head", m_AvatarInfo.m_head);
				mySqlCommandHelper.AddParamInt("topbody", m_AvatarInfo.m_topBody);
				mySqlCommandHelper.AddParamInt("downbody", m_AvatarInfo.m_downBody);
				mySqlCommandHelper.AddParamInt("foot", m_AvatarInfo.m_foot);
				mySqlCommandHelper.AddParamInt("acHead", m_AvatarInfo.m_acHead);
				mySqlCommandHelper.AddParamInt("acFace", m_AvatarInfo.m_acFace);
				mySqlCommandHelper.AddParamInt("acHand", m_AvatarInfo.m_acHand);
				mySqlCommandHelper.AddParamInt("acBack", m_AvatarInfo.m_acBack);
				mySqlCommandHelper.AddParamInt("acNeck", m_AvatarInfo.m_acNeck);
				mySqlCommandHelper.AddParamInt("pet", m_AvatarInfo.m_pet);
				mySqlCommandHelper.AddParamInt("expansion", m_AvatarInfo.m_expansion);
				mySqlCommandHelper.AddParamInt("acWrist", m_AvatarInfo.m_acWrist);
				mySqlCommandHelper.AddParamInt("acBooster", m_AvatarInfo.m_acBooster);
				mySqlCommandHelper.AddParamInt("acTail", m_AvatarInfo.m_accTail);
				mySqlCommandHelper.ExecuteSingle();
				if (mySqlCommandHelper.HasResult())
				{
					err = mySqlCommandHelper.GetInt("ret");
					if (err != 0)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
				// Costume half + mode (columns added for Thai dual-half slots).
				using (MySqlConnection cosConn = new MySqlConnection(Conf.Connstr))
				{
					cosConn.Open();
					using MySqlCommand cosCmd = new MySqlCommand(
						@"UPDATE UserMyRoomSlotSettingInfo SET
							fdCosCharacter=@cc, fdCosHead=@ch, fdCosTopBody=@ct, fdCosDownBody=@cd, fdCosFoot=@cf,
							fdCosACHead=@cah, fdCosACFace=@caf, fdCosACHand=@cak, fdCosACBack=@cab, fdCosACNeck=@can,
							fdCosPet=@cp, fdCosExpansion=@ce, fdCosACWrist=@cw, fdCosACBooster=@cb, fdCosTail=@ctl,
							fdCostumeMode=@cm
						WHERE fdUserNum=@u AND fdSlotNum=@s",
						cosConn);
					cosCmd.Parameters.AddWithValue("@u", UserNum);
					cosCmd.Parameters.AddWithValue("@s", slotNum);
					cosCmd.Parameters.AddWithValue("@cc", (int)cosAvatar.m_character);
					cosCmd.Parameters.AddWithValue("@ch", (int)cosAvatar.m_head);
					cosCmd.Parameters.AddWithValue("@ct", (int)cosAvatar.m_topBody);
					cosCmd.Parameters.AddWithValue("@cd", (int)cosAvatar.m_downBody);
					cosCmd.Parameters.AddWithValue("@cf", (int)cosAvatar.m_foot);
					cosCmd.Parameters.AddWithValue("@cah", (int)cosAvatar.m_acHead);
					cosCmd.Parameters.AddWithValue("@caf", (int)cosAvatar.m_acFace);
					cosCmd.Parameters.AddWithValue("@cak", (int)cosAvatar.m_acHand);
					cosCmd.Parameters.AddWithValue("@cab", (int)cosAvatar.m_acBack);
					cosCmd.Parameters.AddWithValue("@can", (int)cosAvatar.m_acNeck);
					cosCmd.Parameters.AddWithValue("@cp", (int)cosAvatar.m_pet);
					cosCmd.Parameters.AddWithValue("@ce", (int)cosAvatar.m_expansion);
					cosCmd.Parameters.AddWithValue("@cw", (int)cosAvatar.m_acWrist);
					cosCmd.Parameters.AddWithValue("@cb", (int)cosAvatar.m_acBooster);
					cosCmd.Parameters.AddWithValue("@ctl", (int)cosAvatar.m_accTail);
					cosCmd.Parameters.AddWithValue("@cm", useCostume ? 1 : 0);
					cosCmd.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("usp_myRoomSetSlotItemSetting Error: {0}", ex.ToString());
			}
			return false;
		}

		private static void MyRoomGetUserSlotInfo(int UserNum, out List<MyRoomSlotInfo> m_vSlotInfo)
		{
			m_vSlotInfo = new List<MyRoomSlotInfo>();
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_myRoomGetUserSlotInfo");
				mySqlCommandHelper.AddParamInt("pUserNum", UserNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					MyRoomSlotInfo item = default(MyRoomSlotInfo);
					item.m_iSlotNum = mySqlCommandHelper.GetInt("fdSlotNum");
					item.m_strSlotName = mySqlCommandHelper.GetString("fdSlotName");
					mySqlCommandHelper.getResultAvatarInfo(ref item.m_AvatarInfo);
					mySqlCommandHelper.getResultAvatarInfo(ref item.m_CostumeAvatarInfo, bCostume: true);
					item.m_bIsUseCostume = mySqlCommandHelper.GetInt("costumeMode") != 0;
					m_vSlotInfo.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_myRoomGetUserSlotInfo Error: {0}", ex.ToString());
			}
		}

		private static byte AcceptGiftEx(Account User, string strGiftNum, out List<int> itemList)
		{
			itemList = new List<int>();
			byte result = 0;
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_shopAcceptGiftEx";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("strGiftNum", MySqlDbType.VarString).Value = strGiftNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				long tR = User.TR;
				long exp = User.Exp;
				while (mySqlDataReader.Read())
				{
					result = Convert.ToByte(mySqlDataReader["ret"]);
					itemList.Add(mySqlDataReader.GetInt32("fdItemNum"));
					exp = Convert.ToInt64(mySqlDataReader["exp"]);
					tR = Convert.ToInt64(mySqlDataReader["gamemoney"]);
				}
				User.TR = tR;
				User.Exp = exp;
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_shopAcceptGiftEx Error:{0}", ex.Message);
				return 1;
			}
		}

		private static byte StorageReceiveEx(Account User, int type, string strUniqueNum, out List<long> itemList)
		{
			byte result = 0;
			itemList = new List<long>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_storage_receiveEx";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
				mySqlCommand.Parameters.Add("type", MySqlDbType.Int32).Value = type;
				mySqlCommand.Parameters.Add("strUniqueNum", MySqlDbType.VarString).Value = strUniqueNum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					result = Convert.ToByte(mySqlDataReader["retval"]);
					itemList.Add(mySqlDataReader.GetInt64("fdUniqueNum"));
				}
				return result;
			}
			catch (Exception ex)
			{
				Log.Error("usp_storage_receiveEx Error:{0}", ex.Message);
				return 1;
			}
		}

		private static byte StorageGift(Account User, string nickname, string msg, long UniqueNum)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_storage_gift";
			mySqlCommand.Parameters.Add("userNum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("sendNickname", MySqlDbType.VarString).Value = User.NickName;
			mySqlCommand.Parameters.Add("targetNickname", MySqlDbType.VarString).Value = nickname;
			mySqlCommand.Parameters.Add("uniqueNum", MySqlDbType.Int64).Value = UniqueNum;
			mySqlCommand.Parameters.Add("memo", MySqlDbType.VarString).Value = msg;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static byte RepairItem(Account User, int itemnum, int repairitemnum)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_alchemist_repairItem";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("repairTargetItemdescnum", MySqlDbType.Int32).Value = itemnum;
			mySqlCommand.Parameters.Add("repairItemDescNum", MySqlDbType.Int32).Value = repairitemnum;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static byte FeedPet(Account User, int petitemnum, int feeditemnum, int addDays, int addExp, int feedcount)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_myRoomPetFeed";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("petItemDescNum", MySqlDbType.Int32).Value = petitemnum;
			mySqlCommand.Parameters.Add("petFeedItemDescNum", MySqlDbType.Int32).Value = feeditemnum;
			mySqlCommand.Parameters.Add("addDays", MySqlDbType.Int32).Value = addDays;
			mySqlCommand.Parameters.Add("addExp", MySqlDbType.Int32).Value = addExp;
			mySqlCommand.Parameters.Add("feedcount", MySqlDbType.Int32).Value = feedcount;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static byte FeedAllPet(Account User, int feeditemnum, int addDays, int addExp, int feedcount)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_myRoomPetFeedToAll";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("petFeedItemDescNum", MySqlDbType.Int32).Value = feeditemnum;
			mySqlCommand.Parameters.Add("addDays", MySqlDbType.Int32).Value = addDays;
			mySqlCommand.Parameters.Add("addExp", MySqlDbType.Int32).Value = addExp;
			mySqlCommand.Parameters.Add("feedcount", MySqlDbType.Int32).Value = feedcount;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static byte PetUpgrade(Account User, int petitemnum, int needexp)
		{
			byte b = 0;
			using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
			mySqlConnection.Open();
			using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
			mySqlCommand.Parameters.Clear();
			mySqlCommand.CommandType = CommandType.StoredProcedure;
			mySqlCommand.CommandText = "usp_myRoomPetUpgrade";
			mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = User.UserNum;
			mySqlCommand.Parameters.Add("petitemdescnum", MySqlDbType.Int32).Value = petitemnum;
			mySqlCommand.Parameters.Add("needExp", MySqlDbType.Int32).Value = needexp;
			using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader(CommandBehavior.SingleRow);
			mySqlDataReader.Read();
			return Convert.ToByte(mySqlDataReader["retval"]);
		}

		private static bool ItemOnOff(Account User, int itemnum, bool bOnOff, byte last)
		{
			bool flag = false;
			string text = string.Empty;
			try
			{
				text = ((!bOnOff) ? "usp_item_Off" : "usp_item_On");
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper(text);
				mySqlCommandHelper.AddParamInt("UserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("ItemDescNum", itemnum);
				mySqlCommandHelper.ExecuteNonQuery();
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("{0} Error: {1}", text, ex.ToString());
			}
			if (flag)
			{
				ShopItemTable.getItemDataFromItemDescNum(itemnum, out var itemData);
				User.updateItemOnOff(itemnum, bOnOff, out var offItemInfo, out var onItemInfo);
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_UPDATE_ITEM_ONOFF_INFO(User, itemnum, itemData.m_iOnOffType, itemData.m_iPosition, bOnOff, offItemInfo, onItemInfo, last), room.RoomServerID);
				}
				else
				{
					User.Connection.SendAsync(new ITEM_ONOFF_ACK(itemnum, itemData.m_iOnOffType, itemData.m_iPosition, bOnOff, last));
				}
			}
			return flag;
		}

		private static int CanOpenLevelLimitItem(int userNum, int itemNum)
		{
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand("usp_CanOpenLevelLimitItem", mySqlConnection);
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = userNum;
				mySqlCommand.Parameters.Add("pItemNum", MySqlDbType.Int32).Value = itemNum;
				MySqlParameter mySqlParameter = mySqlCommand.Parameters.Add("ret", MySqlDbType.Int32);
				mySqlParameter.Direction = ParameterDirection.Output;
				mySqlCommand.ExecuteNonQuery();
				if (mySqlParameter.Value == null || mySqlParameter.Value == DBNull.Value)
				{
					return 0;
				}
				return Convert.ToInt32(mySqlParameter.Value);
			}
			catch (Exception ex)
			{
				Log.Warning("usp_CanOpenLevelLimitItem: {0}", ex.Message);
				return 0;
			}
		}

		private static bool UseLuckyBag(Account User, int itemnum, byte OpenNum, out Dictionary<int, int> itemlist)
		{
			itemlist = new Dictionary<int, int>();
			try
			{
				int canOpen = CanOpenLevelLimitItem(User.UserNum, itemnum);
				if (canOpen != 0)
				{
					Log.Warning("UseLuckyBag level-limit user={0} item={1} ret={2}", User.UserNum, itemnum, canOpen);
					return false;
				}
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_myRoomUseLuckyBag_New";
					mySqlCommand.Parameters.Add("pUserNum", MySqlDbType.Int32).Value = User.UserNum;
					mySqlCommand.Parameters.Add("pItemDescNum", MySqlDbType.Int32).Value = itemnum;
					mySqlCommand.Parameters.Add("pOpenNum", MySqlDbType.Int32).Value = OpenNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					long tR = User.TR;
					long exp = User.Exp;
					bool found = false;
					do
					{
						if (!HasReaderColumn(mySqlDataReader, "resultItem"))
						{
							continue;
						}
						while (mySqlDataReader.Read())
						{
							int key = Convert.ToInt32(mySqlDataReader["resultItem"]);
							if (key <= 0)
							{
								continue;
							}
							// Prefer real/supply desc so client item table can resolve the icon.
							key = ItemHandle.ResolveGiftItemDescNum(key);
							if (key <= 0 || !ShopItemTable.getItemDataFromItemDescNum(key, out _))
							{
								Log.Warning("UseLuckyBag skip unknown reward user={0} raw/resolved={1}", User.UserNum, key);
								continue;
							}
							found = true;
							if (!itemlist.ContainsKey(key))
							{
								itemlist[key] = 1;
							}
							else
							{
								itemlist[key]++;
							}
							if (HasReaderColumn(mySqlDataReader, "totalExp") && !mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("totalExp")))
							{
								exp = Convert.ToInt64(mySqlDataReader["totalExp"]);
							}
							if (HasReaderColumn(mySqlDataReader, "totalGameMoney") && !mySqlDataReader.IsDBNull(mySqlDataReader.GetOrdinal("totalGameMoney")))
							{
								tR = Convert.ToInt64(mySqlDataReader["totalGameMoney"]);
							}
						}
					}
					while (mySqlDataReader.NextResult());
					if (found && itemlist.Count > 0)
					{
						User.TR = tR;
						User.Exp = exp;
						return true;
					}
				}
				Log.Warning("UseLuckyBag empty result user={0} item={1} open={2}", User.UserNum, itemnum, OpenNum);
				return false;
			}
			catch (Exception ex)
			{
				Log.Error("Error on using lucky bag:{0}, itemnum:{1}", ex.Message, itemnum);
				return false;
			}
		}

		private static bool HasReaderColumn(IDataRecord record, string name)
		{
			for (int i = 0; i < record.FieldCount; i++)
			{
				if (string.Equals(record.GetName(i), name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		private static bool GetFavorite(int UserNum, out List<int> itemlist)
		{
			itemlist = new List<int>();
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_myRoom_GetFavorite";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
					if (mySqlDataReader.HasRows)
					{
						while (mySqlDataReader.Read())
						{
							itemlist.Add(Convert.ToInt32(mySqlDataReader["fdItemNum"]));
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Error on usp_myRoom_GetFavorite:\r\n{0}", ex.Message);
				return false;
			}
		}

		private static bool ModifyFavorite(int UserNum, int itemnum, int type)
		{
			try
			{
				using (MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr))
				{
					mySqlConnection.Open();
					using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
					mySqlCommand.Parameters.Clear();
					mySqlCommand.CommandType = CommandType.StoredProcedure;
					mySqlCommand.CommandText = "usp_myRoom_ModifyFavorite";
					mySqlCommand.Parameters.Add("UserNum", MySqlDbType.Int32).Value = UserNum;
					mySqlCommand.Parameters.Add("ItemNum", MySqlDbType.Int32).Value = itemnum;
					mySqlCommand.Parameters.Add("Type", MySqlDbType.Int32).Value = type;
					mySqlCommand.ExecuteNonQuery();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Error on usp_myRoom_ModifyFavorite:\r\n{0}", ex.Message);
				return false;
			}
		}

		private static void myRoomPetRebirth(int UserNum, int petitemnum, int rebirthitemnum, out List<int> PetItemlist)
		{
			PetItemlist = new List<int>();
			try
			{
				using MySqlConnection mySqlConnection = new MySqlConnection(Conf.Connstr);
				mySqlConnection.Open();
				using MySqlCommand mySqlCommand = new MySqlCommand(string.Empty, mySqlConnection);
				mySqlCommand.Parameters.Clear();
				mySqlCommand.CommandType = CommandType.StoredProcedure;
				mySqlCommand.CommandText = "usp_myRoomPetRebirth";
				mySqlCommand.Parameters.Add("usernum", MySqlDbType.Int32).Value = UserNum;
				mySqlCommand.Parameters.Add("petItemNum", MySqlDbType.Int32).Value = petitemnum;
				mySqlCommand.Parameters.Add("petRebirthItemNum", MySqlDbType.Int32).Value = rebirthitemnum;
				using MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
				while (mySqlDataReader.Read())
				{
					PetItemlist.Add(mySqlDataReader.GetInt32("fdPetItemNum"));
				}
			}
			catch (Exception ex)
			{
				Log.Error("usp_myRoomPetRebirth Error:{0}", ex.Message);
			}
		}

		private static bool UseExtraAbilityItem(Account User, int ItemNum, byte last)
		{
			CActiveItems cActiveItems = new CActiveItems();
			Dictionary<int, ExtraAbilityInfo> dictionary = new Dictionary<int, ExtraAbilityInfo>();
			bool flag = false;
			int num = 0;
			int divinationType = 0;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_useExtraAbilityItem");
				mySqlCommandHelper.AddParamInt("userNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("itemNum", ItemNum);
				mySqlCommandHelper.AddParamInt("coupleNum", User.CoupleInfo.CoupleNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					short key = (short)mySqlCommandHelper.GetInt("attr");
					float @float = mySqlCommandHelper.GetFloat("value");
					int @int = mySqlCommandHelper.GetInt("itemNum");
					int int2 = mySqlCommandHelper.GetInt("limit");
					long dateTime = mySqlCommandHelper.GetDateTime("gotTime", 0L);
					if (!dictionary.TryGetValue(@int, out var value))
					{
						value = new ExtraAbilityInfo();
					}
					value.iItemDescNum = @int;
					value.iLimitTime = int2;
					value.tGotTime = dateTime;
					if (!value.mapAttributes.ContainsKey(key))
					{
						value.mapAttributes.Add(key, @float);
					}
					if (!dictionary.ContainsKey(@int))
					{
						dictionary.Add(@int, value);
					}
				}
				mySqlCommandHelper.NextResult();
				while (mySqlCommandHelper.HasResult())
				{
					int int3 = mySqlCommandHelper.GetInt("itemNum");
					cpk_type character = mySqlCommandHelper.GetInt("itemCharacter");
					cpk_type position = mySqlCommandHelper.GetInt("itemPosition");
					cpk_type kind = mySqlCommandHelper.GetInt("itemKind");
					bool boolean = mySqlCommandHelper.GetBoolean("using");
					int int4 = mySqlCommandHelper.GetInt("itemCount");
					long expiretime = 0L;
					if (!mySqlCommandHelper.IsDBNull("expireTime"))
					{
						expiretime = mySqlCommandHelper.GetDateTime("expireTime", 0L);
					}
					cActiveItems.insertItem(new NetItemInfo(int3, character, position, kind, boolean, int4, expiretime, 0L));
				}
				mySqlCommandHelper.NextResult();
				if (mySqlCommandHelper.HasResult())
				{
					num = mySqlCommandHelper.GetInt("remainItemCount");
					divinationType = mySqlCommandHelper.GetInt("divinationType");
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_useExtraAbilityItem Error:{0}", ex.Message);
			}
			if (flag)
			{
				User.activeItem.updateItemCount(ItemNum, num);
				User.Connection.SendAsync(new Myroom_UseExtraAbilityItem_ACK(User, divinationType, ItemNum, num, dictionary, last));
				foreach (NetItemInfo item in cActiveItems.getVector())
				{
					User.activeItem.replaceItemInfoByItem(item);
				}
				if (User.isInRoom(out var room))
				{
					ServerStatus.ToRoomServer(new eRoom_CHANGE_USER_ACTIVE_ITEM_ONE(User, cActiveItems, dictionary, last), room.RoomServerID);
				}
			}
			return flag;
		}

		private static bool UserCharacterStatReset(Account User, int character, int statType, byte last)
		{
			UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo();
			bool flag = false;
			int statNum = 0;
			long tR = 0L;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_UserCharacterStatReset");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pcharacter", character);
				mySqlCommandHelper.AddParamInt("statType", statType);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					tR = mySqlCommandHelper.GetLongConvert("remain");
					statNum = mySqlCommandHelper.GetInt("statNum");
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					userItemAttrInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemDescNum");
					userItemAttrInfo.m_ItemAttr[@short] = @float;
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_UserCharacterStatReset Error:{0}", ex.Message);
			}
			if (flag)
			{
				User.TR = tR;
				// Reset ACK is preview only. Persisted attrs apply on CONFIRM.
				User.Connection.SendAsync(new Myroom_CharacterStatReset_ACK(statNum, userItemAttrInfo, last));
			}
			return flag;
		}

		private static bool UserCharacterStatConfirm(Account User, int character, int statNum, byte last)
		{
			UserItemAttrInfo userItemAttrInfo = new UserItemAttrInfo();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_UserCharacterStatConfirm");
				mySqlCommandHelper.AddParamInt("pUserNum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pcharacter", character);
				mySqlCommandHelper.AddParamInt("statNum", statNum);
				mySqlCommandHelper.Execute();
				while (mySqlCommandHelper.HasResult())
				{
					short @short = mySqlCommandHelper.GetShort("AttrType");
					float @float = mySqlCommandHelper.GetFloat("AttrValue");
					userItemAttrInfo.m_iItemDescNum = mySqlCommandHelper.GetInt("itemDescNum");
					userItemAttrInfo.m_ItemAttr[@short] = @float;
				}
				flag = true;
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_UserCharacterStatConfirm Error:{0}", ex.Message);
			}
			if (flag)
			{
				User.userItemAttr.insertCharAttr(userItemAttrInfo);
				User.mixUserItemAttr();
				User.Connection.SendAsync(new Myroom_CharacterStatConfirm_ACK(userItemAttrInfo, last));
				User.Connection.SendAsync(new eServer_GET_USER_CHARACTER_ATTR_ACK(User, last));
			}
			else
			{
				User.Connection.SendAsync(new Myroom_CharacterStatConfirmFail(last));
			}
			return flag;
		}

		private static bool myRoomGetCharacterSetting(Account User, int characterKind, out AvatarInfo m_realAvatarInfo)
		{
			m_realAvatarInfo = default(AvatarInfo);
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_myRoomGetCharacterSetting");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("characterKind", characterKind);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					mySqlCommandHelper.getResultAvatarInfo(ref m_realAvatarInfo);
					flag = true;
				}
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_myRoomGetCharacterSetting Error:{0}", ex.Message);
			}
			// Empty body → ensure a zeroed setting row exists (no Essen starter suit).
			if (!flag || IsRealBodyEmpty(m_realAvatarInfo))
			{
				if (TrySeedEmptyCharacterSetting(User.UserNum, characterKind, out var seeded))
				{
					m_realAvatarInfo = seeded;
					flag = true;
				}
			}
			return flag;
		}

		private static bool TryLoadCostumeSetting(int userNum, int characterKind, out AvatarInfo cos)
		{
			cos = default(AvatarInfo);
			cos.m_character = (ushort)characterKind;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using MySqlCommand cmd = new MySqlCommand(
					@"SELECT fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot,
fdACHead, fdACFace, fdACHand, fdACBack, fdACNeck, fdPet, fdExpansion,
fdACWrist, fdACBooster, IFNULL(fdPart,0) AS fdPart
FROM UserCostumeCharacterSetting WHERE fdUserNum=@u AND fdCharacter=@c LIMIT 1", conn);
				cmd.Parameters.AddWithValue("@u", userNum);
				cmd.Parameters.AddWithValue("@c", characterKind);
				using MySqlDataReader r = cmd.ExecuteReader();
				if (!r.Read())
				{
					return false;
				}
				cos.m_character = (ushort)r.GetInt32(0);
				cos.SetWear(1, (ushort)r.GetInt32(1));
				cos.SetWear(2, (ushort)r.GetInt32(2));
				cos.SetWear(3, (ushort)r.GetInt32(3));
				cos.SetWear(4, (ushort)r.GetInt32(4));
				cos.SetWear(5, (ushort)r.GetInt32(5));
				cos.SetWear(6, (ushort)r.GetInt32(6));
				cos.SetWear(7, (ushort)r.GetInt32(7));
				cos.SetWear(8, (ushort)r.GetInt32(8));
				cos.SetWear(9, (ushort)r.GetInt32(9));
				cos.SetWear(10, (ushort)r.GetInt32(10));
				cos.SetWear(11, (ushort)r.GetInt32(11));
				cos.SetWear(12, (ushort)r.GetInt32(12));
				cos.SetWear(13, (ushort)r.GetInt32(13));
				cos.SetWear(14, (ushort)r.GetInt32(14));
				return true;
			}
			catch (Exception ex)
			{
				Log.Warning("TryLoadCostumeSetting: {0}", ex.Message);
				return false;
			}
		}

		private static bool IsRealBodyEmpty(AvatarInfo info)
		{
			return info.m_head == 0 && info.m_topBody == 0 && info.m_downBody == 0 && info.m_foot == 0
				&& info.m_acHead == 0 && info.m_pet == 0 && info.m_acBack == 0;
		}

		/// <summary>
		/// Ensure an unlocked character has a setting row. Always zeros — no default suit.
		/// </summary>
		private static bool TrySeedEmptyCharacterSetting(int userNum, int characterKind, out AvatarInfo info)
		{
			info = default(AvatarInfo);
			info.m_character = (ushort)characterKind;
			try
			{
				using MySqlConnection conn = new MySqlConnection(Conf.Connstr);
				conn.Open();
				using (MySqlCommand unlock = new MySqlCommand(
					@"SELECT 1 FROM tblAvatarUserCharacter t1
LEFT JOIN EssenAvatarItemCPKRef t2 ON t1.fdItemDescNum = t2.fdItemNum
LEFT JOIN tblAvatarItemDesc d ON t1.fdItemDescNum = d.fdItemNum
WHERE t1.fdUserNum=@u AND (
  (t2.fdPos=0 AND t2.fdChar=@c) OR (d.fdPosition=0 AND d.fdCharacter=@c)
) LIMIT 1", conn))
				{
					unlock.Parameters.AddWithValue("@u", userNum);
					unlock.Parameters.AddWithValue("@c", characterKind);
					if (unlock.ExecuteScalar() == null)
					{
						return false;
					}
				}
				using (MySqlCommand upsert = new MySqlCommand(@"
INSERT IGNORE INTO tblAvatarCharacterSetting
  (fdUserNum, fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot, fdACHead, fdACFace, fdACHand, fdACBack, fdACNeck, fdPet, fdExpansion, fdACWrist, fdACBooster, fdPart)
VALUES (@u, @c, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)", conn))
				{
					upsert.Parameters.AddWithValue("@u", userNum);
					upsert.Parameters.AddWithValue("@c", characterKind);
					upsert.ExecuteNonQuery();
				}
				using MySqlCommand sel = new MySqlCommand(@"
SELECT fdCharacter, fdHead, fdTopBody, fdDownBody, fdFoot, fdACHead, fdACFace, fdACHand, fdACBack, fdACNeck, fdPet, fdExpansion, fdACWrist, fdACBooster, fdPart
FROM tblAvatarCharacterSetting WHERE fdUserNum=@u AND fdCharacter=@c", conn);
				sel.Parameters.AddWithValue("@u", userNum);
				sel.Parameters.AddWithValue("@c", characterKind);
				using MySqlDataReader r = sel.ExecuteReader();
				if (!r.Read())
				{
					return characterKind > 0;
				}
				info.m_character = Convert.ToUInt16(r["fdCharacter"]);
				info.m_head = Convert.ToUInt16(r["fdHead"]);
				info.m_topBody = Convert.ToUInt16(r["fdTopBody"]);
				info.m_downBody = Convert.ToUInt16(r["fdDownBody"]);
				info.m_foot = Convert.ToUInt16(r["fdFoot"]);
				info.m_acHead = Convert.ToUInt16(r["fdACHead"]);
				info.m_acFace = Convert.ToUInt16(r["fdACFace"]);
				info.m_acHand = Convert.ToUInt16(r["fdACHand"]);
				info.m_acBack = Convert.ToUInt16(r["fdACBack"]);
				info.m_acNeck = Convert.ToUInt16(r["fdACNeck"]);
				info.m_pet = Convert.ToUInt16(r["fdPet"]);
				info.m_expansion = Convert.ToUInt16(r["fdExpansion"]);
				info.m_acWrist = Convert.ToUInt16(r["fdACWrist"]);
				info.m_acBooster = Convert.ToUInt16(r["fdACBooster"]);
				info.m_accTail = Convert.ToUInt16(r["fdPart"]);
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("TrySeedEmptyCharacterSetting: {0}", ex.Message);
				return false;
			}
		}

		private static bool myRoomSetDefaultCharacter(Account User, int charid, bool sendAvatar)
		{
			AdvancedAvatarInfo advancedAvatarInfo = new AdvancedAvatarInfo();
			bool flag = false;
			try
			{
				using MySqlCommandHelper mySqlCommandHelper = new MySqlCommandHelper("usp_myRoomSetDefaultCharacter");
				mySqlCommandHelper.AddParamInt("usernum", User.UserNum);
				mySqlCommandHelper.AddParamInt("pcharacter", charid);
				mySqlCommandHelper.Execute();
				if (mySqlCommandHelper.HasResult())
				{
					mySqlCommandHelper.getResultAvatarInfo(ref advancedAvatarInfo.m_realAvatarInfo);
					mySqlCommandHelper.getResultAvatarInfo(ref advancedAvatarInfo.m_costumeAvatarInfo, bCostume: true);
					advancedAvatarInfo.m_bIsUseCostume = mySqlCommandHelper.GetBoolean("costumeMode");
					// Inner 0B already wrote the toggle into memory; this SP reload
					// must not replace it (GetBoolean can still return the old row).
					advancedAvatarInfo.m_bIsUseCostume = User.advancedAvatarInfo.m_bIsUseCostume;
					if ((int)advancedAvatarInfo.m_costumeAvatarInfo.m_character == 0
						&& (int)advancedAvatarInfo.m_realAvatarInfo.m_character != 0)
					{
						advancedAvatarInfo.m_costumeAvatarInfo.m_character = advancedAvatarInfo.m_realAvatarInfo.m_character;
					}
					flag = true;
				}
			}
			catch (Exception ex)
			{
				flag = false;
				Log.Error("usp_myRoomSetDefaultCharacter Error: {0}", ex.ToString());
			}
			if (flag && sendAvatar)
			{
				try
				{
					User.setAvatarInfoAndSendTCP(advancedAvatarInfo);
				}
				catch (Exception exSend)
				{
					Log.Warning("myRoomSetDefaultCharacter send user={0}: {1}", User.UserID, exSend);
				}
			}
			return flag;
		}

		/// <summary>65535 = unchanged on Thai wire — replace from stored before save/send.</summary>
		private static void MergeKeepMaxValue(ref AvatarInfo incoming, AvatarInfo stored)
		{
			if ((int)stored.m_character == 0)
			{
				return;
			}
			for (byte b = 0; b < 15; b = (byte)(b + 1))
			{
				if (incoming.GetWear(b) == ushort.MaxValue)
				{
					incoming.SetWear(b, stored.GetWear(b));
				}
			}
			for (byte b = 0; b < 7; b = (byte)(b + 1))
			{
				if (incoming.GetAcc(b) == ushort.MaxValue)
				{
					incoming.SetAcc(b, stored.GetAcc(b));
				}
			}
			if (incoming.GetEF() == ushort.MaxValue)
			{
				incoming.SetEF(stored.GetEF());
			}
		}

		/// <summary>
		/// When a packet is mostly KEEP (65535), bare 0 means "slot not in this edit"
		/// only if there is no explicit clear wave. All-65535 body = unequip everything.
		/// </summary>
		private static void MergeKeepZeroWhenKeepMarkers(ref AvatarInfo incoming, AvatarInfo stored)
		{
			if ((int)stored.m_character == 0)
			{
				return;
			}
			if (HasExplicitClearMarkers(incoming))
			{
				for (byte b = 1; b < 15; b = (byte)(b + 1))
				{
					if (incoming.GetWear(b) == ushort.MaxValue)
					{
						incoming.SetWear(b, 0);
					}
				}
				return;
			}
			for (byte b = 1; b < 15; b = (byte)(b + 1))
			{
				ushort storedWear = stored.GetWear(b);
				if (incoming.GetWear(b) == 0 && storedWear != 0 && storedWear != ushort.MaxValue)
				{
					incoming.SetWear(b, storedWear);
				}
				else if (incoming.GetWear(b) == ushort.MaxValue)
				{
					incoming.SetWear(b, 0);
				}
			}
			for (byte b = 0; b < 7; b = (byte)(b + 1))
			{
				ushort storedAcc = stored.GetAcc(b);
				if (incoming.GetAcc(b) == ushort.MaxValue)
				{
					incoming.SetAcc(b, 0);
				}
				else if (incoming.GetAcc(b) == 0 && storedAcc != 0 && storedAcc != ushort.MaxValue)
				{
					incoming.SetAcc(b, storedAcc);
				}
			}
			ushort storedEf = stored.GetEF();
			if (incoming.GetEF() == ushort.MaxValue)
			{
				incoming.SetEF(0);
			}
			else if (incoming.GetEF() == 0 && storedEf != 0 && storedEf != ushort.MaxValue)
			{
				incoming.SetEF(storedEf);
			}
		}
	}
}
