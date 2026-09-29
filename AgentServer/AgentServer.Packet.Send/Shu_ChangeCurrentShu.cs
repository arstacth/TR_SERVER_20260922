using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Shu;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Shu_ChangeCurrentShu : NetPacket
	{
		public Shu_ChangeCurrentShu(long beforeCharacterItemID, long shuitemid, DBShuInfo infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_SHU_PROTOCOL);
			ns.Write((int)eShuProtocol.CHANGE_CURRENT_SHU_REQ);
			ns.Write(0);
			ns.Write(beforeCharacterItemID);
			ns.Write(shuitemid);
			ns.Write(0);
			if (shuitemid != -1)
			{
				ns.Write(infos.characterItemID.Count);
				foreach (long item in infos.characterItemID)
				{
					infos.shuchars.TryGetValue(item, out var value);
					infos.shuavatars.TryGetValue(item, out var value2);
					infos.shustatus.TryGetValue(item, out var value3);
					ShuWire.WriteCharacter(ns, item, value, value2, value3);
				}
			}
			else
			{
				ns.Write(0);
			}
			_ = last;
		}
	}
}
