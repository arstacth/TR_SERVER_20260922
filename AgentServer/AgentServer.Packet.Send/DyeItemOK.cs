using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DyeItemOK : NetPacket
	{
		public DyeItemOK(int itemnum, UserItemDyeing info, byte last, int err = 0)
		{
			ns.WriteOP(Opcodes.eServer_ITEM_DYEING_DYE_ITEM_ACK);
			ns.Write(err);
			if (err == 0)
			{
				ns.Write(itemnum);
				ns.Write(info.DyeingPart);
				ns.Write(info.Color1, 0, 3);
				ns.Write(info.Color2, 0, 3);
				ns.Write(info.Color3, 0, 3);
				ns.Write(info.Type);
			}
			_ = last;
		}
	}
}
