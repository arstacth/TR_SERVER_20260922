using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;
using LocalCommons.Utilities;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_HatchOK : NetPacket
	{
		public Shu_HatchOK(long eggitemid, int eggItemNum, DBShuInfo infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.HATCH_REQ);
			ns.Write(0);
			ns.Write(eggitemid);
			long newCharId = infos.characterItemID.Count > 0 ? infos.characterItemID[0] : 0L;
			ns.Write(newCharId);
			int num = infos.shuitems.Values.Sum((List<ShuItemInfo> s) => s.Count);
			bool flag = false;
			if (infos.shuitems.TryGetValue(0L, out var value))
			{
				flag = true;
			}
			else
			{
				num++;
			}
			ns.Write(num);
			if (flag)
			{
				int desc = value[0].itemdescnum != 0 ? value[0].itemdescnum : eggItemNum;
				long got = value[0].gotDateTime > 0
					? value[0].gotDateTime
					: Utility.ConvertToTimestamp(System.DateTime.Now);
				int cnt = value[0].count > 0 ? value[0].count : 1;
				ns.Write(desc);
				ns.Write(eggitemid);
				ns.Write(got);
				ns.Write(cnt);
				ns.Write(value[0].state);
			}
			else
			{
				ns.Write(eggItemNum);
				ns.Write(eggitemid);
				ns.Write(Utility.ConvertToTimestamp(System.DateTime.Now));
				ns.Write(1);
				ns.Write(0);
			}
			foreach (KeyValuePair<long, List<ShuItemInfo>> item in infos.shuitems.Where((KeyValuePair<long, List<ShuItemInfo>> w) => w.Key != 0))
			{
				foreach (ShuItemInfo item2 in item.Value)
				{
					ns.Write(item2.itemdescnum);
					ns.Write(item2.itemID);
					ns.Write(item2.gotDateTime);
					ns.Write(item2.count);
					ns.Write(item2.state);
				}
			}
			ns.Write(0);
			ns.Write(infos.characterItemID.Count);
			foreach (long item3 in infos.characterItemID)
			{
				infos.shuchars.TryGetValue(item3, out var value2);
				infos.shuavatars.TryGetValue(item3, out var value3);
				infos.shustatus.TryGetValue(item3, out var value4);
				ShuWire.WriteCharacter(ns, item3, value2, value3, value4);
			}
			_ = last;
		}
	}
}
