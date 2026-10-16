using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_GetUserCharacterItemList : NetPacket
	{
		public Shu_GetUserCharacterItemList(DBShuInfo infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.GET_CHARACTER_ITEM_LIST_ACK);
			ns.Write(0);
			// Thai wire: remainMP + maxMP as two ints (8 bytes — same size as KR int64).
			// int64(remain) became (remain, 0); HUD wants remain/max both populated.
			int remain = 0;
			int maxMp = 20;
			if (infos != null)
			{
				remain = infos.remainMP < 0 ? 0 : infos.remainMP;
				maxMp = infos.maxMP > 0 ? infos.maxMP : 20;
				if (remain > maxMp)
				{
					maxMp = remain;
				}
				if (remain > 20)
				{
					remain = 20;
				}
				if (maxMp > 20)
				{
					maxMp = 20;
				}
			}
			ns.Write(remain);
			ns.Write(maxMp);
			int num = infos != null ? infos.shuitems.Values.Sum((List<ShuItemInfo> s) => s.Count) : 0;
			ns.Write(num);
			if (num > 0 && infos != null)
			{
				foreach (List<ShuItemInfo> value4 in infos.shuitems.Values)
				{
					foreach (ShuItemInfo item in value4)
					{
						ns.Write(item.itemdescnum);
						ns.Write(item.itemID);
						ns.Write(item.gotDateTime);
						ns.Write(item.count < 0 ? 0 : item.count);
						ns.Write(item.state);
					}
				}
				// Client 0x764740: i32 mapCount of (i64,i32) pairs, then char count.
				ns.Write(0);
				List<long> ids = new List<long>();
				foreach (long id in infos.characterItemID)
				{
					if (infos.shuchars.TryGetValue(id, out var ch) && ch != null)
					{
						ids.Add(id);
					}
				}
				// Client closes Shu UI when no char has state=1 (common after unequip).
				bool anySelected = false;
				foreach (long id in ids)
				{
					if (infos.shuchars.TryGetValue(id, out var ch0) && ch0 != null && ch0.state == 1)
					{
						anySelected = true;
						break;
					}
				}
				if (!anySelected && ids.Count > 0 && infos.shuchars.TryGetValue(ids[0], out var first) && first != null)
				{
					first.state = 1;
				}
				ns.Write(ids.Count);
				foreach (long item2 in ids)
				{
					infos.shuchars.TryGetValue(item2, out var value);
					infos.shuavatars.TryGetValue(item2, out var value2);
					infos.shustatus.TryGetValue(item2, out var value3);
					if (value2 == null)
					{
						value2 = new List<ShuAvatarInfo>();
					}
					if (value3 == null)
					{
						value3 = new List<ShuStatusInfo>();
					}
					ShuWire.WriteCharacter(ns, item2, value, value2, value3);
				}
			}
			else
			{
				// TRTHOLD empty-inventory path: one trailing int64 after itemCount=0.
				ns.Write(0L);
			}
			_ = last;
		}
	}
}
