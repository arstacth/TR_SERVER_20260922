using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class Myroom_PetRebirth_New : NetPacket
	{
		public Myroom_PetRebirth_New(List<int> petlist, int rebirthitemnum, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MYROOM_ACK);
			ns.WriteOP(eMyRoomProtocol.eMyRoomProtocol_USE_PET_REBIRTH_ACK);
			ns.Write(petlist.Count);
			foreach (int item in petlist)
			{
				ns.Write(item);
			}
			ns.Write(rebirthitemnum);
			_ = last;
		}
	}
}
