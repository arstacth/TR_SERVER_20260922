using System.Collections.Generic;
using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MissionGiveReward_ACK : NetPacket
	{
		public MissionGiveReward_ACK(int kind, List<int> missionList, List<ExchangeItemInfo> infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_GIVE_REWARD_ACK);
			ns.Write(missionList.Count);
			foreach (int mission in missionList)
			{
				ns.Write(kind);
				ns.Write(mission);
			}
			ns.Write(infos.Count);
			foreach (ExchangeItemInfo info in infos)
			{
				ns.Write(info.type);
				ns.Write(info.id);
				ns.Write(info.count);
				ns.Write(int.MaxValue);
				// Overpop 78 83 20 — client pops 20 bytes per reward item (5 ints).
				ns.Write(0);
			}
			_ = last;
		}
	}
}
