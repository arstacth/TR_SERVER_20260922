using AgentServer.Structuring.Opcode;
using LocalCommons.Network;

namespace AgentServer.Packet.Send
{
	/// <summary>
	/// eServer_RECEIVE_ITEM_ACK (2451). Client accepts result 0 (ok) or 0x4E (fail).
	/// Capsule2 Goodspiece can fire RECEIVE_ITEM for catalog prizes after GetInfo;
	/// reject so UI does not play a fake spin/claim without PickUP.
	/// </summary>
	public sealed class ReceiveItemFailAck : NetPacket
	{
		public ReceiveItemFailAck(byte last)
		{
			_ = last;
			ns.WriteOP(Opcodes.eServer_RECEIVE_ITEM_ACK);
			// result 0x4E = fail. Overpop 6/6/4: client pops a second int after result.
			ns.Write(0x4E);
			ns.Write(0);
		}
	}
}
