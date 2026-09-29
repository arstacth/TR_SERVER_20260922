using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Park;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class EventPickBoardUserInfo_ACK : NetPacket
	{
		public EventPickBoardUserInfo_ACK(int PickBoardNum, EventPickBoardInfo t, byte PickBoardStep, int RemainCount, Dictionary<byte, ExchangeItemInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_EVENT_PICK_BOARD_USER_INFO_ACK);
			ns.Write(PickBoardNum);
			ns.Write(0);
			ns.Write(PickBoardNum);
			ns.Write(Utility.ConvertToTimestamp(t.StartDateTime));
			ns.Write(Utility.ConvertToTimestamp(t.EndDateTime));
			ns.Write(t.ConstructType);
			ns.Write(t.StepUpturnType);
			ns.Write(t.StepResetType);
			ns.Write(PickBoardStep);
			ns.Write(RemainCount);
			ns.Write(infos.Count);
			foreach (KeyValuePair<byte, ExchangeItemInfo> info in infos)
			{
				ns.Write(info.Key);
				ns.Write(info.Value.type);
				ns.Write(info.Value.id);
				ns.Write(info.Value.count);
				ns.Write(int.MaxValue);
			}
			_ = last;
		}
	}
}
