using AgentServer.Network.Connections;
using AgentServer.Packet.Send;
using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet
{
	public static class PieroOlympicHandle
	{
		public static bool TryHandle(ClientConnection client, ushort serverOp, PacketReader reader, byte last)
		{
			switch (serverOp)
			{
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_MY_PIERO_OLYMPIC_INFO_REQ:
				client.SendAsync(new PieroOlympic_GetMyInfo_ACK(last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_SHOP_REQ:
				client.SendAsync(new PieroOlympic_GetShop_ACK(last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_SHOP_BUY_ITEM_REQ:
				if (reader.Remaining >= 4)
				{
					reader.ReadLEInt32();
				}
				client.SendAsync(new PieroOlympic_ShopBuy_ACK(last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_MY_CONTRIBUTION_POINT_REQ:
				client.SendAsync(new PieroOlympic_Point_ACK((ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_MY_CONTRIBUTION_POINT_ACK, last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_TODAY_GROWTHPOINT_REQ:
				client.SendAsync(new PieroOlympic_Point_ACK((ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_TODAY_GROWTHPOINT_ACK, last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_TOTAL_GROWTHPOINT_REQ:
				client.SendAsync(new PieroOlympic_Point_ACK((ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_TOTAL_GROWTHPOINT_ACK, last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_INTERRUPT_GROWTHPOINT_REQ:
				client.SendAsync(new PieroOlympic_Point_ACK((ushort)Opcodes.eServer_PIERO_OLYMPIC_GET_INTERRUPT_GROWTHPOINT_ACK, last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_GIVE_LEVEL_UP_REWARD_REQ:
				client.SendAsync(new PieroOlympic_LevelUpReward_ACK(last));
				return true;
			case (ushort)Opcodes.eServer_PIERO_OLYMPIC_JOIN_PARTY_REQ:
				if (reader.Remaining >= 4)
				{
					reader.ReadLEInt32();
				}
				client.SendAsync(new PieroOlympic_JoinParty_ACK(last));
				return true;
			default:
				return false;
			}
		}
	}
}
