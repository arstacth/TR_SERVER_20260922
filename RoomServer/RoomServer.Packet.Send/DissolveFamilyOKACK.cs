using System.Collections.Generic;
using System.Linq;
using LocalCommons.Network;
using RoomServer.Structuring.Opcode;
using RoomServer.Structuring.User;

namespace RoomServer.Packet.Send
{
	public sealed class DissolveFamilyOKACK : NetPacket
	{
		public DissolveFamilyOKACK(string searchname, string targetNickName, bool isParents, List<FamilyInfo> familyinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FAMILY_DISSOLVE_FAMILY_ACK);
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
			ns.WriteAnsiFixed_intSize(targetNickName);
			ns.Write(isParents);
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
