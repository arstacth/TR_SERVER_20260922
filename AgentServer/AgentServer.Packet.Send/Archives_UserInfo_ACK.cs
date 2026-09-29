using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Archives_UserInfo_ACK : NetPacket
	{
		public Archives_UserInfo_ACK(List<ArchiveslInfo> Infos, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ARCHIVES_GET_USERINFO_ACK);
			ns.Write(Infos.Count);
			foreach (ArchiveslInfo Info in Infos)
			{
				ns.Write(Info.Index);
				ns.Write(Info.ItemNum);
				ns.Write(Info.Count);
				ns.Write(Info.NeedCount);
				ns.Write(Info.IsReceived);
			}
			_ = last;
		}
	}
}
