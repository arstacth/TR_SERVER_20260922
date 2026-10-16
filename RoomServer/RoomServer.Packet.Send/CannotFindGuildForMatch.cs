using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class CannotFindGuildForMatch : NetPacket
	{
		public CannotFindGuildForMatch(byte last)
		{
			ns.WriteOP(Opcodes.eServer_GUILDMATCH_SEARCHING_PARTY_RETRY);
			_ = last;
		}
	}
}
