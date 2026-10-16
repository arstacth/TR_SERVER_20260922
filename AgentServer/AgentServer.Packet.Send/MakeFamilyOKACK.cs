using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class MakeFamilyOKACK : NetPacket
	{
		public MakeFamilyOKACK(string searchname, List<FamilyInfo> familyinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FAMILY_MAKE_FAMILY_ACK);
			FamilyInfo familyInfo = familyinfos.FirstOrDefault((FamilyInfo f) => f.NickName == searchname);
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
			ns.Write((byte)1);
			ns.Write(familyinfos.Count);
			foreach (FamilyInfo familyinfo in familyinfos)
			{
				ns.WriteAnsiFixed_intSize(familyinfo.NickName);
				ns.Write(familyinfo.CharacterNum);
				ns.Write(familyinfo.familyUnitType);
			}
			_ = last;
		}
	}
}
