using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class FishMyRankingAck : NetPacket
	{
		public FishMyRankingAck(int rank, int point, byte last)
		{
			_ = last;
			_ = rank;
			_ = point;
			ns.WriteOP(Opcodes.eServer_FISHING_MY_RANKING_ACK);
			ns.Write(0);
			// Next int is the row count. Count 0 ends the packet. Rank/point are row fields.
			ns.Write(0);
		}
	}

	public sealed class FishAsyncPointAck : NetPacket
	{
		public FishAsyncPointAck(int point, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_FISHING_MY_ASYNC_FISHING_POINT_ACK);
			ns.Write(0);
			ns.Write(point);
		}
	}
}
