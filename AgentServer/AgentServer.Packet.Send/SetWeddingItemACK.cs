using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class SetWeddingItemACK : NetPacket
	{
		public SetWeddingItemACK(byte last)
		{
			ns.WriteOP(RoomOpcodes.eRoom_WEDDING_CHANGE_AGREE_STATE_ACK);
			ns.Write((byte)0);
			_ = last;
		}
	}
}
