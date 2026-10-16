using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class PieroOlympic_GetMyInfo_ACK : NetPacket
	{
		public PieroOlympic_GetMyInfo_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GET_MY_PIERO_OLYMPIC_INFO_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.Write(1);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}

	public sealed class PieroOlympic_GetShop_ACK : NetPacket
	{
		public PieroOlympic_GetShop_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GET_SHOP_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}

	public sealed class PieroOlympic_ShopBuy_ACK : NetPacket
	{
		public PieroOlympic_ShopBuy_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_SHOP_BUY_ITEM_ACK);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}

	public sealed class PieroOlympic_Point_ACK : NetPacket
	{
		public PieroOlympic_Point_ACK(ushort ackOp, byte last)
		{
			ns.WriteOP(ackOp);
			ns.Write(0);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}

	public sealed class PieroOlympic_JoinParty_ACK : NetPacket
	{
		public PieroOlympic_JoinParty_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_JOIN_PARTY_ACK);
			ns.Write(0);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}

	public sealed class PieroOlympic_LevelUpReward_ACK : NetPacket
	{
		public PieroOlympic_LevelUpReward_ACK(byte last)
		{
			ns.WriteOP(Opcodes.eServer_PIERO_OLYMPIC_GIVE_LEVEL_UP_REWARD_ACK);
			ns.Write(0);
			ns.WriteThaiLast(last);
		}
	}
}
