using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.Park;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class GetHuMongPickBoardInfo : NetPacket
	{
		public GetHuMongPickBoardInfo(HuMongPickBoardData PickBoardData, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_HUMONG_PICKBOARD_STATE_ACK);
			ns.Write(0);
			ns.Write(PickBoardData.HuMongPickBoardNum);
			bool[] pickInfo = PickBoardData.PickInfo ?? new bool[0];
			ns.Write(pickInfo.Length);
			for (short num = 1; num <= pickInfo.Length; num = (short)(num + 1))
			{
				ns.Write(num);
				ns.Write(pickInfo[num - 1]);
			}
		}

		public GetHuMongPickBoardInfo(eServerResult result, byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_HUMONG_PICKBOARD_STATE_ACK);
			ns.Write((int)result);
		}
	}
}
