using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class Farm_ChangeFarmMapTypeBySlot_UpdateItem : NetPacket
	{
		public Farm_ChangeFarmMapTypeBySlot_UpdateItem(List<FarmMapInfo> farmmapinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmMapTypeBySlot_UpdateItem);
			ns.Write(farmmapinfo.Count);
			foreach (FarmMapInfo item in farmmapinfo)
			{
				ns.Write(1);
				ns.Write(0);
				ns.Write(item.FarmItemID);
				ns.Write(item.CellPosX);
				ns.Write(item.CellPosY);
				ns.Write(item.CellPosZ);
				ns.Write(item.CellWidth);
				ns.Write(item.CellHeight);
				ns.Write(item.BlockPosX);
				ns.Write(item.BlockPosY);
				ns.Write(item.BlockWidth);
				ns.Write(item.BlockHeight);
				ns.Write(item.RotateType);
				ns.Write((short)0);
				ns.Write(item.Angle);
				ns.Write(item.ItemDescNum);
				ns.Fill(12);
				ns.Write(item.Exp);
				ns.Write(item.View);
				ns.Write(item.NowTime);
			}
			_ = last;
		}
	}
	public sealed class Farm_ChangeFarmMapTypeBySlot_UpdateInfo : NetPacket
	{
		public Farm_ChangeFarmMapTypeBySlot_UpdateInfo(int FarmUniqueNum, NormalRoom room, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmMapTypeBySlot_UpdateInfo);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(room.FarmRoomInfo.FarmTypeNum);
			ns.Write(room.FarmRoomInfo.FarmSkyTypeNum);
			ns.Write(room.FarmRoomInfo.FarmWeatherTypeNum);
			_ = last;
		}
	}
}
