using System.Collections.Generic;
using System.Linq;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DissolveFamilyOKACK2 : NetPacket
	{
		public DissolveFamilyOKACK2(string targetNickName, List<FamilyInfo> familyinfos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FAMILY_DISSOLVE_FAMILY_FOR_FRIEND_LIST_UPDATE_NOTIFY);
			FamilyInfo familyInfo = familyinfos.FirstOrDefault((FamilyInfo f) => f.familyUnitType == 4);
			ns.Write(2);
			ns.WriteAnsiFixed_intSize(targetNickName);
			ns.WriteAnsiFixed_intSize(familyInfo.NickName);
			_ = last;
		}
	}
}
