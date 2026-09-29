using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;
using Serilog;

namespace RoomServer.Packet.Send
{
	public sealed class SendFarmInfo : NetPacket
	{
		// Working Thai ENTER is size 171: trailing ownFarm as BYTE + last.
		// Writing ownFarm as int made PacketSize 174 with RemainSize=3 and Overpop.
		private const long PremiumUnsetMagic = 1842465389770955L;

		public SendFarmInfo(Account User, NormalRoom room, int FarmUniqueNum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_ACK);
			ns.Write(0);
			ns.Write(room.RoomKindID);
			ns.WriteBIG5Fixed_shortSize(room.Name);
			ns.Write(room.MaxPlayersCount);
			ns.WriteBIG5Fixed_shortSize(room.Password);
			ns.Write(2);
			ns.Write(room.ID);
			ns.Write(User.RoomPos);
			ns.Write(FarmMapNum(room.FarmRoomInfo.FarmTypeNum));
			ns.Write(30);
			ns.Write(room.IsTeamPlay);
			ns.Write(room.IsStepOn);
			ns.Write(room.ItemType);
			ns.Write(0);
			ns.Write(Utility.CurrentTimeMilliseconds());
			ns.Write(room.PosWeight);
			ns.Write(FarmUniqueNum);
			ns.Write(0);
			ns.Write(0);
			// Must stay 0. Writing FarmTypeNum=1 made client open map_s2\farm_65536.trv (fatal).
			// Map comes from MapNum=70 (69+type) above; this slot is not the map id.
			ns.Write(0);
			ns.WriteAnsiFixed_intSize(room.FarmRoomInfo.FarmName);
			ns.WriteAnsiFixed_intSize(room.FarmRoomInfo.MasterName);
			ns.Write(FixFarmTime(room.FarmRoomInfo.ExpireTime));
			ns.Write(FixFarmTime(room.FarmRoomInfo.CreateTime));
			bool ownFarm = (User.MyFarmUniqueNum > 0 && User.MyFarmUniqueNum == FarmUniqueNum)
				|| room.FarmRoomInfo.IsMaster;
			if (ownFarm && User.MyFarmUniqueNum <= 0 && FarmUniqueNum > 0)
			{
				User.MyFarmUniqueNum = FarmUniqueNum;
			}
			ns.Write(room.FarmRoomInfo.Password == string.Empty);
			ns.Write(room.FarmRoomInfo.isPublic);
			ns.Write((byte)(ownFarm ? 1 : 0));
			ns.Write(room.FarmRoomInfo.TotalCount);
			ns.Write(room.FarmRoomInfo.TodaysVisitorCount);
			// Login GetMyFarmInfo: mine byte + 2 pad + premium bool, then expire/exp/owner int.
			ns.Write((byte)(ownFarm ? 1 : 0));
			ns.Fill(2);
			ns.Write(room.FarmRoomInfo.PremiumFarmUsing);
			long premExp = room.FarmRoomInfo.PremiumFarmExpireDateTime;
			ns.Write(premExp > 0L ? FixFarmTime(premExp) : PremiumUnsetMagic);
			int farmExp = room.FarmRoomInfo.farmExp;
			if (farmExp < 1 && User.MyFarmInfo != null)
			{
				farmExp = User.MyFarmInfo.farmExp;
			}
			ns.Write(Utility.FarmHudExp(farmExp));
			// Thai ENTER size 171. ushort+byte pad made 173 Remain=2 (00:59).
			ns.Write((byte)0);
			_ = last;
			Log.Information("SendFarmInfo bytes={0} farm={1} own={2} my={3} kind={4} map={5}",
				ns.Length, FarmUniqueNum, ownFarm, User.MyFarmUniqueNum, room.RoomKindID, FarmMapNum(room.FarmRoomInfo.FarmTypeNum));
		}

		internal static int FarmTypeWire(int farmTypeNum = 1)
		{
			if (farmTypeNum <= 0)
			{
				farmTypeNum = 1;
			}
			return farmTypeNum;
		}

		internal static int PackedFarmTypeNum(int farmTypeNum = 1)
		{
			return FarmTypeWire(farmTypeNum);
		}

		internal static int FarmMapNum(int farmTypeNum)
		{
			if (farmTypeNum <= 0)
			{
				farmTypeNum = 1;
			}
			return 69 + farmTypeNum;
		}

		private static long FixFarmTime(long timeMs)
		{
			const long minOk = 946684800000L; // 2000-01-01
			const long maxOk = 2145916800000L; // ~2038
			if (timeMs <= 0L || timeMs < minOk)
			{
				return maxOk;
			}
			if (timeMs > maxOk)
			{
				return maxOk;
			}
			return timeMs;
		}
	}
}
