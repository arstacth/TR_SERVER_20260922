using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CancelLookingForGuildMatch : NetPacket
	{
		public CancelLookingForGuildMatch(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_SEARCHING_PARTY_CANCEL_ACK);
			_ = last;
		}
	}
}
