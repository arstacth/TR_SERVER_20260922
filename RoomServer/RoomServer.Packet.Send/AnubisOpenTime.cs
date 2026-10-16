using System;
using LocalCommons.Network;
using LocalCommons.Utilities;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class AnubisOpenTime : NetPacket
	{
		public AnubisOpenTime(byte last)
		{
			ns.WriteOP(Opcodes.eServer_ANUBIS_EXPEDITION_GET_INFO_ACK);
			ns.Write(Utility.ConvertToTimestamp(DateTime.Today));
			ns.Write(1439);
			ns.Fill(3);
			_ = last;
		}
	}
}
