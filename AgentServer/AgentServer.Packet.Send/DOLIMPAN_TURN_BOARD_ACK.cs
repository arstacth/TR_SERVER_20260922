using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class DOLIMPAN_TURN_BOARD_ACK : NetPacket
	{
		public DOLIMPAN_TURN_BOARD_ACK(DolimpanHandle.Dolimpan result, int RemainPoint, short DolimpanPoint, byte idx, int iPoint, bool bDoubleBonus, byte last)
		{
			ns.WriteOP(Opcodes.eServer_DOLIMPAN_TURN_BOARD_ACK);
			ns.Write((int)result);
			if (result == DolimpanHandle.Dolimpan.eServerResult_OK_ACK)
			{
				if (bDoubleBonus)
				{
					ns.Write(0);
					ns.Write(RemainPoint);
					ns.Write(DolimpanPoint);
					ns.Write(idx);
				}
				else
				{
					ns.Write(iPoint);
					ns.Write(RemainPoint);
					ns.Write(DolimpanPoint);
					ns.Write(idx);
				}
			}
			_ = last;
		}
	}
}
