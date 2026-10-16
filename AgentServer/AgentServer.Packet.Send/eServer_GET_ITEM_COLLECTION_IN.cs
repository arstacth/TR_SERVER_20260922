using AgentServer.Structuring.Opcode;
using AgentServer.Structuring.User;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	public sealed class eServer_GET_ITEM_COLLECTION_INFO_REQ : NetPacket
	{
		public eServer_GET_ITEM_COLLECTION_INFO_REQ(string UserName, bool bOtherUser, UserItemCollectionInfo info, byte last)
		{
			ns.WriteOP(Opcodes.eServer_GET_ITEM_COLLECTION_INFO_ACK);
			_ = UserName;
			_ = bOtherUser;
			// Packed 683 (trRelease 0x75f3f6) pops int edx then byte r8 into vtable+0x950.
			// Sending point as the int made the book show 148/204 as rank.
			ns.Write(info.rank);
			ns.Write(info.noticedLevel);
			_ = last;
		}
	}
}
