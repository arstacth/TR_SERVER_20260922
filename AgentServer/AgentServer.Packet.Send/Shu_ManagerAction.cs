using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ManagerAction : NetPacket
	{
		public Shu_ManagerAction(int actionType, long shuitemid, DBShuActionInfo infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.MANAGER_ACTION_REQ);
			ns.Write(0);
			ns.Write(shuitemid);
			ns.Write(actionType);
			ns.Write(infos.remainMP);
			ns.Write(infos.ActionResult.Count);
			foreach (ShuActionResultInfo item in infos.ActionResult)
			{
				ns.Write(item.statusType);
				ns.Write(item.giveValue);
			}
			ns.Write(infos.shustatus.Count);
			foreach (KeyValuePair<long, List<ShuStatusInfo>> item2 in infos.shustatus)
			{
				ns.Write(item2.Key);
				ShuWire.WriteStatus(ns, item2.Value);
			}
			_ = last;
		}
	}
}
