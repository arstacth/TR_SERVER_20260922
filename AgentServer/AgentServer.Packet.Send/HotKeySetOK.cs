using System.Collections.Generic;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class HotKeySetOK : NetPacket
	{
		public HotKeySetOK(List<int> list, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MOTION_QUICKSLOT_UPDATE_ACK);
			ns.Write((short)list.Count);
			foreach (int item in list)
			{
				ns.Write(item);
			}
			_ = last;
		}
	}
}
