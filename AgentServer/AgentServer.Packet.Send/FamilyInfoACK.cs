using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FamilyInfoACK : NetPacket
	{
		public FamilyInfoACK(string searchname, byte flag, List<FamilyInfo> familyinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FAMILY_GET_FAMILY_INFO_ACK);
			FamilyInfo familyInfo = familyinfos.FirstOrDefault((FamilyInfo f) => f.familyUnitType == 3);
			FamilyInfo familyInfo2 = familyinfos.FirstOrDefault((FamilyInfo f) => f.familyUnitType == 0);
			int value = -1;
			if (familyInfo2 != null)
			{
				value = familyInfo2.coupleNum;
			}
			ns.Write(familyInfo.coupleNum);
			ns.Write(familyInfo.coupleType);
			ns.Write(value);
			ns.WriteAnsiFixed_intSize(familyInfo.NickName);
			ns.Write(flag);
			ns.Write(familyinfos.Count);
			foreach (FamilyInfo familyinfo in familyinfos)
			{
				ns.WriteAnsiFixed_intSize(familyinfo.NickName);
				ns.Write(familyinfo.CharacterNum);
				ns.Write(familyinfo.familyUnitType);
			}
		}
	}
}
