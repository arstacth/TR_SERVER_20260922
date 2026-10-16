using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class SendFarmItemPos : NetPacket
	{
		public SendFarmItemPos(List<FarmMapInfo> farmmapinfo, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ENTER_ROOM_DATA_RECV_PROC_ACK);
			ns.Write(farmmapinfo.Count);
			foreach (FarmMapInfo item in farmmapinfo)
			{
				ns.Write(1);
				ns.Write(15);
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
				ns.Write((short)30792);
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
}
