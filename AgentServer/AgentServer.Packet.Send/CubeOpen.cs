using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CubeOpen : NetPacket
	{
		public CubeOpen(int cubeitemnum, List<ITEM_OPEN_INFO> items_open, List<ITEM_UNOPEN_INFO> items_unopen, eCUBE_TYPE cube_type, eCUBE_OPEN_RESULT result, int max_acceptable, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEMCUBE_CUBEOPEN_ACK);
			ns.Write(cubeitemnum);
			ns.Write(items_open.Count);
			foreach (ITEM_OPEN_INFO item in items_open)
			{
				ns.Write(item.index);
				ns.Write(item.item);
				ns.Write(item.count);
				ns.Write(item.guage_delta);
				ns.Write(item.canmove_storage);
			}
			ns.Write(items_unopen.Count);
			foreach (ITEM_UNOPEN_INFO item2 in items_unopen)
			{
				ns.Write(item2.index);
				ns.Write(item2.item);
				ns.Write(item2.count);
			}
			ns.Write((int)cube_type);
			ns.Write((int)result);
			ns.Write(max_acceptable);
			_ = last;
		}

		public CubeOpen(int cubeitemnum, CUBE_INFO_PROCESS process, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEMCUBE_CUBEOPEN_ACK);
			ns.Write(cubeitemnum);
			ns.Write(process.result.items_open.Count);
			foreach (ITEM_OPEN_INFO item in process.result.items_open)
			{
				ns.Write(item.index);
				ns.Write(item.item);
				ns.Write(item.count);
				ns.Write(item.guage_delta);
				ns.Write(item.canmove_storage);
			}
			ns.Write(process.result.items_unopen.Count);
			foreach (ITEM_UNOPEN_INFO item2 in process.result.items_unopen)
			{
				ns.Write(item2.index);
				ns.Write(item2.item);
				ns.Write(item2.count);
			}
			ns.Write((int)process.result.cube_type);
			ns.Write((int)process.result.open_result);
			ns.Write(process.result.max_acceptable);
			_ = last;
		}
	}
}
