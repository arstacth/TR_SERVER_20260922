using AgentServer.Structuring.Item;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class CubeInfo : NetPacket
	{
		public CubeInfo(int cubeitem, int count, int goldcube_guage, int max_goldcube_guage, eCUBE_INFO_RESULT result, byte last)
		{
			ns.WriteOP(Opcodes.eServer_ITEMCUBE_CUBEINFO_ACK);
			ns.Write(cubeitem);
			ns.Write(count);
			ns.Write(goldcube_guage);
			ns.Write(max_goldcube_guage);
			ns.Write((int)result);
			_ = last;
		}
	}
}
