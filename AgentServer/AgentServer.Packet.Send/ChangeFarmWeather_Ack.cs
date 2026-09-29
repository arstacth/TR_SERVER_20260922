using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class ChangeFarmWeather_Ack : NetPacket
	{
		public ChangeFarmWeather_Ack(int FarmUniqueNum, int weatherType, byte last)
		{
			ns.WriteOP(Opcodes.eServer_FARM_ACK);
			ns.WriteOP(FarmProtocol.ChangeFarmWeather_ACK);
			ns.Write(0);
			ns.Write(FarmUniqueNum);
			ns.Write(weatherType);
			_ = last;
		}
	}
}
