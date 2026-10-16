using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GameRoom_SendRoomInfo : NetPacket
	{
		public GameRoom_SendRoomInfo(NormalRoom room, byte last, byte roompos = 0)
		{
			int roomKindID = room.RoomKindID;
			string name = PackedAsciiName(room.Name);
			string password = room.Password ?? string.Empty;
			int isTeamPlay = room.IsTeamPlay;
			bool isStepOn = room.IsStepOn;
			int itemType = room.ItemType;
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_ACK);
			ns.Write(0);
			ns.Write(roomKindID);
			ns.WriteBIG5Fixed_shortSize(name);
			// Packed MaxPlayers is a byte. 19:08/19:23 ushort made password
			// length misalign (Remain 35 size 63) and the client crashed
			// before NEW_ROOM_USER. 18:38 byte was Remain 2 size 62.
			ns.Write(room.MaxPlayersCount);
			ns.WriteBIG5Fixed_shortSize(password);
			int value = 2;
			ns.Write(value);
			int iD = room.ID;
			ns.Write(iD);
			ns.Write(roompos);
			int mapWire = room.MapNum;
			if (roomKindID == 74 && mapWire <= 0)
			{
				mapWire = 1;
			}
			ns.Write(mapWire);
			ns.Write(5);
			ns.Write(isTeamPlay);
			ns.Write(isStepOn);
			// Client 0x759820 reads item as a byte, then map, a flag byte, time, weight, one more byte.
			ns.Write((byte)itemType);
			ns.Write(mapWire);
			ns.Write((byte)0);
			ns.Write(Utility.CurrentTimeMilliseconds());
			ns.Write(room.PosWeight);
			ns.Write((byte)0);
			// Kind 0 (and 1, 2, 20-22, 24, 25) then reads an int count. This create was kind 0
			// and overpopped 79 80 4. Count 0 skips the id loop.
			if (roomKindID == 0 || (roomKindID > 0 && roomKindID <= 0x19 && ((0x3700006 >> roomKindID) & 1) != 0))
			{
				ns.Write(0);
			}
			_ = last;
		}

		private static string PackedAsciiName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return "Room";
			}
			// Thai room titles are TIS-620 / CP874, not ASCII. Stripping non-ASCII
			// forced every create to the literal "Room".
			string trimmed = name.Trim();
			if (trimmed.Length == 0)
			{
				return "Room";
			}
			if (trimmed.Length > 24)
			{
				return trimmed.Substring(0, 24);
			}
			return trimmed;
		}
	}
}
