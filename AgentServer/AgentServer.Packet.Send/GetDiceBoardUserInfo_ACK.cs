using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetDiceBoardUserInfo_ACK : NetPacket
	{
		public GetDiceBoardUserInfo_ACK(int DiceBoardNum, int Gauge, byte last)
		{
			ns.WriteOP(Opcodes.eServer_TALES_MARBLE_PROTOCOL);
			ns.Write((int)eTalesMarbleProtocol.GET_USERINFO_REQ);
			ns.Write(0);
			ns.Write(DiceBoardNum);
			ns.Write(Gauge);
			_ = last;
		}
	}
}
