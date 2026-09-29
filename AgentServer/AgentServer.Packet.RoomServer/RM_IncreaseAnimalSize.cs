using AgentServer.Structuring;
using LocalCommons.Network;
using NetMsg.Room;

namespace AgentServer.Packet.RoomServer
{
	public sealed class RM_IncreaseAnimalSize : NetPacket
	{
		public RM_IncreaseAnimalSize(Account User, int FarmUniqueNum, int ItemNum, long FarmItemID, byte last)
		{
			ns.WriteOP(RMProtocol.RM_IncreaseAnimalSize_REQ);
			ns.Write(User.Session);
			ns.Write(User.CurrentRoomId);
			ns.Write(FarmUniqueNum);
			ns.Write(ItemNum);
			ns.Write(FarmItemID);
			ns.Write(last);
		}
	}
}
