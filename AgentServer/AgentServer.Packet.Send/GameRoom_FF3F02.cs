using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GameRoom_FF3F02 : NetPacket
	{
		public GameRoom_FF3F02(int unk1, int unk2, int unk4, int unk5, byte last)
		{
			ns.WriteOP(Opcodes.eServer_MISSION_USER_MISSION_UPDATE_ACK);
			ns.Write(unk1);
			ns.Write(unk2);
			ns.Write(unk4);
			ns.Write(unk5);
			_ = last;
		}
	}
}
