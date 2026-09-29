using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class FarmCraft_ModifyFarmMapOK : NetPacket
	{
		public FarmCraft_ModifyFarmMapOK(byte last, int innerOpcode = 2)
		{
			ns.WriteOP(RoomOpcodes.eServer_FARM_CRAFT_PROTOCOL);
			ns.Write(innerOpcode);
			ns.Write(0);
			_ = last;
		}
	}
}
