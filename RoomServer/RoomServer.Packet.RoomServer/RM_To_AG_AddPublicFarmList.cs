using LocalCommons.Network;
using NetMsg.Room;
using RoomServer.Structuring;

namespace RoomServer.Packet.RoomServer
{
	public sealed class RM_To_AG_AddPublicFarmList : NetPacket
	{
		public RM_To_AG_AddPublicFarmList(NormalRoom farmroom)
		{
			ns.WriteOP(RMProtocol.RM_AddPublicFarmList_REQ);
			ns.Write(farmroom.ID);
			ns.Write(farmroom.FarmIndex);
			ns.Write(farmroom.FarmRoomInfo.FarmTypeNum);
			ns.WriteAnsiFixed_intSize(farmroom.Name);
			ns.Write(farmroom.HasPassword);
			ns.Write((byte)farmroom.PlayerCount());
			ns.Write(farmroom.MaxPlayersCount);
			ns.Write(1);
			ns.Write((short)0);
			ns.Write((byte)farmroom.PlayerCount());
			ns.Write(farmroom.FarmRoomInfo.farmExp);
			ns.Write(1);
			ns.WriteAnsiFixed_intSize(farmroom.FarmRoomInfo.MasterName);
			ns.Write(farmroom.FarmRoomInfo.Type);
			ns.Write(farmroom.MaxPlayersCount);
			ns.Write((byte)0);
			ns.Write((byte)0);
			ns.Write(farmroom.PlayerCount());
			foreach (Account item in farmroom.PlayerList())
			{
				ns.WriteAnsiFixed_intSize(item.NickName);
			}
			ns.Write(farmroom.hasFishingReward);
		}
	}
}
