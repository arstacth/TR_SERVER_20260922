using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public class ClientCheckAutoBanACK : NetPacket
	{
		public ClientCheckAutoBanACK(int unk1, byte last)
		{
			ns.WriteOP(Opcodes.eServer_Blocking_To_Me_ACK);
			ns.Write(unk1);
			_ = last;
		}
	}
}
