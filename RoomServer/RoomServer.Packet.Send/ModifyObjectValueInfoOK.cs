using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class ModifyObjectValueInfoOK : NetPacket
	{
		public ModifyObjectValueInfoOK(long FarmItemID, int value, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ModifyObjectValueInfo_ACK);
			ns.Write(FarmItemID);
			ns.Write(value);
			_ = last;
		}
	}
}
