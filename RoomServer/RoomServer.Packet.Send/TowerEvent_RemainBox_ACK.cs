using System.Collections.Concurrent;
using System.Collections.Generic;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_RemainBox_ACK : NetPacket
	{
		public TowerEvent_RemainBox_ACK(ConcurrentDictionary<int, int> TowerEvent_BoxList, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_OREDEAL_ITEMINDEX_NOTIFY);
			ns.Write(TowerEvent_BoxList.Count);
			foreach (KeyValuePair<int, int> TowerEvent_Box in TowerEvent_BoxList)
			{
				ns.Write(TowerEvent_Box.Key);
				ns.Write(TowerEvent_Box.Value);
			}
			_ = last;
		}
	}
}
