using System;
using LocalCommons.Network;
using RoomServer.Structuring;
using RoomServer.Structuring.Opcode;

namespace RoomServer.Packet.Send
{
	public sealed class TowerEvent_Enter_ACK : NetPacket
	{
		public TowerEvent_Enter_ACK(byte result, int remainfree, int remaincash, bool IsUseItem, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TOWER_OF_ORDEAL_EVENT_ENTER_ACK);
			ns.Write(result);
			ns.Write(remainfree);
			ns.Write(remaincash);
			int num = (int)(ServerStatus.TowerEventEndTime - DateTime.Now).TotalSeconds;
			ns.Write(ServerStatus.TowerEventEnable ? num : 0);
			ns.Write(IsUseItem);
			_ = last;
		}
	}
}
