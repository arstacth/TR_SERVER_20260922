using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring.Farm;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class GetFarmCraftMapData : NetPacket
	{
		// zlib(empty) — length 0 still calls UnCompress and throws on Thai client.
		private static readonly byte[] EmptyZlib = { 0x78, 0x9C, 0x03, 0x00, 0x00, 0x00, 0x00, 0x01 };

		public GetFarmCraftMapData(int FarmUniqueNum, FarmCraftMapData farmcraftmapdata, byte last, byte bUpdate = 1, int result = 0, bool packUnique = true)
		{
			ns.WriteOP(RoomOpcodes.eServer_FARM_CRAFT_PROTOCOL);
			ns.Write(3);
			if (result != 0)
			{
				// Thai OnRecvFarmCraft_GetVoxelsData_Failed — result 14 = no map.
				// ENTER must not use this (unbinds owner).
				ns.Write(result);
				_ = last;
				return;
			}
			bool hasCraft = farmcraftmapdata != null && farmcraftmapdata.isFarmCraft
				&& farmcraftmapdata.CompressedData != null && farmcraftmapdata.CompressedData.Length > 0;
			ns.Write(0);
			ns.Write(packUnique ? Utility.PackFarmUnique(FarmUniqueNum) : FarmUniqueNum);
			ns.Write((byte)0);
			ns.Write(bUpdate);
			if (hasCraft)
			{
				ns.Write(farmcraftmapdata.TotalBlock);
				ns.Write((ushort)farmcraftmapdata.CompressedData.Length);
				ns.Write(farmcraftmapdata.CompressedData, 0, farmcraftmapdata.CompressedData.Length);
			}
			else
			{
				ns.Write(0);
				ns.Write((ushort)EmptyZlib.Length);
				ns.Write(EmptyZlib, 0, EmptyZlib.Length);
			}
			_ = last;
		}
	}
}
